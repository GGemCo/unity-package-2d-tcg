using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 손패의 장비(Equipment) 카드를 사용(Play)하는 커맨드를 처리합니다.
    /// </summary>
    /// <remarks>
    /// 처리 흐름:
    /// <list type="number">
    /// <item><description>마나를 차감합니다.</description></item>
    /// <item><description>손패에서 카드를 제거합니다.</description></item>
    /// <item><description>카드의 OnPlay Ability(있는 경우)를 빌드하고, 필요 시 명시적 타겟을 해석합니다.</description></item>
    /// <item><description>UI 연출 Step(이동/시전/효과/무덤 이동)을 생성하여 반환합니다.</description></item>
    /// </list>
    /// 이 핸들러는 Ability 효과(피해/버프 등) 자체를 직접 처리하지 않고,
    /// <see cref="TcgAbilityRunner.TryRunOnPlayAbility"/>를 통해 도메인 Ability 실행 및 연출 Step 생성을 위임합니다.
    /// </remarks>
    public sealed class CommandHandlerUseCardEquipment : CommandHandlerBase, ITcgBattleCommandHandler
    {
        /// <summary>
        /// 이 핸들러가 처리하는 커맨드 타입입니다.
        /// </summary>
        public ConfigCommonTcg.TcgBattleCommandType CommandType =>
            ConfigCommonTcg.TcgBattleCommandType.UseCardEquipment;

        /// <summary>
        /// 장비 카드 사용 커맨드를 실행하고, 필요한 경우 UI 연출용 <see cref="TcgPresentationStep"/>들을 반환합니다.
        /// </summary>
        /// <param name="context">현재 전투 상태 컨텍스트입니다.</param>
        /// <param name="cmd">실행할 전투 커맨드 데이터입니다.</param>
        /// <returns>
        /// 처리 결과입니다. 연출이 필요한 경우 <see cref="CommandResult.OkPresentation"/> 형태로 Step을 포함할 수 있습니다.
        /// </returns>
        /// <remarks>
        /// Ability가 명시적 타겟을 요구하는 경우, 타겟이 해석되지 않으면 실패를 반환합니다.
        /// </remarks>
        public CommandResult Execute(TcgBattleDataMain context, in TcgBattleCommand cmd)
        {
            if (context == null)
                return CommandResult.Fail("Error_Tcg_InvalidContext");

            // NOTE: 변수명은 cmd 구조(공격자/대상)와 맞추되, 이 커맨드는 "손패 카드 사용"이 핵심입니다.
            var attackerZone = cmd.attackerZone;
            var attacker = cmd.attackerBattleDataCardInField; // NOTE: 현재 로직에서는 미사용(확장 대비)
            var targetZone = cmd.targetZone;
            var target = cmd.targetBattleDataCardInField;

            var card = cmd.attackerBattleDataCardInHand;
            if (card == null)
                return CommandResult.Fail("Error_Tcg_NoCard");

            var actor = context.GetSideState(cmd.Side);
            var opponent = context.GetOpponentState(cmd.Side);

            // 카드 정의로부터 OnPlay Ability를 빌드합니다(없을 수도 있음).
            var ability = TcgAbilityBuilder.BuildOnPlayAbilityDefinition(card);

            // Ability가 명시적 타겟을 요구할 때, 커맨드로 전달된 대상이 유효한지 검증/해석합니다.
            TcgBattleDataCardInField explicitTarget = null;
            if (ability.IsValid && ability.tcgAbilityTriggerType == TcgAbilityConstants.TcgAbilityTriggerType.OnPlay)
            {
                if (TcgBattleCommand.RequiresExplicitTarget(ability.tcgAbilityTargetType))
                {
                    explicitTarget = TcgBattleCommand.ResolveExplicitTarget(
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

            // 명시적 타겟이 요구되지 않더라도, Ability가 유효하면 가능한 타겟을 해석해 둡니다(연출/실행에 사용될 수 있음).
            if (explicitTarget == null && ability.IsValid)
            {
                explicitTarget = TcgBattleCommand.ResolveExplicitTarget(
                    ability.tcgAbilityTargetType,
                    actor,
                    opponent,
                    target,
                    targetZone);
            }

            // 3) 공통 연출: 손패 카드의 시전/투사체 이동(타겟이 없으면 None/-1로 처리)
            // - 실제 효과(피해/회복/버프 등)는 Ability 기반 Step에서 처리합니다.
            if (target != null)
            {
                steps.Add(new TcgPresentationStep(
                    TcgPresentationConstants.TcgPresentationStepType.MoveCardToTarget,
                    cmd.Side,
                    fromZone: attackerZone,
                    fromIndex: fromIndex,
                    toIndex: target.Index,
                    toZone: targetZone));
            }

            // 4) 도메인: OnPlay Ability 실행 + (발행된 AbilityPresentationEvent를) Step으로 변환하여 동일 타임라인에 누적
            TcgAbilityRunner.TryRunOnPlayAbility(
                context,
                cmd.Side,
                attackerZone,
                fromIndex,
                targetZone,
                explicitTarget?.Index ?? -1,
                ability,
                steps);

            // 5) 사용한 카드는 소모되어 무덤으로 이동합니다.
            steps.Add(new TcgPresentationStep(
                TcgPresentationConstants.TcgPresentationStepType.MoveCardToGrave,
                cmd.Side,
                fromZone: attackerZone,
                fromIndex: fromIndex,
                toIndex: -1,
                toZone: ConfigCommonTcg.TcgZone.None));

            return steps.Count > 0
                ? CommandResult.OkPresentation(steps.ToArray())
                : CommandResult.Ok();
        }
    }
}
