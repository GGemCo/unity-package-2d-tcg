using System.Collections.Generic;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 초/중/후반 구간별 코스트 가중치를 적용하는 셔플 전략.
    /// 
    /// 동작 순서:
    /// 1. 전체 덱을 Fisher–Yates 로 완전 랜덤 셔플한다.
    /// 2. 덱 앞쪽 Config.FrontLoadedCount 장만 다시 재배열한다.
    ///    - 이 영역을 초/중/후반 구간으로 나누고
    ///    - 각 구간에 대해 코스트 가중치 기반 랜덤 순서를 생성한다.
    /// 3. FrontLoadedCount 이후의 카드는 1번에서 셔플된 순서를 그대로 유지한다.
    /// 
    /// 결과적으로:
    /// - "최대 마나를 얻기 전까지 드로우될 카드들"은
    ///   초/중/후반 코스트 가중치가 반영된 순서로 배치되고,
    /// - 그 이후 카드들은 완전 랜덤 순서가 유지된다.
    /// </summary>
    public class ShuffleStrategyPhaseWeighted : IShuffleStrategy
    {
        public void Shuffle<TCard>(List<TCard> cards, ShuffleMetaData metaData)
            where TCard : ICardInfo
        {
            if (cards == null || cards.Count <= 1)
                return;

            if (metaData == null || metaData.Config == null || metaData.SeedManager == null)
                return;

            var config = metaData.Config;

            // 1차: 전체를 완전 랜덤 셔플 (Fisher–Yates)
            metaData.SeedManager.ApplySeed();

            int count = cards.Count;
            for (int i = 0; i < count; i++)
            {
                int randomIndex = Random.Range(i, count);
                (cards[i], cards[randomIndex]) = (cards[randomIndex], cards[i]);
            }

            // 가중 셔플 대상 카드 수 (최대 마나 이전 드로우 구간)
            int frontCount = Mathf.Clamp(config.FrontLoadedCount, 0, count);
            if (frontCount <= 1)
                return;

            // 초/중/후반 구간 카드 수 계산
            config.CalculatePhaseCounts(frontCount,
                out int earlyCount,
                out int midCount,
                out int lateCount);

            int earlyStart = 0;
            int midStart = earlyStart + earlyCount;
            int lateStart = midStart + midCount;

            // 안전장치: 구간 합이 frontCount 를 넘지 않도록 보정
            int totalPhaseCount = earlyCount + midCount + lateCount;
            if (totalPhaseCount > frontCount)
            {
                int overflow = totalPhaseCount - frontCount;
                int reduceLate = Mathf.Min(lateCount, overflow);
                lateCount -= reduceLate;
                overflow -= reduceLate;
                int reduceMid = Mathf.Min(midCount, overflow);
                midCount -= reduceMid;
                overflow -= reduceMid;
                earlyCount = Mathf.Max(0, earlyCount - overflow);
            }

            // 실제 가중 재배열 수행
            if (earlyCount > 1)
            {
                ReorderRange(cards, earlyStart, earlyCount,
                    c => config.EarlyPhaseWeights.GetWeight(c.Cost));
            }

            if (midCount > 1)
            {
                ReorderRange(cards, midStart, midCount,
                    c => config.MidPhaseWeights.GetWeight(c.Cost));
            }

            if (lateCount > 1)
            {
                ReorderRange(cards, lateStart, lateCount,
                    c => config.LatePhaseWeights.GetWeight(c.Cost));
            }
        }

        /// <summary>
        /// 지정된 구간 [start, start+length) 에 대해
        /// weightSelector 를 기반으로 가중 무작위 순서를 생성합니다.
        /// </summary>
        private void ReorderRange<TCard>(
            List<TCard> cards,
            int start,
            int length,
            System.Func<TCard, float> weightSelector)
            where TCard : ICardInfo
        {
            var temp = new List<TCard>(length);
            for (int i = 0; i < length; i++)
                temp.Add(cards[start + i]);

            var reordered = WeightedReorder(temp, weightSelector);

            for (int i = 0; i < length; i++)
                cards[start + i] = reordered[i];
        }

        /// <summary>
        /// 리스트 전체를 "가중치 기반 랜덤 순서"로 재배치합니다.
        /// </summary>
        private List<TCard> WeightedReorder<TCard>(
            List<TCard> source,
            System.Func<TCard, float> weightSelector)
            where TCard : ICardInfo
        {
            var working = new List<TCard>(source);
            var result = new List<TCard>(source.Count);

            while (working.Count > 0)
            {
                float totalWeight = 0f;
                for (int i = 0; i < working.Count; i++)
                {
                    float w = weightSelector(working[i]);
                    if (w > 0f)
                        totalWeight += w;
                }

                if (totalWeight <= 0f)
                {
                    // 모든 weight 가 0 이하이면 남은 순서는 그대로
                    result.AddRange(working);
                    break;
                }

                float r = Random.Range(0f, totalWeight);
                float acc = 0f;

                for (int i = 0; i < working.Count; i++)
                {
                    float w = weightSelector(working[i]);
                    if (w <= 0f)
                        continue;

                    acc += w;
                    if (acc >= r)
                    {
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
