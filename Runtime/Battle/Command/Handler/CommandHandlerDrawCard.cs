using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    public class CommandHandlerDrawCard : ITcgBattleCommandHandler
    {
        public ConfigCommonTcg.TcgBattleCommandType CommandType =>
            ConfigCommonTcg.TcgBattleCommandType.PlayCardFromHand;
        
        public CommandResult Execute(TcgBattleDataMain context, in TcgBattleCommand cmd)
        {
            var card = cmd.tcgBattleDataCard;
            if (card == null)
                return null;

            var actor = context.GetSideState(cmd.Side);
            var opponent = context.GetOpponentState(cmd.Side);
            int toIndex = 0;
            
            // 마나 차감
            if (!actor.TryConsumeMana(card.Cost))
            {
                // todo. localization. 자원 소모 이름 tcg settings에 추가하기
                // systemMessageManager.ShowMessageError("마나가 부족합니다.");
                GcLogger.LogWarning($"[Battle] ExecutePlayCard: Not enough mana. (Need: {card.Cost}, Have: {actor.Mana.Current})");
                return CommandResult.Fail("Error_Tcg_NotEnoughMana");
            }
            // 손에서 제거
            if (!actor.Hand.TryRemove(card, out int fromIndex)) 
                return CommandResult.Fail("Error_Tcg_NoCardInHand");

            // 카드 타입에 따라 분기 (예시)
            switch (card.Type)
            {
                case CardConstants.Type.Creature:
                {
                    // 1) 유닛 런타임 생성
                    var unit = TcgBattleDataCardFactory.CreateBattleDataFieldCard(actor.Side, card);
                    if (unit != null)
                    {
                        toIndex = actor.Board.Add(unit);
                    }

                    // 2) "소환 시 발동" 이펙트가 있다면 실행
                    if (card.SummonEffects != null && card.SummonEffects.Count > 0)
                    {
                        AbilityRunner.RunAbility(
                            actor,
                            opponent,
                            card,
                            card.SummonEffects,
                            explicitTargetBattleData: null /* 필요 시 타겟 전달 */);
                    }
                    break;
                }

                case CardConstants.Type.Spell:
                {
                    // 스펠은 필드에 남지 않고, 이펙트만 실행
                    if (card.SpellEffects != null && card.SpellEffects.Count > 0)
                    {
                        // TODO: TargetType 에 따라 타겟 선택 로직 추가
                        AbilityRunner.RunAbility(
                            actor,
                            opponent,
                            card,
                            card.SpellEffects,
                            explicitTargetBattleData: null);
                    }
                    break;
                }

                default:
                {
                    // 기타 타입(장비/영속물 등)은 추후 확장
                    break;
                }
            }
            return CommandResult.OkPresentation(new[]
            {
                new TcgPresentationStep(
                    TcgPresentationStepType.MoveCardHandToBoard,
                    cmd.Side,
                    attacker: actor,
                    target: opponent,
                    fromIndex: fromIndex,
                    toIndex: toIndex,
                    valueA: actor.Board.Count)
            });

        }
    }
}