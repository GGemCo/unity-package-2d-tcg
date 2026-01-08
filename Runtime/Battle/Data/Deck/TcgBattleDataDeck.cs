using System;
using System.Collections.Generic;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 전투 런타임에서 사용하는 덱(Deck) 컨테이너입니다.
    /// 카드 목록을 보관하며, 셔플/드로우 등 기본 동작을 제공합니다.
    /// </summary>
    /// <typeparam name="TCard">덱에 담길 카드 타입입니다. <see cref="ICardInfo"/>를 구현해야 합니다.</typeparam>
    public class TcgBattleDataDeck<TCard> where TCard : ICardInfo
    {
        /// <summary>
        /// 현재 덱에 포함된 카드 목록입니다.
        /// </summary>
        public List<TCard> Cards { get; } = new();

        /// <summary>
        /// 히어로 카드(또는 대표 카드) 참조입니다.
        /// 게임 규칙에 따라 덱과 별도로 취급되는 카드를 보관합니다.
        /// </summary>
        public TCard HeroCard { get; set; }

        /// <summary>
        /// 셔플 환경(메타 데이터) 및 셔플 전략(Strategy) 설정입니다.
        /// </summary>
        public ShuffleMetaData ShuffleMetaData { get; private set; }

        /// <summary>
        /// 덱을 생성합니다.
        /// </summary>
        /// <param name="shuffleMetaData">셔플에 사용할 메타 데이터 및 전략 정보입니다.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="shuffleMetaData"/>가 null인 경우(계약을 강제하고 싶을 때) 발생할 수 있습니다.
        /// </exception>
        public TcgBattleDataDeck(ShuffleMetaData shuffleMetaData)
        {
            ShuffleMetaData = shuffleMetaData;
        }

        /// <summary>
        /// 덱의 카드 목록을 통째로 교체(초기화)합니다.
        /// </summary>
        /// <param name="cards">새로 설정할 카드 시퀀스입니다.</param>
        /// <remarks>
        /// 기존 <see cref="Cards"/>를 비운 뒤 <paramref name="cards"/>를 순서대로 추가합니다.
        /// </remarks>
        public void SetCards(IEnumerable<TCard> cards)
        {
            Cards.Clear();
            Cards.AddRange(cards);
        }

        /// <summary>
        /// 히어로 카드를 설정합니다.
        /// </summary>
        /// <param name="heroCard">설정할 히어로 카드입니다. null이면 무시합니다.</param>
        public void SetHeroCard(TCard heroCard)
        {
            if (heroCard == null) return;
            HeroCard = heroCard;
        }

        /// <summary>
        /// 현재 <see cref="ShuffleMetaData"/>에 설정된 전략에 따라 덱을 셔플합니다.
        /// </summary>
        /// <remarks>
        /// 카드가 0~1장이면 셔플해도 변화가 없으므로 아무 작업도 하지 않습니다.
        /// </remarks>
        /// <exception cref="NullReferenceException">
        /// <see cref="ShuffleMetaData"/> 또는 그 내부의 Strategy가 null인 경우 발생할 수 있습니다.
        /// (호출부에서 유효한 값이 설정된다는 전제를 둘 수도 있습니다.)
        /// </exception>
        public void Shuffle()
        {
            if (Cards.Count <= 1)
                return;

            ShuffleMetaData.Strategy.Shuffle(Cards, ShuffleMetaData);
        }

        /// <summary>
        /// 덱 최상단(인덱스 0)의 카드를 한 장 뽑아 반환합니다.
        /// </summary>
        /// <returns>뽑은 카드입니다. 덱이 비어 있으면 default를 반환합니다.</returns>
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
        /// 덱 최상단 카드를 한 장 드로우 시도합니다.
        /// </summary>
        /// <param name="card">성공 시 뽑은 카드가 설정되며, 실패 시 default가 설정됩니다.</param>
        /// <returns>드로우에 성공하면 true, 덱이 비어 있으면 false입니다.</returns>
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

        /// <summary>
        /// 현재 덱에 남아 있는 카드 수입니다.
        /// </summary>
        public int Count => Cards.Count;

        /// <summary>
        /// 덱이 비어 있는지 여부입니다.
        /// </summary>
        public bool IsEmpty => Cards.Count == 0;
    }
}
