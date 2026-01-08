namespace GGemCo2DTcg
{
    /// <summary>
    /// Ability 실행 결과 중 피해(Damage) 정보를
    /// UI 연출(프레젠테이션) 레이어로 전달하기 위한 페이로드입니다.
    /// </summary>
    /// <remarks>
    /// - 도메인 로직은 UI에 직접 의존하지 않으며,
    ///   UI는 이 페이로드를 기반으로 Damage 팝업, 이펙트, 강조 연출 등을 선택적으로 재생할 수 있습니다.
    /// - 단순 트리거용 연출의 경우 값이 의미 없을 수 있으며,
    ///   이때는 0을 전달하는 방식으로 표현할 수 있습니다.
    /// </remarks>
    public sealed class TcgAbilityPayloadDamage
    {
        /// <summary>
        /// 피해량을 나타내는 값입니다.
        /// </summary>
        /// <remarks>
        /// UI에서는 이 값을 사용해 숫자 팝업, 애니메이션 강도 등을 결정할 수 있습니다.
        /// </remarks>
        public int DamageValue { get; }

        /// <summary>
        /// 피해 연출용 페이로드를 생성합니다.
        /// </summary>
        /// <param name="damageValue">표시 또는 처리할 피해 수치입니다.</param>
        public TcgAbilityPayloadDamage(int damageValue)
        {
            DamageValue = damageValue;
        }
    }
}