using System.Collections.Generic;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 특정 구간(초반/중반/후반)에 대한 코스트별 가중치 설정.
    /// </summary>
    public sealed class PhaseCostWeights
    {
        private readonly Dictionary<int, float> _costWeights = new();

        /// <summary>
        /// 어떤 코스트에 대해서도 설정이 없을 때 사용할 기본 가중치.
        /// </summary>
        public float DefaultWeight { get; set; } = 1f;

        /// <summary>
        /// 모든 코스트 가중치의 읽기 전용 뷰.
        /// </summary>
        public IReadOnlyDictionary<int, float> Weights => _costWeights;

        /// <summary>
        /// 지정된 코스트의 가중치를 설정합니다.
        /// </summary>
        public void SetWeight(int cost, float weight)
        {
            if (weight < 0f)
                weight = 0f;

            _costWeights[cost] = weight;
        }

        /// <summary>
        /// 지정된 코스트의 가중치를 가져옵니다.
        /// 설정되지 않았으면 DefaultWeight 를 반환합니다.
        /// </summary>
        public float GetWeight(int cost)
        {
            if (_costWeights.TryGetValue(cost, out float weight))
                return weight;

            return DefaultWeight;
        }

        /// <summary>
        /// 내부 딕셔너리를 직접 다루고 싶을 때 사용.
        /// </summary>
        public void Clear()
        {
            _costWeights.Clear();
        }
    }
}