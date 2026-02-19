using System.Collections.Generic;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 초/중/후반(Phase) 구간별 코스트 가중치를 반영하여 덱 앞부분을 재배열하는 셔플 전략입니다.
    /// </summary>
    /// <remarks>
    /// <para>
    /// 이 전략은 “최대 마나에 도달하기 전까지 드로우될 카드 구간”(<c>FrontLoadedCount</c>)에만
    /// 페이즈별 코스트 가중치를 적용하고, 그 이후 구간은 완전 랜덤 셔플 결과를 유지합니다.
    /// </para>
    /// <para><b>동작 순서</b></para>
    /// <list type="number">
    ///   <item><description>전체 덱을 Fisher–Yates 방식으로 완전 랜덤 셔플합니다.</description></item>
    ///   <item><description>덱 앞쪽 <c>FrontLoadedCount</c> 장만 다시 재배열합니다(초/중/후반 분할 후, 각 구간을 가중 랜덤 재배치).</description></item>
    ///   <item><description><c>FrontLoadedCount</c> 이후의 카드는 1단계 셔플 결과(완전 랜덤 순서)를 그대로 유지합니다.</description></item>
    /// </list>
    /// <para><b>결과</b></para>
    /// <para>
    /// 초반 드로우될 카드들은 페이즈별 코스트 분포 의도를 반영한 순서로 배치되며,
    /// 중후반 이후 카드들은 편향 없이 랜덤성이 보존됩니다.
    /// </para>
    /// </remarks>
    public class ShuffleStrategyPhaseWeighted : IShuffleStrategy
    {
        /// <summary>
        /// 전달된 덱을 제자리(in-place)에서 셔플합니다.
        /// </summary>
        /// <typeparam name="TCard">셔플 대상 카드 타입입니다.</typeparam>
        /// <param name="cards">셔플 대상 카드 리스트입니다. 호출 후 순서가 변경됩니다.</param>
        /// <param name="metaData">
        /// 셔플에 필요한 메타데이터입니다.
        /// <para>- <c>Config</c>: FrontLoadedCount 및 페이즈 가중치 설정</para>
        /// <para>- <c>SeedManager</c>: 셔플 랜덤 시드 적용</para>
        /// </param>
        /// <remarks>
        /// <para>
        /// 입력이 유효하지 않으면(리스트가 null/길이 1 이하, Config/SeedManager 없음) 아무 동작도 하지 않습니다.
        /// </para>
        /// </remarks>
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
        /// 지정된 구간을 가중치 기반 무작위 순서로 재배열합니다.
        /// </summary>
        /// <typeparam name="TCard">셔플 대상 카드 타입입니다.</typeparam>
        /// <param name="cards">전체 덱 리스트입니다.</param>
        /// <param name="start">재배열을 시작할 인덱스(포함)입니다.</param>
        /// <param name="length">재배열할 카드 수입니다.</param>
        /// <param name="weightSelector">카드로부터 가중치 값을 선택하는 함수입니다.</param>
        /// <remarks>
        /// <para>
        /// 구간은 <c>[start, start + length)</c>이며, 구간 밖의 카드 순서는 변경하지 않습니다.
        /// </para>
        /// <para>
        /// 내부적으로 구간을 복사한 뒤 <see cref="WeightedReorder{TCard}"/> 결과로 다시 덮어씁니다.
        /// </para>
        /// </remarks>
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
        /// 리스트 전체를 “가중치 기반 랜덤 순서”로 재배치한 새 리스트를 생성합니다.
        /// </summary>
        /// <typeparam name="TCard">재배치 대상 카드 타입입니다.</typeparam>
        /// <param name="source">재배치할 원본 리스트입니다.</param>
        /// <param name="weightSelector">카드별 가중치를 반환하는 함수입니다.</param>
        /// <returns>가중 무작위 선택을 반복하여 생성된 재배치 결과 리스트입니다.</returns>
        /// <remarks>
        /// <para>
        /// 구현 방식은 “남은 카드 집합에서 가중치 룰렛 선택을 반복”하는 형태입니다.
        /// 즉, 각 단계에서 <c>weightSelector</c>가 반환한 가중치에 비례하여 1장을 뽑아 결과에 추가하고,
        /// 뽑힌 카드는 후보에서 제거합니다.
        /// </para>
        /// <para>
        /// 가중치가 0 이하인 카드는 선택 대상에서 제외되며,
        /// 남은 카드의 가중치 합이 0 이하가 되는 순간(= 모두 0 이하) 이후에는
        /// 남은 카드들을 현재 순서 그대로 결과에 추가합니다.
        /// </para>
        /// <para>
        /// NOTE: 난수는 <see cref="Random.Range(float, float)"/>를 사용하므로,
        /// 외부에서 시드를 동일하게 적용하면 재현 가능한 결과를 얻을 수 있습니다(환경/플랫폼 의존성은 Unity 규칙을 따름).
        /// </para>
        /// </remarks>
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
                    if (float.IsNaN(w) || float.IsInfinity(w) || w <= 0f)
                        continue;
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

                int pickedIndex = -1;
                for (int i = 0; i < working.Count; i++)
                {
                    float w = weightSelector(working[i]);
                    if (float.IsNaN(w) || float.IsInfinity(w) || w <= 0f)
                        continue;

                    acc += w;
                    if (acc >= r)
                    {
                        pickedIndex = i;
                        break;
                    }
                }

                // 누적 오차/비정상 값으로 선택되지 않는 경우를 대비하여 fallback 합니다.
                if (pickedIndex < 0)
                {
                    for (int i = working.Count - 1; i >= 0; i--)
                    {
                        float w = weightSelector(working[i]);
                        if (float.IsNaN(w) || float.IsInfinity(w) || w <= 0f)
                            continue;

                        pickedIndex = i;
                        break;
                    }

                    if (pickedIndex < 0)
                    {
                        // 이론상 불가(totalWeight > 0 조건)지만, 안전을 위해 남은 순서를 그대로 사용합니다.
                        result.AddRange(working);
                        break;
                    }
                }

                result.Add(working[pickedIndex]);
                working.RemoveAt(pickedIndex);
            }

            return result;
        }
    }
}
