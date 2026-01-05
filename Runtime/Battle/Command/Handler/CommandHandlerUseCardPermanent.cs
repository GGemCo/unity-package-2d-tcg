using System.Collections.Generic;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 손패에서 카드를 사용(Play)하는 커맨드를 처리합니다.
    /// - 코스트 소모
    /// - 손패 제거
    /// - 카드 타입에 따른 처리(Creature 소환, Spell/Equipment 즉시 실행, Permanent/Event 등록 등)
    /// </summary>
    public sealed class CommandHandlerUseCardPermanent : CommandHandlerBase, ITcgBattleCommandHandler
    {
        public ConfigCommonTcg.TcgBattleCommandType CommandType =>
            ConfigCommonTcg.TcgBattleCommandType.UseCardPermanent;

        public CommandResult Execute(TcgBattleDataMain context, in TcgBattleCommand cmd)
        {
            if (context == null)
                return CommandResult.Fail("Error_Tcg_InvalidContext");
            
            var attackerZone = cmd.attackerZone;
            var attacker = cmd.attackerBattleDataCardInHand;
            var targetZone = ConfigCommonTcg.TcgZone.None;
            var target = -1;
            
            if (attacker == null)
                return CommandResult.Fail("Error_Tcg_NoCard");
            if (attacker.PermanentDetail == null)
                return CommandResult.Fail("Error_Tcg_NoPermanentDetail");
            
            var actor = context.GetSideState(cmd.Side);
            var opponent = context.GetOpponentState(cmd.Side);

            // 1) 마나 차감
            if (!actor.TryConsumeMana(attacker.Cost))
                return CommandResult.Fail("Error_Tcg_NotEnoughMana");
            
            // 2) 손에서 제거
            if (!actor.Hand.TryRemove(attacker, out int fromIndex))
                return CommandResult.Fail("Error_Tcg_NoCardInHand");
            
            var steps = new List<TcgPresentationStep>(6);
            
            // Permanent는 지속 영역에 등록 (필드/보드와 별개로 관리)
            var inst = new TcgBattlePermanentInstance(attacker, attacker.PermanentDetail, attackerZone);
            actor.Permanents.Add(inst);

            // Lifetime 초기화 훅
            inst.Lifetime?.OnAdded(new TcgPermanentLifetimeContext(
                context.TurnCount,
                TcgAbilityConstants.TcgAbilityTriggerType.None,
                inst));

            if (attacker.PermanentDetail.tcgAbilityTriggerType == TcgAbilityConstants.TcgAbilityTriggerType.OnPlay)
            {
                var ability = TcgAbilityBuilder.BuildAbility(attacker.PermanentDetail);
                TcgBattleDataCardInField explicitTarget = null; 
                if (TcgBattleCommand.RequiresExplicitTarget(ability.tcgAbilityTargetType))
                {
                    explicitTarget = TcgBattleCommand.ResolveRandomTarget(ability.tcgAbilityTargetType, actor, opponent, includeHero: true);
                    if (explicitTarget == null)
                        return CommandResult.Fail("Error_Tcg_TargetRequired");
                }
                TcgAbilityRunner.TryRunOnPlayAbility(context, cmd.Side, attackerZone, fromIndex, targetZone, explicitTarget?.Index ?? -1, ability, steps);
            }
            
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
