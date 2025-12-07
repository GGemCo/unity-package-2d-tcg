using System.Collections.Generic;
using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 런타임에서 사용되는 덱.
    /// 카드 목록과 셔플/드로우 등의 기본 동작을 제공한다.
    /// </summary>
    public class TcgBattleDataDeck<TCard> where TCard : GGemCo2DTcg.ICardInfo
    {
        /// <summary>
        /// 현재 덱에 포함된 카드 리스트.
        /// </summary>
        public List<TCard> Cards { get; } = new();

        /// <summary>
        /// 셔플 환경 및 전략.
        /// </summary>
        public ShuffleMetaData ShuffleMetaData { get; private set; }

        public TcgBattleDataDeck(ShuffleMetaData shuffleMetaData)
        {
            ShuffleMetaData = shuffleMetaData;
        }

        /// <summary>
        /// 덱 내용을 교체하거나 초기화할 때 사용.
        /// </summary>
        public void SetCards(IEnumerable<TCard> cards)
        {
            Cards.Clear();
            Cards.AddRange(cards);
        }

        /// <summary>
        /// 현재 셔플 모드에 따라 덱을 셔플한다.
        /// </summary>
        public void Shuffle()
        {
            if (Cards.Count <= 1)
                return;

            ShuffleMetaData.Strategy.Shuffle(Cards, ShuffleMetaData);
        }

        /// <summary>
        /// 한 장 드로우. 성공 여부를 bool 로 반환한다.
        /// </summary>
        public bool TryDraw(out TCard card)
        {
            if (Cards.Count == 0)
            {
                card = default;
                return false;
            }

            card = Cards[0];
            Cards.RemoveAt(0);
            return true;
        }
        public int Count => Cards.Count;
# if UNITY_EDITOR
        public void DebugCard()
        {
            if (Cards == null || Cards.Count == 0)
            {
                Debug.Log("[DeckRuntime] 덱에 카드가 없습니다.");
                return;
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"[DeckRuntime] 카드 개수: {Cards.Count}");
            sb.AppendLine("[DeckRuntime] 현재 덱 순서 (위 -> 아래):");

            for (int i = 0; i < Cards.Count; i++)
            {
                var card = Cards[i];

                if (card == null)
                {
                    sb.AppendLine($"{i:D2}: null");
                    continue;
                }

                // CardRuntime 일 때 Uid 까지 출력
                if (card is TcgBattleDataCard runtimeCard)
                {
                    // CardRuntime 내부 필드를 상황에 맞게 추가
                    // 예: runtimeCard.CardInfo, runtimeCard.Cost 등
                    sb.AppendLine(
                        $"{i:D2}: Uid={runtimeCard.Uid}, Cost={runtimeCard.Cost}"
                    );
                }
                else
                {
                    // 그 외 ICardInfo 타입은 기본 정보만 출력
                    sb.AppendLine($"{i:D2}: {card}");
                }
            }

            Debug.Log(sb.ToString());
        }
#endif
    }
}