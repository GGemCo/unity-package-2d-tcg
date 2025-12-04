using System.Collections.Generic;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// Fisher–Yates 알고리즘 기반의 완전 랜덤 셔플.
    /// </summary>
    public class PureRandomShuffleStrategy : IShuffleStrategy
    {
        public void Shuffle<TCard>(List<TCard> cards, ShuffleMetaData metaData)
            where TCard : GGemCo2DTcg.ICardInfo
        {
            if (cards == null || cards.Count <= 1)
                return;

            // 시드 적용 (SeedManager 를 통해 일원화)
            metaData.SeedManager.ApplySeed();

            int count = cards.Count;
            for (int i = 0; i < count; i++)
            {
                int randomIndex = Random.Range(i, count);
                (cards[i], cards[randomIndex]) = (cards[randomIndex], cards[i]);
            }
        }
    }
}