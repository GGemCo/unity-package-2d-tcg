using System.Collections.Generic;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 셔플 동작에 필요한 부가 설정.
    /// - 코스트별 가중치
    /// - 초반 몇 장을 가중 셔플 대상 영역으로 할지 등
    /// </summary>
    public class ShuffleConfig
    {
        /// <summary>
        /// 코스트별 가중치.
        /// key: 코스트, value: 가중치.
        /// 기본값은 1.0f.
        /// </summary>
        public Dictionary<int, float> CostWeights { get; } = new();

        /// <summary>
        /// 덱 앞부분에서 가중 셔플을 적용할 카드 수.
        /// 0 이면 일반 셔플과 동일.
        /// </summary>
        public int FrontLoadedCount { get; set; } = 0;

        /// <summary>
        /// 지정된 코스트에 해당하는 가중치를 반환한다.
        /// 설정되지 않은 코스트는 1.0f 로 취급.
        /// </summary>
        public float GetCostWeight(int cost)
        {
            if (CostWeights.TryGetValue(cost, out float weight))
                return weight;
            return 1f;
        }
    }
}