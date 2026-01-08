namespace GGemCo2DTcg
{
    /// <summary>
    /// 추가 행동(Extra Action) Ability 실행 결과를
    /// UI 연출(프레젠테이션) 레이어로 전달하기 위한 페이로드입니다.
    /// </summary>
    /// <remarks>
    /// UI에서는 이 정보를 기반으로 추가 턴/행동 획득,
    /// 아이콘 표시 또는 강조 연출 등을 선택적으로 재생할 수 있습니다.
    /// </remarks>
    public sealed class TcgAbilityPayloadExtraAction
    {
        /// <summary>
        /// 추가로 부여되는 행동 횟수입니다.
        /// </summary>
        /// <remarks>
        /// 값이 0인 경우, 실제 행동 증가는 없지만
        /// 트리거성 연출만 수행하는 용도로 해석될 수 있습니다.
        /// </remarks>
        public int ExtraActionCount { get; }

        /// <summary>
        /// 추가 행동 연출용 페이로드를 생성합니다.
        /// </summary>
        /// <param name="extraActionCount">추가로 부여할 행동 횟수입니다.</param>
        public TcgAbilityPayloadExtraAction(int extraActionCount)
        {
            ExtraActionCount = extraActionCount;
        }
    }
}