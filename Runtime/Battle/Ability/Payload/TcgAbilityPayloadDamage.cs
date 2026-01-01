namespace GGemCo2DTcg
{
    /// <summary>
    /// Ability 실행 결과를 UI 연출(프레젠테이션) 레이어로 전달하기 위한 구조화된 페이로드입니다.
    /// 
    /// - 도메인 로직은 UI에 의존하지 않으며, UI는 이 정보를 기반으로
    ///   Damage/Heal 팝업, 버프 아이콘, 마나/드로우 강조 등 연출을 선택적으로 재생할 수 있습니다.
    /// - 값이 필요 없는 경우(예: 단순 트리거 표시)는 0 또는 null로 전달될 수 있습니다.
    /// </summary>
    public sealed class TcgAbilityPayloadDamage
    {
        /// <summary>
        /// 기본 파라미터(주로 ParamA). UI에서 숫자 팝업 등에 사용할 수 있습니다.
        /// </summary>
        public int DamageValue { get; }

        public TcgAbilityPayloadDamage(
            int damageValue)
        {
            DamageValue = damageValue;
        }
    }
}
