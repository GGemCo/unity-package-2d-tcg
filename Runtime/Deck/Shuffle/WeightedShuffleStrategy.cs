using System.Collections.Generic;
using UnityEngine;

namespace GGemCo2DTcg
{
    public class WeightedShuffleStrategy : IShuffleStrategy
    {
        public void Shuffle<TCard>(List<TCard> cards, ShuffleMetaData context) where TCard : ICardInfo
        {
            if (cards == null || cards.Count <= 1)
                return;

            context.SeedManager.ApplySeed();

            int count = cards.Count;

            // 1차: 기본 Fisher–Yates
            for (int i = 0; i < count; i++)
            {
                int randomIndex = Random.Range(i, count);
                (cards[i], cards[randomIndex]) = (cards[randomIndex], cards[i]);
            }

            // 2차: 앞쪽 일부를 가중 확률로 재배치
            int frontCount = Mathf.Clamp(context.Config.FrontLoadedCount, 0, count);
            if (frontCount <= 1)
                return;

            // 앞 N장을 임시 리스트로 복사
            var tempList = new List<TCard>();
            for (int i = 0; i < frontCount; i++)
                tempList.Add(cards[i]);

            // 가중치 기반으로 tempList를 재배열
            var reordered = WeightedReorder(tempList, context);

            // 덱 앞부분에 다시 넣기
            for (int i = 0; i < frontCount; i++)
                cards[i] = reordered[i];
        }

        // =============================================
        // 가중치 기반 순서 생성
        // =============================================
        private List<TCard> WeightedReorder<TCard>(List<TCard> source, ShuffleMetaData context)
            where TCard : ICardInfo
        {
            List<TCard> working = new List<TCard>(source);
            List<TCard> result = new List<TCard>();

            while (working.Count > 0)
            {
                float totalWeight = 0f;

                // 전체 weight 합
                foreach (var c in working)
                    totalWeight += context.Config.GetCostWeight(c.Cost);

                // 랜덤 픽
                float r = Random.Range(0, totalWeight);
                float acc = 0f;

                for (int i = 0; i < working.Count; i++)
                {
                    acc += context.Config.GetCostWeight(working[i].Cost);
                    if (acc >= r)
                    {
                        // 뽑힌 카드 추가
                        result.Add(working[i]);
                        working.RemoveAt(i);
                        break;
                    }
                }
            }

            return result;
        }
    }
}
