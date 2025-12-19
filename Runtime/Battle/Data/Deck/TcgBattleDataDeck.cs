using System.Collections.Generic;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 런타임에서 사용되는 덱.
    /// 카드 목록과 셔플/드로우 등의 기본 동작을 제공한다.
    /// </summary>
    public class TcgBattleDataDeck<TCard> where TCard : ICardInfo
    {
        /// <summary>
        /// 현재 덱에 포함된 카드 리스트.
        /// </summary>
        public List<TCard> Cards { get; } = new();
        public TCard HeroCard { get; set; }

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
        public void SetHeroCard(TCard heroCard)
        {
            if (heroCard == null) return;
            HeroCard = heroCard;
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

        public TCard DrawTop()
        {
            TCard card;
            if (Cards.Count == 0)
            {
                card = default;
                return card;
            }

            card = Cards[0];
            Cards.RemoveAt(0);
            return card;
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
        public bool IsEmpty => Cards.Count == 0;
    }
}