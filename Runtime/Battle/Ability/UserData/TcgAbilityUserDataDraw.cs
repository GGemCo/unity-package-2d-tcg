using System.Collections.Generic;

namespace GGemCo2DTcg
{
    /// <summary>
    /// Draw Ability 실행 시점에 발생한 "실제 손패 변화" 정보를 UI 연출 레이어로 전달하기 위한 사용자 데이터입니다.
    /// </summary>
    public sealed class TcgAbilityUserDataDraw
    {
        /// <summary>
        /// 요청된 드로우 장수(Ability paramA 기반)입니다.
        /// </summary>
        public int RequestedDrawCount { get; }

        /// <summary>
        /// 손패에 실제로 추가된 카드 목록입니다(오버드로우/피로 등으로 일부 누락될 수 있음).
        /// </summary>
        public IReadOnlyList<TcgBattleDataCardInHand> AddedCards { get; }

        /// <summary>
        /// 손패에 실제로 추가된 카드의 손패 인덱스 목록입니다.
        /// </summary>
        public IReadOnlyList<int> AddedHandIndices { get; }

        public TcgAbilityUserDataDraw(
            int requestedDrawCount,
            IReadOnlyList<TcgBattleDataCardInHand> addedCards,
            IReadOnlyList<int> addedHandIndices)
        {
            RequestedDrawCount = requestedDrawCount;
            AddedCards = addedCards;
            AddedHandIndices = addedHandIndices;
        }
    }
}
