using System.Collections.Generic;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 덱 셔플 전략 인터페이스.
    /// 구현체에서 Fisher–Yates, Weighted 등 다양한 방식을 제공한다.
    /// </summary>
    public interface IShuffleStrategy
    {
        /// <summary>
        /// 전달된 카드 리스트를 제자리(in-place) 셔플한다.
        /// </summary>
        void Shuffle<TCard>(List<TCard> cards, ShuffleMetaData metaData) where TCard : GGemCo2DTcg.ICardInfo;
    }
}