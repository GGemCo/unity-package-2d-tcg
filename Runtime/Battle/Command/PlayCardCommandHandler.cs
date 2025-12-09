using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    public class PlayCardCommandHandler : ITcgBattleCommandHandler
    {
        public ConfigCommonTcg.TcgBattleCommandType CommandType =>
            ConfigCommonTcg.TcgBattleCommandType.PlayCardFromHand;
        
        public void Execute(TcgBattleDataMain context, in TcgBattleCommand cmd)
        {
            var card = cmd.tcgBattleDataCard;
            if (card == null)
                return;

            var actor = context.GetSideState(cmd.Side);
            var opponent = context.GetOpponentState(cmd.Side);
            
            // 마나 차감
            if (!actor.TryConsumeMana(card.Cost)) return;
            // 손에서 제거
            if (!actor.RemoveCardFromHand(card)) return;

            // 카드 타입에 따라 분기 (예시)
            switch (card.Type)
            {
                case CardConstants.Type.Creature:
                {
                    // 1) 유닛 런타임 생성
                    var unit = CreateUnitFromCard(actor.Side, card);
                    if (unit != null)
                    {
                        actor.AddUnitToBoard(unit);
                    }

                    // 2) "소환 시 발동" 이펙트가 있다면 실행
                    if (card.SummonEffects != null && card.SummonEffects.Count > 0)
                    {
                        EffectRunner.RunEffects(
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
                        EffectRunner.RunEffects(
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
        }
        /// <summary>
        /// Creature 타입 카드를 기반으로 필드에 소환할 유닛 런타임을 생성합니다.
        /// - 실제 스탯/키워드는 카드 테이블/런타임에서 가져와야 합니다.
        /// </summary>
        public TcgBattleDataFieldCard CreateUnitFromCard(
            ConfigCommonTcg.TcgPlayerSide ownerSide,
            TcgBattleDataCard tcgBattleDataCard)
        {
            if (tcgBattleDataCard == null)
            {
                GcLogger.LogError("[Battle] CreateUnitFromCard: cardRuntime is null.");
                return null;
            }

            // 1) CardRuntime 에서 스탯/키워드 정보 가져오기
            //    (아래는 예시. 실제 필드 이름에 맞게 수정 필요)
            int attack = tcgBattleDataCard.Attack; // 예: CardRuntime.Attack
            int hp     = tcgBattleDataCard.Health; // 예: CardRuntime.Health

            // 키워드 예시: CardRuntime.Keywords 또는 테이블에서 변환
            List<ConfigCommonTcg.TcgKeyword> keywords = new List<ConfigCommonTcg.TcgKeyword>(4);
            foreach (var kw in tcgBattleDataCard.Keywords) // 예: IEnumerable<TcgKeyword>
            {
                keywords.Add(kw);
            }

            // 2) 유닛 런타임 생성
            var unit = new TcgBattleDataFieldCard(
                tcgBattleDataCard.Uid,
                ownerSide,
                tcgBattleDataCard,
                attack,
                hp,
                keywords);

            // 소환 시점에는 공격 불가 (돌진 키워드가 있으면 예외)
            if (unit.HasKeyword(ConfigCommonTcg.TcgKeyword.Rush))
                unit.CanAttack = true;
            else
                unit.CanAttack = false;

            return unit;
        }
    }
}