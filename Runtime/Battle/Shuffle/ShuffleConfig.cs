using System.Collections.Generic;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 셔플 동작에 필요한 부가 설정.
    /// - 코스트별 가중치 (단일 테이블)
    /// - 초반 구간 카드 수 (FrontLoadedCount)
    /// - 초/중/후반 구간 비율
    /// - 초/중/후반 구간별 코스트 가중치
    /// </summary>
    public class ShuffleConfig
    {
        // --------------------------------------------------------------------
        // 1) 공통 코스트 가중치
        // --------------------------------------------------------------------

        /// <summary>
        /// 어떤 코스트에도 별도 설정이 없을 때 사용할 기본 가중치.
        /// </summary>
        public float DefaultCostWeight { get; set; } = 1f;

        /// <summary>
        /// 코스트별 공통 가중치.
        /// <para>key: 코스트, value: 가중치.</para>
        /// 설정되지 않은 코스트는 기본값(DefaultCostWeight) 로 취급합니다.
        /// </summary>
        public Dictionary<int, float> CostWeights { get; } = new();

        /// <summary>
        /// 지정된 코스트에 해당하는 공통 가중치를 반환합니다.
        /// 설정되지 않은 코스트는 DefaultCostWeight 로 취급합니다.
        /// </summary>
        public float GetCostWeight(int cost)
        {
            return CostWeights.GetValueOrDefault(cost, DefaultCostWeight);
        }

        // --------------------------------------------------------------------
        // 2) 최대 마나 도달 전까지 가중 셔플을 적용할 카드 개수
        // --------------------------------------------------------------------

        /// <summary>
        /// 가중 셔플을 적용할 덱 앞부분 카드 수.
        /// <para>
        /// 전투 설정(startMana, maxMana, manaPerTurn, initialDrawCount, drawPerTurn) 을 기반으로
        /// ShufflePhaseHelper.CalculateFrontLoadedCountByManaCurve 를 통해 계산합니다.
        /// </para>
        /// 이 구간까지만 "초/중/후반 코스트 가중 셔플"이 적용되며,
        /// 이후 카드는 순수 랜덤 순서를 유지합니다.
        /// </summary>
        public int FrontLoadedCount { get; set; }

        // --------------------------------------------------------------------
        // 3) 초/중/후반 구간 비율
        // --------------------------------------------------------------------

        /// <summary>
        /// FrontLoadedCount 영역 내에서 초반에 해당하는 비율 (0~1).
        /// 예: 0.3 → 앞 30% 를 초반으로 사용.
        /// </summary>
        public float EarlyPhaseRatio { get; set; } = 0.3f;

        /// <summary>
        /// FrontLoadedCount 영역 내에서 중반에 해당하는 비율 (0~1).
        /// 예: 0.4 → 다음 40% 를 중반으로 사용.
        /// 나머지는 자동으로 후반으로 처리됩니다.
        /// </summary>
        public float MidPhaseRatio { get; set; } = 0.4f;

        /// <summary>
        /// FrontLoadedCount 와 비율 설정을 기반으로
        /// 초/중/후반 구간 카드 수를 계산합니다.
        /// </summary>
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
        /// 초반 구간(FrontLoadedCount 의 앞부분)에 대한 코스트별 가중치.
        /// </summary>
        public PhaseCostWeights EarlyPhaseWeights { get; } = new();

        /// <summary>
        /// 중반 구간에 대한 코스트별 가중치.
        /// </summary>
        public PhaseCostWeights MidPhaseWeights { get; } = new();

        /// <summary>
        /// 후반 구간에 대한 코스트별 가중치.
        /// </summary>
        public PhaseCostWeights LatePhaseWeights { get; } = new();
    }
}
