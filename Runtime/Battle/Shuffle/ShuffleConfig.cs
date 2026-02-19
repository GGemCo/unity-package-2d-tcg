using System.Collections.Generic;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 셔플 전략이 참고하는 설정 값(가중치/구간/비율)을 모아 둔 구성 클래스입니다.
    /// </summary>
    /// <remarks>
    /// <para>
    /// 이 설정은 셔플 알고리즘 자체를 구현하지 않으며,
    /// 셔플 전략(예: Weighted, PhaseWeighted)이 확률/구간 규칙을 적용할 때 사용하는 “입력 데이터”입니다.
    /// </para>
    /// <para><b>포함 내용</b></para>
    /// <list type="bullet">
    ///   <item><description>코스트별 공통 가중치(단일 테이블)</description></item>
    ///   <item><description>가중 셔플을 적용할 덱 앞부분 카드 수(<see cref="FrontLoadedCount"/>)</description></item>
    ///   <item><description>FrontLoadedCount 구간의 초/중/후반 비율(<see cref="EarlyPhaseRatio"/>, <see cref="MidPhaseRatio"/>)</description></item>
    ///   <item><description>초/중/후반 구간별 코스트 가중치(<see cref="PhaseCostWeights"/>)</description></item>
    /// </list>
    /// </remarks>
    public class ShuffleConfig
    {
        // --------------------------------------------------------------------
        // 1) 공통 코스트 가중치
        // --------------------------------------------------------------------

        /// <summary>
        /// 어떤 코스트에도 별도 설정이 없을 때 적용되는 공통 기본 가중치입니다.
        /// </summary>
        /// <remarks>
        /// 기본값은 <c>1.0</c>이며, 설정이 없는 코스트를 동일한 비중으로 취급합니다.
        /// </remarks>
        public float DefaultCostWeight { get; set; } = 1f;

        /// <summary>
        /// 코스트별 공통 가중치 테이블입니다.
        /// </summary>
        /// <remarks>
        /// <para>key: 코스트, value: 가중치</para>
        /// <para>
        /// 테이블에 존재하지 않는 코스트는 <see cref="DefaultCostWeight"/>로 취급합니다.
        /// </para>
        /// </remarks>
        public Dictionary<int, float> CostWeights { get; } = new();

        /// <summary>
        /// 지정된 코스트에 대한 공통 가중치를 반환합니다.
        /// </summary>
        /// <param name="cost">가중치를 조회할 코스트 값입니다.</param>
        /// <returns>
        /// <see cref="CostWeights"/>에 설정되어 있으면 해당 값을, 없으면 <see cref="DefaultCostWeight"/>를 반환합니다.
        /// </returns>
        public float GetCostWeight(int cost)
        {
            float w = CostWeights.TryGetValue(cost, out float v) ? v : DefaultCostWeight;

            // 데이터가 깨졌거나( NaN / Infinity ), 음수 가중치가 들어온 경우 방어합니다.
            // 가중치가 0이면 해당 코스트는 "가중치 재배열"에 참여하지 않는 것으로 간주됩니다.
            if (float.IsNaN(w) || float.IsInfinity(w) || w < 0f)
                return Mathf.Max(0f, DefaultCostWeight);

            return w;
        }

        // --------------------------------------------------------------------
        // 2) 최대 마나 도달 전까지 가중 셔플을 적용할 카드 개수
        // --------------------------------------------------------------------

        /// <summary>
        /// 가중 셔플을 적용할 덱 앞부분 카드 수입니다.
        /// </summary>
        /// <remarks>
        /// <para>
        /// 일반적으로 전투 설정(startMana, maxMana, manaPerTurn, initialDrawCount, drawPerTurn)을 바탕으로
        /// <c>ShufflePhaseHelper.CalculateFrontLoadedCountByManaCurve</c>를 통해 계산된 값을 사용합니다.
        /// </para>
        /// <para>
        /// 이 값(앞 구간)까지만 “페이즈/가중치 기반 재배열”이 적용되며,
        /// 이후 구간의 카드는 (전략에 따라) 기본 랜덤 셔플 결과를 그대로 유지합니다.
        /// </para>
        /// </remarks>
        public int FrontLoadedCount { get; set; }

        // --------------------------------------------------------------------
        // 3) 초/중/후반 구간 비율
        // --------------------------------------------------------------------

        /// <summary>
        /// FrontLoadedCount 구간 내에서 초반(Early) 구간이 차지하는 비율입니다(0~1).
        /// </summary>
        /// <remarks>
        /// 예: <c>0.3</c>이면 FrontLoadedCount의 앞 30%를 초반으로 사용합니다.
        /// </remarks>
        public float EarlyPhaseRatio { get; set; } = 0.3f;

        /// <summary>
        /// FrontLoadedCount 구간 내에서 중반(Mid) 구간이 차지하는 비율입니다(0~1).
        /// </summary>
        /// <remarks>
        /// <para>
        /// 예: <c>0.4</c>이면 초반 다음 구간의 40%를 중반으로 사용합니다.
        /// </para>
        /// <para>
        /// 나머지 영역은 자동으로 후반(Late) 구간으로 처리됩니다.
        /// </para>
        /// </remarks>
        public float MidPhaseRatio { get; set; } = 0.4f;

        /// <summary>
        /// FrontLoadedCount 값과 비율 설정을 기반으로 초/중/후반 구간의 카드 수를 계산합니다.
        /// </summary>
        /// <param name="frontLoadedCount">분할 대상이 되는 앞 구간 카드 수입니다.</param>
        /// <param name="earlyCount">계산된 초반 구간 카드 수입니다.</param>
        /// <param name="midCount">계산된 중반 구간 카드 수입니다.</param>
        /// <param name="lateCount">계산된 후반 구간 카드 수입니다.</param>
        /// <remarks>
        /// <para><b>보정 규칙</b></para>
        /// <list type="bullet">
        ///   <item><description><paramref name="frontLoadedCount"/>는 0 미만이면 0으로 보정됩니다.</description></item>
        ///   <item><description><see cref="EarlyPhaseRatio"/>와 <see cref="MidPhaseRatio"/>는 각각 0~1로 Clamp됩니다.</description></item>
        ///   <item><description>초반+중반 비율 합이 너무 커지는 것을 방지하기 위해 합계는 최대 0.95로 Clamp됩니다.</description></item>
        ///   <item><description>비율 합이 거의 0이면(잘못된 설정) 초/중반은 0으로 두고 전부 후반으로 처리합니다.</description></item>
        ///   <item><description>반올림으로 인해 초반+중반 합이 초과되면 중반부터 우선 감소시켜 합계를 맞춥니다.</description></item>
        /// </list>
        /// </remarks>
        public void CalculatePhaseCounts(
            int frontLoadedCount,
            out int earlyCount,
            out int midCount,
            out int lateCount)
        {
            frontLoadedCount = Mathf.Max(0, frontLoadedCount);

            if (frontLoadedCount == 0)
            {
                earlyCount = midCount = lateCount = 0;
                return;
            }

            float earlyRatio = Mathf.Clamp01(EarlyPhaseRatio);
            float midRatio = Mathf.Clamp01(MidPhaseRatio);

            // early+mid 가 1을 넘지 않도록 보정
            float ratioSum = Mathf.Clamp(earlyRatio + midRatio, 0f, 0.95f);
            if (ratioSum < 0.0001f)
            {
                // 비율 설정이 잘못된 경우, 전부 후반으로 처리
                earlyCount = 0;
                midCount = 0;
                lateCount = frontLoadedCount;
                return;
            }

            earlyRatio = earlyRatio / ratioSum;
            midRatio = midRatio / ratioSum;

            earlyCount = Mathf.RoundToInt(frontLoadedCount * earlyRatio);
            midCount = Mathf.RoundToInt(frontLoadedCount * midRatio);

            int used = earlyCount + midCount;
            if (used > frontLoadedCount)
            {
                int overflow = used - frontLoadedCount;
                // overflow 를 mid 부터 줄이도록 보정
                int reduceMid = Mathf.Min(midCount, overflow);
                midCount -= reduceMid;
                overflow -= reduceMid;
                earlyCount = Mathf.Max(0, earlyCount - overflow);
                used = earlyCount + midCount;
            }

            lateCount = Mathf.Max(0, frontLoadedCount - used);
        }

        // --------------------------------------------------------------------
        // 4) 초/중/후반 구간별 코스트 가중치
        // --------------------------------------------------------------------

        /// <summary>
        /// 초반(Early) 구간(FrontLoadedCount의 앞부분)에 적용할 코스트별 가중치 설정입니다.
        /// </summary>
        public PhaseCostWeights EarlyPhaseWeights { get; } = new();

        /// <summary>
        /// 중반(Mid) 구간에 적용할 코스트별 가중치 설정입니다.
        /// </summary>
        public PhaseCostWeights MidPhaseWeights { get; } = new();

        /// <summary>
        /// 후반(Late) 구간에 적용할 코스트별 가중치 설정입니다.
        /// </summary>
        public PhaseCostWeights LatePhaseWeights { get; } = new();
    }
}
