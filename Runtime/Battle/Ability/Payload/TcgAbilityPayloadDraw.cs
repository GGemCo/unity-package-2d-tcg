using System.Collections.Generic;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 드로우(카드 획득) Ability 실행 결과를
    /// UI 연출(프레젠테이션) 레이어로 전달하기 위한 페이로드입니다.
    /// </summary>
    /// <remarks>
    /// UI에서는 이 정보를 기반으로 카드 드로우 애니메이션,
    /// 덱/손패 강조 연출 등을 선택적으로 재생할 수 있습니다.
    /// </remarks>
    public sealed class TcgAbilityPayloadDraw
    {
        /// <summary>
        /// 드로우 요청 장수입니다.
        /// </summary>
        /// <remarks>
        /// 값이 0인 경우, 실제 드로우 없이
        /// 트리거성 연출만 수행하는 용도로 해석될 수 있습니다.
        /// </remarks>
        public int DrawCount { get; }

        /// <summary>
        /// 손패에 실제로 추가된 카드 목록입니다.
        /// </summary>
        /// <remarks>
        /// 오버드로우/피로 등으로 일부 드로우가 손패에 들어가지 않을 수 있으므로,
        /// UI가 "실제 유입된 카드만" 연출하고 싶을 때 사용합니다.
        /// </remarks>
        public IReadOnlyList<TcgBattleDataCardInHand> AddedCards { get; }

        /// <summary>
        /// 손패에 실제로 추가된 카드의 손패 인덱스 목록입니다.
        /// </summary>
        public IReadOnlyList<int> AddedHandIndices { get; }

        /// <summary>
        /// 드로우 연출용 페이로드를 생성합니다.
        /// </summary>
        /// <param name="drawCount">드로우 요청 장수입니다.</param>
        public TcgAbilityPayloadDraw(int drawCount)
            : this(drawCount, addedCards: null, addedHandIndices: null)
        {
        }

        /// <summary>
        /// 드로우 연출용 페이로드를 생성합니다.
        /// </summary>
        /// <param name="drawCount">드로우 요청 장수입니다.</param>
        /// <param name="addedCards">손패에 실제로 추가된 카드 목록입니다.</param>
        /// <param name="addedHandIndices">손패에 실제로 추가된 카드의 인덱스 목록입니다.</param>
        public TcgAbilityPayloadDraw(
            int drawCount,
            IReadOnlyList<TcgBattleDataCardInHand> addedCards,
            IReadOnlyList<int> addedHandIndices)
        {
            DrawCount = drawCount;
            AddedCards = addedCards;
            AddedHandIndices = addedHandIndices;
        }
    }
}
