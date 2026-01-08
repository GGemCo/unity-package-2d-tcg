using System.Collections.Generic;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 셔플을 수행하지 않는 기본(No-op) 셔플 전략입니다.
    /// </summary>
    /// <remarks>
    /// <para>
    /// 전달된 카드 리스트의 순서를 변경하지 않으며,
    /// 이미 정렬된 덱을 그대로 유지하고 싶을 때 사용됩니다.
    /// </para>
    /// <para>
    /// 테스트, 디버깅, 혹은 셔플이 의도적으로 비활성화된 규칙에서
    /// 기본 전략으로 활용할 수 있습니다.
    /// </para>
    /// </remarks>
    public class ShuffleStrategyNone : IShuffleStrategy
    {
        /// <summary>
        /// 카드 리스트를 셔플하지 않고 그대로 유지합니다.
        /// </summary>
        /// <typeparam name="TCard">셔플 대상 카드 타입입니다.</typeparam>
        /// <param name="cards">셔플 대상 카드 리스트입니다. 순서는 변경되지 않습니다.</param>
        /// <param name="metaData">
        /// 셔플 정책에 필요한 메타데이터이지만,
        /// 본 구현체에서는 사용되지 않습니다.
        /// </param>
        public void Shuffle<TCard>(List<TCard> cards, ShuffleMetaData metaData)
            where TCard : ICardInfo
        {
            // Intentionally left blank (No-op)
        }
    }
}