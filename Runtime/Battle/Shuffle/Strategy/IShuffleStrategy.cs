using System.Collections.Generic;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 덱(카드 리스트) 셔플 전략을 정의하는 인터페이스입니다.
    /// </summary>
    /// <remarks>
    /// <para>
    /// 구현체는 Fisher–Yates, 가중치 기반(Weighted) 등 다양한 셔플 방식을 제공할 수 있습니다.
    /// </para>
    /// <para>
    /// 셔플은 전달된 리스트를 제자리(in-place)에서 재배열하며,
    /// 동일 입력이라도 난수/메타데이터에 따라 결과가 달라질 수 있습니다.
    /// </para>
    /// </remarks>
    public interface IShuffleStrategy
    {
        /// <summary>
        /// 전달된 카드 리스트를 제자리(in-place)에서 셔플합니다.
        /// </summary>
        /// <typeparam name="TCard">셔플 대상 카드 타입으로 <see cref="ICardInfo"/>를 구현해야 합니다.</typeparam>
        /// <param name="cards">셔플할 카드 리스트입니다. 호출 후 내부 순서가 변경됩니다.</param>
        /// <param name="metaData">
        /// 셔플 정책에 필요한 부가 정보입니다(예: 시드(seed), 페이즈 정보, 가중치 설정 등).
        /// </param>
        /// <remarks>
        /// <para>
        /// 구현체는 필요 시 <paramref name="metaData"/>를 참고하여
        /// 특정 카드의 우선 배치/회피, 구간별 가중치 적용 등의 규칙을 반영할 수 있습니다.
        /// </para>
        /// <para>
        /// NOTE: <paramref name="cards"/>가 <c>null</c>인 경우의 처리(예외/무시)는 구현체 정책에 따릅니다.
        /// </para>
        /// </remarks>
        void Shuffle<TCard>(List<TCard> cards, ShuffleMetaData metaData)
            where TCard : GGemCo2DTcg.ICardInfo;
    }
}