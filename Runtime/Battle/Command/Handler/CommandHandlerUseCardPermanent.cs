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
    public sealed class CommandHandlerUseCardPermanent : CommandHandlerBase, ITcgBattleCommandHandler
    {
        public ConfigCommonTcg.TcgBattleCommandType CommandType =>
            ConfigCommonTcg.TcgBattleCommandType.UseCardPermanent;

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

            var targetCardIndex = 0;
            var targetCardWindowUId = UIWindowConstants.WindowUid.None;

            var actor = context.GetSideState(cmd.Side);
            var opponent = context.GetOpponentState(cmd.Side);

            var ability = BuildOnPlayAbilityDefinition(card);
            if (ability.IsValid && ability.tcgAbilityTriggerType == TcgAbilityConstants.TcgAbilityTriggerType.OnPlay)
            {
                if (RequiresExplicitTarget(ability.tcgAbilityTargetType))
                {
                    var explicitTarget = ResolveExplicitTarget(ability.tcgAbilityTargetType, actor, opponent, target, targetZone);
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

            // Permanent는 지속 영역에 등록 (필드/보드와 별개로 관리)
            if (card.PermanentDetail != null)
            {
                var inst = new TcgBattlePermanentInstance(card, card.PermanentDetail);
                actor.Permanents.Add(inst);
            }

            // 1) 공통: 캐스팅/투사체 연출
            // - 효과(피해/회복/버프 등)는 Ability 기반 Step에서 처리합니다.
            steps.Add(new TcgPresentationStep(
                TcgPresentationConstants.TcgPresentationStepType.MoveCardToTarget,
                cmd.Side,
                fromZone: attackerZone,
                fromIndex: attacker.Index,
                toZone: targetZone,
                toIndex: target.Index));

            // 2) OnPlay Ability 임팩트 (등록 이후 실행되는 타임라인)
            if (card.PermanentDetail != null &&
                card.PermanentDetail.tcgAbilityTriggerType == TcgAbilityConstants.TcgAbilityTriggerType.OnPlay)
            {
                var explicitTarget = ability.IsValid
                    ? ResolveExplicitTarget(ability.tcgAbilityTargetType, actor, opponent, target, targetZone)
                    : null;
                TryRunOnPlayAbility(context, cmd.Side, attackerZone, fromIndex, targetZone, target.Index, ability, steps);
            }

            return steps.Count > 0 ? CommandResult.OkPresentation(steps.ToArray()) : CommandResult.Ok();
        }

    }
}
