using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    public sealed class CommandHandlerDrawCardToField : CommandHandlerBase, ITcgBattleCommandHandler
    {
        public ConfigCommonTcg.TcgBattleCommandType CommandType =>
            ConfigCommonTcg.TcgBattleCommandType.DrawCardToField;

        public CommandResult Execute(TcgBattleDataMain context, in TcgBattleCommand cmd)
        {
            if (context == null)
                return CommandResult.Fail("Error_Tcg_InvalidContext");

            var attackerZone = cmd.attackerZone;
            var targetZone = cmd.targetZone;
            var card = cmd.attackerBattleDataCardInHand;
            if (card == null)
                return CommandResult.Fail("Error_Tcg_NoCard");

            var actor = context.GetSideState(cmd.Side);
            var opponent = context.GetOpponentState(cmd.Side);

            // 1) 마나 차감
            if (!actor.TryConsumeMana(card.Cost))
                return CommandResult.Fail("Error_Tcg_NotEnoughMana");

            // 2) 손에서 제거
            if (!actor.Hand.TryRemove(card, out int fromIndex))
                return CommandResult.Fail("Error_Tcg_NoCardInHand");

            var steps = new List<TcgPresentationStep>(6);

            // 3) 카드 타입에 따른 처리
            switch (card.Type)
            {
                case CardConstants.Type.Creature:
                {
                    var unit = TcgBattleDataCardFactory.CreateBattleDataFieldCard(actor.Side, card);
                    if (unit == null)
                        return CommandResult.Fail("Error_Tcg_CreateUnitFailed");

                    int toIndex = actor.Field.Add(unit);

                    steps.Add(new TcgPresentationStep(
                        TcgPresentationConstants.TcgPresentationStepType.MoveCardToField,
                        cmd.Side,
                        fromZone: attackerZone,
                        fromIndex: fromIndex,
                        toZone: targetZone,
                        toIndex: toIndex));
                    break;
                }

                default:
                {
                    GcLogger.LogError($"{card.Type} 타입은 {nameof(CommandHandlerDrawCardToField)} 에서 처리할 수 없습니다.");
                    break;
                }
            }

            return steps.Count > 0 ? CommandResult.OkPresentation(steps.ToArray()) : CommandResult.Ok();
        }
    }
}
