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
        /// 드로우할 카드 장수입니다.
        /// </summary>
        /// <remarks>
        /// 값이 0인 경우, 실제 드로우 없이
        /// 트리거성 연출만 수행하는 용도로 해석될 수 있습니다.
        /// </remarks>
        public int DrawCount { get; }

        /// <summary>
        /// 드로우 연출용 페이로드를 생성합니다.
        /// </summary>
        /// <param name="drawCount">드로우할 카드 장수입니다.</param>
        public TcgAbilityPayloadDraw(int drawCount)
        {
            DrawCount = drawCount;
        }
    }
}