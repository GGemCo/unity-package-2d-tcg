using System.Collections.Generic;

namespace GGemCo2DTcg
{
    public class ShuffleStrategyNone : IShuffleStrategy
    {
        public void Shuffle<TCard>(List<TCard> cards, ShuffleMetaData metaData) where TCard : ICardInfo
        {
        }
    }
}
