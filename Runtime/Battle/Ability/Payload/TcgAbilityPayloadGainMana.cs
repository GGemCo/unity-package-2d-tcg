namespace GGemCo2DTcg
{
    /// <summary>
    /// 마나 획득(Gain Mana) Ability 실행 결과를
    /// UI 연출(프레젠테이션) 레이어로 전달하기 위한 페이로드입니다.
    /// </summary>
    /// <remarks>
    /// UI에서는 이 정보를 기반으로 마나 증가 애니메이션,
    /// 마나 게이지 강조 또는 이펙트 연출 등을 선택적으로 재생할 수 있습니다.
    /// </remarks>
    public sealed class TcgAbilityPayloadGainMana
    {
        /// <summary>
        /// 획득할 마나 수치입니다.
        /// </summary>
        /// <remarks>
        /// 값이 0인 경우, 실제 수치 변화 없이
        /// 트리거성 연출만 수행하는 용도로 해석될 수 있습니다.
        /// </remarks>
        public int ManaValue { get; }

        /// <summary>
        /// 마나 획득 연출용 페이로드를 생성합니다.
        /// </summary>
        /// <param name="manaValue">획득할 마나 수치입니다.</param>
        public TcgAbilityPayloadGainMana(int manaValue)
        {
            ManaValue = manaValue;
        }
    }
}