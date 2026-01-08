using System.Collections.Generic;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 게임 진행 단계(초반/중반/후반 등)에 따라
    /// 카드 코스트별 선택 가중치를 정의하는 설정 클래스입니다.
    /// </summary>
    /// <remarks>
    /// <para>
    /// 특정 코스트에 대한 가중치가 정의되지 않은 경우,
    /// <see cref="DefaultWeight"/> 값이 기본으로 사용됩니다.
    /// </para>
    /// <para>
    /// 주로 AI 선택 로직, 카드 등장 확률 조정, 밸런싱 계산 등에 활용됩니다.
    /// </para>
    /// </remarks>
    public sealed class PhaseCostWeights
    {
        /// <summary>
        /// 코스트별 가중치를 저장하는 내부 딕셔너리입니다.
        /// </summary>
        private readonly Dictionary<int, float> _costWeights = new();

        /// <summary>
        /// 어떤 코스트에 대해서도 별도의 설정이 없을 때 적용되는 기본 가중치입니다.
        /// </summary>
        /// <remarks>
        /// 기본값은 <c>1.0</c>이며, 모든 코스트를 동일한 비중으로 취급합니다.
        /// </remarks>
        public float DefaultWeight { get; set; } = 1f;

        /// <summary>
        /// 현재 설정된 모든 코스트 가중치의 읽기 전용 뷰를 반환합니다.
        /// </summary>
        /// <remarks>
        /// 외부에서는 컬렉션을 수정할 수 없으며,
        /// 내부 상태 보호 및 디버깅/조회 용도로 사용됩니다.
        /// </remarks>
        public IReadOnlyDictionary<int, float> Weights => _costWeights;

        /// <summary>
        /// 지정된 코스트에 대한 가중치를 설정합니다.
        /// </summary>
        /// <param name="cost">가중치를 설정할 카드 코스트 값입니다.</param>
        /// <param name="weight">
        /// 적용할 가중치 값입니다.
        /// 음수 값이 전달되면 0으로 보정됩니다.
        /// </param>
        public void SetWeight(int cost, float weight)
        {
            if (weight < 0f)
                weight = 0f;

            _costWeights[cost] = weight;
        }

        /// <summary>
        /// 지정된 코스트에 대한 가중치를 반환합니다.
        /// </summary>
        /// <param name="cost">조회할 카드 코스트 값입니다.</param>
        /// <returns>
        /// 해당 코스트에 대한 가중치가 설정되어 있으면 그 값을,
        /// 설정되어 있지 않으면 <see cref="DefaultWeight"/>를 반환합니다.
        /// </returns>
        public float GetWeight(int cost)
        {
            if (_costWeights.TryGetValue(cost, out float weight))
                return weight;

            return DefaultWeight;
        }

        /// <summary>
        /// 모든 코스트 가중치 설정을 초기화합니다.
        /// </summary>
        /// <remarks>
        /// <see cref="DefaultWeight"/> 값은 유지되며,
        /// 내부 딕셔너리만 비워집니다.
        /// </remarks>
        public void Clear()
        {
            _costWeights.Clear();
        }
    }
}
