using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 손패의 이벤트(Event) 카드를 사용(Play)하는 커맨드를 처리합니다.
    /// </summary>
    /// <remarks>
    /// 처리 흐름:
    /// <list type="number">
    /// <item><description>마나를 차감합니다.</description></item>
    /// <item><description>손패에서 카드를 제거합니다.</description></item>
    /// <item><description>이벤트 인스턴스를 생성하여 플레이어의 이벤트 목록에 등록합니다.</description></item>
    /// <item><description><c>consumeOnTrigger</c> 설정에 따라 즉시 시전/소모 또는 등록 상태 유지로 분기합니다.</description></item>
    /// <item><description>UI 연출은 <see cref="TcgPresentationStep"/> 타임라인으로 반환합니다.</description></item>
    /// </list>
    /// </remarks>
    public sealed class CommandHandlerUseCardEvent : CommandHandlerBase, ITcgBattleCommandHandler
    {
        /// <summary>
        /// 이 핸들러가 처리하는 커맨드 타입입니다.
        /// </summary>
        public ConfigCommonTcg.TcgBattleCommandType CommandType =>
            ConfigCommonTcg.TcgBattleCommandType.UseCardEvent;

        /// <summary>
        /// 이벤트 카드 사용 커맨드를 실행하고, 필요한 경우 UI 연출용 <see cref="TcgPresentationStep"/>들을 반환합니다.
        /// </summary>
        /// <param name="context">현재 전투 상태 컨텍스트입니다.</param>
        /// <param name="cmd">실행할 전투 커맨드 데이터입니다.</param>
        /// <returns>
        /// 처리 결과입니다. 연출이 필요한 경우 <see cref="CommandResult.OkPresentation"/> 형태로 Step을 포함할 수 있습니다.
        /// </returns>
        /// <remarks>
        /// Ability가 명시적 타겟을 요구하는 경우 타겟이 해석되지 않으면 실패를 반환합니다.
        /// </remarks>
        public CommandResult Execute(TcgBattleDataMain context, in TcgBattleCommand cmd)
        {
            if (context == null)
                return CommandResult.Fail("Error_Tcg_InvalidContext");

            var attackerZone = cmd.attackerZone;
            var attacker = cmd.attackerBattleDataCardInField; // NOTE: 일부 분기에서 인덱스에 사용됨(널 가능성 주의)
            var targetZone = cmd.targetZone;
            var target = cmd.targetBattleDataCardInField;

            var card = cmd.attackerBattleDataCardInHand;
            if (card == null)
                return CommandResult.Fail("Error_Tcg_NoCard");

            var actor = context.GetSideState(cmd.Side);
            var opponent = context.GetOpponentState(cmd.Side);

            // 카드 정의로부터 OnPlay Ability를 빌드합니다(없을 수도 있음).
            var ability = TcgAbilityBuilder.BuildOnPlayAbilityDefinition(card);

            // Ability가 명시적 타겟을 요구하는 경우, 커맨드로 전달된 대상이 유효한지 검증/해석합니다.
            if (ability.IsValid && ability.tcgAbilityTriggerType == TcgAbilityConstants.TcgAbilityTriggerType.OnPlay)
            {
                if (TcgBattleCommand.RequiresExplicitTarget(ability.tcgAbilityTargetType))
                {
                    var explicitTarget = TcgBattleCommand.ResolveExplicitTarget(
                        ability.tcgAbilityTargetType,
                        actor,
                        opponent,
                        target,
                        targetZone);

                    if (explicitTarget == null)
                        return CommandResult.Fail("Error_Tcg_TargetRequired");
                }
            }

            // 1) 마나 차감
            if (!actor.TryConsumeMana(card.Cost))
                return CommandResult.Fail("Error_Tcg_NotEnoughMana");

            // 2) 손에서 제거
            if (!actor.Hand.TryRemove(card, out int fromIndex))
                return CommandResult.Fail("Error_Tcg_NoCardInHand");

            var steps = new List<TcgPresentationStep>(6);

            // 이벤트 상세가 없으면(테이블 누락 등) 등록/연출을 진행할 수 없으므로 종료합니다.
            if (card.EventDetail == null)
                return steps.Count > 0 ? CommandResult.OkPresentation(steps.ToArray()) : CommandResult.Ok();

            // 3) 이벤트 등록(도메인 상태)
            var inst = new TcgBattleEventInstance(card, card.EventDetail);
            actor.Events.Add(inst);

            // 4) UI 연출 타임라인
            // - consumeOnTrigger=true : 스펠처럼 "시전 → 임팩트 → 소모"로 표현
            // - consumeOnTrigger=false: "등록" 연출 후(필요 시) 임팩트를 실행하며, 이벤트는 유지됩니다.
            if (card.EventDetail.consumeOnTrigger)
            {
                // 캐스팅 연출(타겟이 없어도 허용)
                var explicitTarget = ability.IsValid
                    ? TcgBattleCommand.ResolveExplicitTarget(ability.tcgAbilityTargetType, actor, opponent, target, targetZone)
                    : null;

                steps.Add(new TcgPresentationStep(
                    TcgPresentationConstants.TcgPresentationStepType.MoveCardToTarget,
                    cmd.Side,
                    fromZone: attackerZone,
                    fromIndex: fromIndex,
                    toIndex: explicitTarget != null ? target.Index : -1,
                    toZone: explicitTarget != null ? targetZone : ConfigCommonTcg.TcgZone.None));

                // 도메인: OnPlay Ability 실행 + (발행된 이벤트를) Step으로 변환하여 동일 타임라인에 누적
                // NOTE: explicitTarget을 계산했지만 target.Index를 전달합니다.
                //       explicitTarget과 target이 달라질 수 있다면 explicitTarget.Index를 사용하도록 정합성 개선이 필요합니다.
                TcgAbilityRunner.TryRunOnPlayAbility(
                    context,
                    cmd.Side,
                    attackerZone,
                    fromIndex,
                    targetZone,
                    target.Index,
                    ability,
                    steps);

                // 즉시 소모 이벤트는 등록 해제 후 무덤으로 이동 연출을 추가합니다.
                actor.Events.Remove(inst);

                steps.Add(new TcgPresentationStep(
                    TcgPresentationConstants.TcgPresentationStepType.MoveCardToGrave,
                    cmd.Side,
                    fromZone: attackerZone,
                    fromIndex: fromIndex,
                    toIndex: -1,
                    toZone: ConfigCommonTcg.TcgZone.None));
            }
            else
            {
                // 등록 연출(필드/보드로 표시)
                // NOTE: 여기서는 HandPlayer/FieldPlayer로 Zone을 하드코딩하고 attacker.Index/target.Index를 사용합니다.
                //       - attacker/target이 null일 수 있는 커맨드라면 NRE 위험이 있습니다.
                //       - 이벤트 전용 Zone이 필요하다면 StepType/Zone 확장이 권장됩니다.
                steps.Add(new TcgPresentationStep(
                    TcgPresentationConstants.TcgPresentationStepType.MoveCardToField,
                    cmd.Side,
                    fromZone: ConfigCommonTcg.TcgZone.HandPlayer,
                    fromIndex: attacker.Index,
                    toZone: ConfigCommonTcg.TcgZone.FieldPlayer,
                    toIndex: target.Index));

                // 등록형 이벤트라도 OnPlay 트리거를 가진 경우 즉시 1회 실행을 허용합니다(설정에 따른 정책).
                if (card.EventDetail.TcgAbilityTriggerType == TcgAbilityConstants.TcgAbilityTriggerType.OnPlay)
                {
                    var explicitTarget = ability.IsValid
                        ? TcgBattleCommand.ResolveExplicitTarget(ability.tcgAbilityTargetType, actor, opponent, target, targetZone)
                        : null;

                    // NOTE: explicitTarget 변수를 계산하지만 현재 분기에서는 직접 사용하지 않습니다(연출/정합성 필요 시 반영).
                    TcgAbilityRunner.TryRunOnPlayAbility(
                        context,
                        cmd.Side,
                        attackerZone,
                        fromIndex,
                        targetZone,
                        target.Index,
                        ability,
                        steps);
                }
            }

            return steps.Count > 0
                ? CommandResult.OkPresentation(steps.ToArray())
                : CommandResult.Ok();
        }
    }
}
