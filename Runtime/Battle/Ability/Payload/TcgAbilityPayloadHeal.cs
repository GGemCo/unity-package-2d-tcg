namespace GGemCo2DTcg
{
    /// <summary>
    /// 회복(Heal) Ability 실행 결과를
    /// UI 연출(프레젠테이션) 레이어로 전달하기 위한 페이로드입니다.
    /// </summary>
    /// <remarks>
    /// UI에서는 이 정보를 기반으로 회복 수치 팝업,
    /// 회복 이펙트 또는 체력 바 강조 연출 등을 선택적으로 재생할 수 있습니다.
    /// </remarks>
    public sealed class TcgAbilityPayloadHeal
    {
        /// <summary>
        /// 회복될 체력 수치입니다.
        /// </summary>
        /// <remarks>
        /// 값이 0인 경우, 실제 수치 변화 없이
        /// 트리거성 연출만 수행하는 용도로 해석될 수 있습니다.
        /// </remarks>
        public int HealValue { get; }

        /// <summary>
        /// 회복 연출용 페이로드를 생성합니다.
        /// </summary>
        /// <param name="healValue">회복될 체력 수치입니다.</param>
        public TcgAbilityPayloadHeal(int healValue)
        {
            HealValue = healValue;
        }
    }
}