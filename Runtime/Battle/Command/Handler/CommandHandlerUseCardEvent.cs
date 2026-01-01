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
    public sealed class CommandHandlerUseCardEvent : CommandHandlerBase, ITcgBattleCommandHandler
    {
        public ConfigCommonTcg.TcgBattleCommandType CommandType =>
            ConfigCommonTcg.TcgBattleCommandType.UseCardEvent;

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

            var ability = BuildOnPlayAbilityDefinition(card);
            if (ability.IsValid && ability.tcgAbilityTriggerType == TcgAbilityConstants.TcgAbilityTriggerType.OnPlay)
            {
                if (RequiresExplicitTarget(ability.tcgAbilityTargetType))
                {
                    var explicitTarget = ResolveExplicitTarget(ability.tcgAbilityTargetType, actor, opponent, target,
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

            if (card.EventDetail == null)
                return steps.Count > 0 ? CommandResult.OkPresentation(steps.ToArray()) : CommandResult.Ok();

            // 1) 이벤트 등록
            var inst = new TcgBattleEventInstance(card, card.EventDetail);
            actor.Events.Add(inst);

            // 2) UI 연출 타임라인
            // - consumeOnTrigger: 스펠처럼 "시전 → 임팩트 → 소모"로 표현
            // - not consume: "등록" 연출 후(필요 시) 임팩트를 실행

            if (card.EventDetail.consumeOnTrigger)
            {
                // 캐스팅 연출(타겟이 없어도 허용)
                var explicitTarget = ability.IsValid
                    ? ResolveExplicitTarget(ability.tcgAbilityTargetType, actor, opponent, target, targetZone)
                    : null;

                steps.Add(new TcgPresentationStep(
                    TcgPresentationConstants.TcgPresentationStepType.MoveCardToTarget,
                    cmd.Side,
                    fromZone: attackerZone,
                    fromIndex: fromIndex,
                    toIndex: explicitTarget != null ? target.Index : -1,
                    toZone: explicitTarget != null ? targetZone : ConfigCommonTcg.TcgZone.None));

                TryRunOnPlayAbility(context, cmd.Side, attackerZone, fromIndex, targetZone, target.Index, ability, steps);

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
                // 등록 연출(필드/보드로 표시). 실제 UI에서 별도 영역이 필요하면 StepType 확장 권장.
                steps.Add(new TcgPresentationStep(
                    TcgPresentationConstants.TcgPresentationStepType.MoveCardToField,
                    cmd.Side,
                    fromZone: ConfigCommonTcg.TcgZone.HandPlayer,
                    fromIndex: attacker.Index,
                    toZone: ConfigCommonTcg.TcgZone.FieldPlayer,
                    toIndex: target.Index));

                if (card.EventDetail.tcgAbilityTriggerType == TcgAbilityConstants.TcgAbilityTriggerType.OnPlay)
                {
                    var explicitTarget = ability.IsValid
                        ? ResolveExplicitTarget(ability.tcgAbilityTargetType, actor, opponent, target, targetZone)
                        : null;
                    
                    TryRunOnPlayAbility(context, cmd.Side, attackerZone, fromIndex, targetZone, target.Index, ability, steps);
                }
            }

            return steps.Count > 0 ? CommandResult.OkPresentation(steps.ToArray()) : CommandResult.Ok();
        }
    }
}
