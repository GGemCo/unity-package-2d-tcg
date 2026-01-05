using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 손패에서 카드를 사용(Play)하는 커맨드를 처리합니다.
    /// - 코스트 소모
    /// - 손패 제거
    /// - 카드 타입에 따른 처리(Creature 소환, Spell/Equipment 즉시 실행, Permanent/Event 등록 등)
    /// </summary>
    public sealed class CommandHandlerUseCardEquipment : CommandHandlerBase, ITcgBattleCommandHandler
    {
        public ConfigCommonTcg.TcgBattleCommandType CommandType =>
            ConfigCommonTcg.TcgBattleCommandType.UseCardEquipment;

        public CommandResult Execute(TcgBattleDataMain context, in TcgBattleCommand cmd)
        {
            if (context == null)
                return CommandResult.Fail("Error_Tcg_InvalidContext");
            
            var attackerZone = cmd.attackerZone;
            var attacker = cmd.attackerBattleDataCardInField;
            var targetZone = cmd.targetZone;
            var target   = cmd.targetBattleDataCardInField;
            
            var card = cmd.attackerBattleDataCardInHand;
            if (card == null)
                return CommandResult.Fail("Error_Tcg_NoCard");
            
            var actor = context.GetSideState(cmd.Side);
            var opponent = context.GetOpponentState(cmd.Side);
            
            var ability = TcgAbilityBuilder.BuildOnPlayAbilityDefinition(card);
            TcgBattleDataCardInField explicitTarget = null;
            if (ability.IsValid && ability.tcgAbilityTriggerType == TcgAbilityConstants.TcgAbilityTriggerType.OnPlay)
            {
                if (TcgBattleCommand.RequiresExplicitTarget(ability.tcgAbilityTargetType))
                {
                    explicitTarget = TcgBattleCommand.ResolveExplicitTarget(ability.tcgAbilityTargetType, actor, opponent, target, targetZone);
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
            
            // 스펠은 즉시 능력 실행 후 소모
            if (explicitTarget == null && ability.IsValid)
            {
                explicitTarget = TcgBattleCommand.ResolveExplicitTarget(
                    ability.tcgAbilityTargetType,
                    actor,
                    opponent,
                    target,
                    targetZone);
            }
            
            // 1) 공통: 캐스팅/투사체 연출
            // - 효과(피해/회복/버프 등)는 Ability 기반 Step에서 처리합니다.
            steps.Add(new TcgPresentationStep(
                TcgPresentationConstants.TcgPresentationStepType.MoveCardToTarget,
                cmd.Side,
                fromZone: attackerZone,
                fromIndex: fromIndex,
                toIndex: explicitTarget != null ? target.Index : -1,
                toZone: explicitTarget != null ? targetZone : ConfigCommonTcg.TcgZone.None));
            
            TcgAbilityRunner.TryRunOnPlayAbility(context, cmd.Side, attackerZone, fromIndex, targetZone, target.Index, ability, steps);
            
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
