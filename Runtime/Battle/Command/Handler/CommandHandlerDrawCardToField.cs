using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 손패의 카드를 비용(마나)을 지불하고 필드로 이동(플레이/소환)하는 커맨드를 처리합니다.
    /// </summary>
    /// <remarks>
    /// 처리 흐름:
    /// <list type="number">
    /// <item><description>마나를 차감합니다.</description></item>
    /// <item><description>손패에서 카드를 제거합니다.</description></item>
    /// <item><description>카드 타입에 따라 필드 카드 데이터를 생성하고 필드에 배치합니다.</description></item>
    /// <item><description>UI 연출을 위한 <see cref="TcgPresentationStep"/>을 생성하여 반환합니다.</description></item>
    /// </list>
    /// 현재 구현은 <see cref="CardConstants.Type.Creature"/>만 지원합니다.
    /// </remarks>
    public sealed class CommandHandlerDrawCardToField : CommandHandlerBase, ITcgBattleCommandHandler
    {
        /// <summary>
        /// 이 핸들러가 처리하는 커맨드 타입입니다.
        /// </summary>
        public ConfigCommonTcg.TcgBattleCommandType CommandType =>
            ConfigCommonTcg.TcgBattleCommandType.DrawCardToField;

        /// <summary>
        /// 손패 카드를 필드로 플레이하고, 필요한 경우 UI 연출용 <see cref="TcgPresentationStep"/>들을 반환합니다.
        /// </summary>
        /// <param name="context">현재 전투 상태 컨텍스트입니다.</param>
        /// <param name="cmd">실행할 전투 커맨드 데이터입니다.</param>
        /// <returns>
        /// 처리 결과입니다. 연출이 필요한 경우 <see cref="CommandResult.OkPresentation"/> 형태로 Step을 포함할 수 있습니다.
        /// </returns>
        public CommandResult Execute(TcgBattleDataMain context, in TcgBattleCommand cmd)
        {
            if (context == null)
                return CommandResult.Fail("Error_Tcg_InvalidContext");

            var fromZone = cmd.attackerZone;
            var toZone = cmd.targetZone;

            var card = cmd.attackerBattleDataCardInHand;
            if (card == null)
                return CommandResult.Fail("Error_Tcg_NoCard");

            var actor = context.GetSideState(cmd.Side);
            var opponent = context.GetOpponentState(cmd.Side); // NOTE: 현재 로직에서는 미사용(추후 효과/검증 확장 대비)

            // 1) 마나 차감
            if (!actor.TryConsumeMana(card.Cost))
                return CommandResult.Fail("Error_Tcg_NotEnoughMana");

            // 2) 손패에서 제거
            if (!actor.Hand.TryRemove(card, out int fromIndex))
                return CommandResult.Fail("Error_Tcg_NoCardInHand");

            var steps = new List<TcgPresentationStep>(6);

            // 3) 카드 타입에 따른 처리
            switch (card.Type)
            {
                case CardConstants.Type.Creature:
                {
                    // 손패 카드 정의로부터 필드 유닛 데이터를 생성합니다.
                    var unit = TcgBattleDataCardFactory.CreateBattleDataFieldCard(actor.Side, card);
                    if (unit == null)
                        return CommandResult.Fail("Error_Tcg_CreateUnitFailed");

                    // 필드에 배치하고, 배치된 슬롯 인덱스를 확보합니다.
                    int toIndex = actor.Field.Add(unit);

                    // UI 연출: 손패 → 필드 이동
                    steps.Add(new TcgPresentationStep(
                        TcgPresentationConstants.TcgPresentationStepType.MoveCardToField,
                        cmd.Side,
                        fromZone: fromZone,
                        fromIndex: fromIndex,
                        toZone: toZone,
                        toIndex: toIndex));
                    break;
                }

                default:
                {
                    // 지원하지 않는 타입(예: Spell 등)은 현재 핸들러에서 처리하지 않습니다.
                    GcLogger.LogError($"{card.Type} 타입은 {nameof(CommandHandlerDrawCardToField)} 에서 처리할 수 없습니다.");
                    break;
                }
            }

            return steps.Count > 0
                ? CommandResult.OkPresentation(steps.ToArray())
                : CommandResult.Ok();
        }
    }
}
