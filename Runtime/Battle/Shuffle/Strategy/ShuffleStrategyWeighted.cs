using System.Collections.Generic;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 덱 전체를 완전 랜덤 셔플한 뒤,
    /// 덱 앞부분 일부(<c>FrontLoadedCount</c>)만 코스트 가중치 기반으로 재배열하는 셔플 전략입니다.
    /// </summary>
    /// <remarks>
    /// <para><b>동작 개요</b></para>
    /// <list type="number">
    ///   <item><description>전체 덱을 Fisher–Yates로 1차 셔플합니다.</description></item>
    ///   <item><description>덱 앞쪽 <c>FrontLoadedCount</c>장만 추출하여 코스트 가중치 룰렛 방식으로 재배열합니다.</description></item>
    ///   <item><description>재배열된 결과를 덱 앞부분에 다시 덮어씁니다(나머지 카드는 1차 셔플 결과 유지).</description></item>
    /// </list>
    /// <para>
    /// 이 전략은 “초반에 드로우될 카드들”의 코스트 분포를 의도에 맞게 조정하면서,
    /// 전체 덱의 랜덤성은 가능한 한 보존하는 것을 목표로 합니다.
    /// </para>
    /// </remarks>
    public class ShuffleStrategyWeighted : IShuffleStrategy
    {
        /// <summary>
        /// 전달된 카드 리스트를 제자리(in-place)에서 셔플합니다.
        /// </summary>
        /// <typeparam name="TCard">셔플 대상 카드 타입입니다.</typeparam>
        /// <param name="cards">셔플할 카드 리스트입니다. 호출 후 순서가 변경됩니다.</param>
        /// <param name="context">
        /// 셔플 메타데이터입니다.
        /// <para>- <c>SeedManager</c>: 난수 시드 적용</para>
        /// <para>- <c>Config</c>: <c>FrontLoadedCount</c> 및 코스트 가중치 조회(<c>GetCostWeight</c>)</para>
        /// </param>
        /// <remarks>
        /// <para>
        /// 카드 리스트가 <c>null</c>이거나 카드 수가 1 이하인 경우 셔플을 수행하지 않습니다.
        /// </para>
        /// <para>
        /// <c>FrontLoadedCount</c>가 1 이하이면 2차(가중 재배열) 단계는 생략됩니다.
        /// </para>
        /// </remarks>
        public void Shuffle<TCard>(List<TCard> cards, ShuffleMetaData context)
            where TCard : ICardInfo
        {
            if (cards == null || cards.Count <= 1)
                return;

            // 시드 적용 (SeedManager 를 통해 일원화)
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

        /// <summary>
        /// 전달된 리스트를 코스트 가중치에 비례하는 확률로 1장씩 선택하여
        /// 새로운 “가중 랜덤 순서” 리스트를 생성합니다.
        /// </summary>
        /// <typeparam name="TCard">재배치 대상 카드 타입입니다.</typeparam>
        /// <param name="source">재배치할 원본 리스트입니다.</param>
        /// <param name="context">코스트 가중치 조회에 필요한 셔플 메타데이터입니다.</param>
        /// <returns>가중치 룰렛 선택을 반복하여 생성된 재배치 결과 리스트입니다.</returns>
        /// <remarks>
        /// <para>
        /// 동작 방식은 다음과 같습니다:
        /// 남은 후보(working)에서 각 카드의 <c>GetCostWeight(Cost)</c> 합을 구한 뒤,
        /// <c>[0, totalWeight)</c> 범위에서 난수를 뽑아 누적합 기준으로 1장을 선택합니다.
        /// 선택된 카드는 후보에서 제거되고 결과에 추가됩니다.
        /// </para>
        /// <para>
        /// NOTE: 이 구현은 <c>totalWeight</c>가 0인 경우를 별도로 처리하지 않습니다.
        /// 따라서 설정 상 모든 가중치가 0이 될 수 있다면(또는 음수 포함),
        /// 호출부/설정에서 <c>totalWeight &gt; 0</c>이 되도록 보장하는 것을 전제로 합니다.
        /// </para>
        /// <para>
        /// 성능 특성상 매 선택마다 합산/제거를 수행하므로 리스트 크기가 커질수록 비용이 증가합니다.
        /// (일반적인 덱 크기에서는 문제가 되지 않을 수 있습니다.)
        /// </para>
        /// </remarks>
        private List<TCard> WeightedReorder<TCard>(List<TCard> source, ShuffleMetaData context)
            where TCard : ICardInfo
        {
            var working = new List<TCard>(source);
            var result = new List<TCard>(source.Count);

            while (working.Count > 0)
            {
                float totalWeight = 0f;

                // GetCostWeight() 호출 비용을 줄이고, totalWeight 검증/분포 안정성을 확보하기 위해 weight를 캐시합니다.
                var weights = new float[working.Count];
                for (int i = 0; i < working.Count; i++)
                {
                    float w = context.Config.GetCostWeight(working[i].Cost);

                    // NaN / Infinity 는 0으로 취급(루프/선택 안전성)
                    if (float.IsNaN(w) || float.IsInfinity(w) || w <= 0f)
                        w = 0f;

                    weights[i] = w;
                    totalWeight += w;
                }

                // 모든 weight가 0(또는 비정상)인 경우: 가중치 재배열을 포기하고 균등 랜덤으로 1장 선택합니다.
                // (이 단계는 1차 Fisher–Yates 셔플 이후에 수행되므로, 결과는 여전히 랜덤성을 유지합니다.)
                if (float.IsNaN(totalWeight) || float.IsInfinity(totalWeight) || totalWeight <= 0f)
                {
                    int pick = Random.Range(0, working.Count);
                    result.Add(working[pick]);
                    working.RemoveAt(pick);
                    continue;
                }

                float r = Random.Range(0f, totalWeight);
                float acc = 0f;

                int pickedIndex = -1;
                for (int i = 0; i < working.Count; i++)
                {
                    float w = weights[i];
                    if (w <= 0f)
                        continue;

                    acc += w;
                    if (acc >= r)
                    {
                        pickedIndex = i;
                        break;
                    }
                }

                // 부동소수 누적 오차로 인해 선택되지 않는 경우를 대비하여 마지막 유효 항목으로 fallback 합니다.
                if (pickedIndex < 0)
                {
                    for (int i = working.Count - 1; i >= 0; i--)
                    {
                        if (weights[i] > 0f)
                        {
                            pickedIndex = i;
                            break;
                        }
                    }

                    // 그래도 없으면(이론상 불가): 균등 랜덤
                    if (pickedIndex < 0)
                        pickedIndex = Random.Range(0, working.Count);
                }

                result.Add(working[pickedIndex]);
                working.RemoveAt(pickedIndex);
            }

            return result;
        }
    }
}
