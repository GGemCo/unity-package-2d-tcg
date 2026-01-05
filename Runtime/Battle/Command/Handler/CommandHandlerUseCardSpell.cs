using System.Collections.Generic;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 스펠 카드 사용(캐스팅) 커맨드를 처리합니다.
    /// 마나를 차감하고 손에서 카드를 제거한 뒤, OnPlay 능력을 실행하고 최종적으로 무덤으로 이동시키는 연출 Step을 생성합니다.
    /// </summary>
    public sealed class CommandHandlerUseCardSpell : CommandHandlerBase, ITcgBattleCommandHandler
    {
        /// <summary>
        /// 이 핸들러가 처리하는 전투 커맨드 타입입니다.
        /// </summary>
        public ConfigCommonTcg.TcgBattleCommandType CommandType =>
            ConfigCommonTcg.TcgBattleCommandType.UseCardSpell;

        /// <summary>
        /// 스펠 카드 사용 커맨드를 실행합니다.
        /// </summary>
        /// <param name="context">전투 진행에 필요한 메인 컨텍스트 데이터입니다.</param>
        /// <param name="cmd">실행할 전투 커맨드(시전자/대상/카드/존 정보 포함)입니다.</param>
        /// <returns>실행 결과(실패 사유 또는 연출 Step 포함 성공 결과)를 반환합니다.</returns>
        public CommandResult Execute(TcgBattleDataMain context, in TcgBattleCommand cmd)
        {
            if (context == null)
                return CommandResult.Fail("Error_Tcg_InvalidContext");

            // 커맨드에서 필요한 참조를 로컬 변수로 분리(가독성/실수 방지 목적)
            var attackerZone = cmd.attackerZone;
            var attacker = cmd.attackerBattleDataCardInField;
            var targetZone = cmd.targetZone;
            var target = cmd.targetBattleDataCardInField;

            // 스펠은 손의 카드가 반드시 필요합니다.
            var card = cmd.attackerBattleDataCardInHand;
            if (card == null)
                return CommandResult.Fail("Error_Tcg_NoCard");

            var actor = context.GetSideState(cmd.Side);
            var opponent = context.GetOpponentState(cmd.Side);

            // OnPlay 능력 정의를 생성하고, 명시적 타겟이 필요한 경우 선검증합니다.
            var ability = BuildOnPlayAbilityDefinition(card);
            TcgBattleDataCardInField explicitTarget = null;
            if (ability.IsValid && ability.tcgAbilityTriggerType == TcgAbilityConstants.TcgAbilityTriggerType.OnPlay)
            {
                if (RequiresExplicitTarget(ability.tcgAbilityTargetType))
                {
                    explicitTarget = ResolveExplicitTarget(
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

            // 2) 손에서 제거(연출에서 사용할 hand index 확보)
            if (!actor.Hand.TryRemove(card, out int fromIndex))
                return CommandResult.Fail("Error_Tcg_NoCardInHand");

            var steps = new List<TcgPresentationStep>(6);

            // 스펠은 즉시 능력 실행 후 소모되므로, 타겟이 아직 없고 능력이 유효하면 여기서도 타겟 해석을 시도합니다.
            // (명시적 타겟이 필수가 아닌 Ability라도 자동 타겟이 필요한 케이스를 지원)
            if (explicitTarget == null && ability.IsValid)
            {
                explicitTarget = ResolveExplicitTarget(
                    ability.tcgAbilityTargetType,
                    actor,
                    opponent,
                    target,
                    targetZone);
            }

            // 1) 공통: 캐스팅/투사체 연출
            // - 실제 효과(피해/회복/버프 등)는 Ability 기반 Step에서 처리합니다.
            steps.Add(new TcgPresentationStep(
                TcgPresentationConstants.TcgPresentationStepType.MoveCardToTarget,
                cmd.Side,
                fromZone: attackerZone,
                fromIndex: fromIndex,
                toIndex: explicitTarget != null ? target.Index : -1,
                toZone: explicitTarget != null ? targetZone : ConfigCommonTcg.TcgZone.None));

            // 2) Ability 임팩트(AbilityType별 연출 Step 자동 추가)
            TryRunOnPlayAbility(
                context,
                cmd.Side,
                attackerZone,
                fromIndex,
                targetZone,
                target.Index,
                ability,
                steps);

            // 3) 후처리: 소모(Grave)
            steps.Add(new TcgPresentationStep(
                TcgPresentationConstants.TcgPresentationStepType.MoveCardToGrave,
                cmd.Side,
                fromZone: attackerZone,
                fromIndex: fromIndex,
                toIndex: -1,
                toZone: ConfigCommonTcg.TcgZone.None));

            return steps.Count > 0 ? CommandResult.OkPresentation(steps.ToArray()) : CommandResult.Ok();
        }
    }
}
