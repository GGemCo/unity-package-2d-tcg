namespace GGemCo2DTcg
{
    /// <summary>
    /// 테이블 기반 Ability 실행을 위한 상위 분류.
    /// - 테이블에서 문자열로 관리(예: "Damage", "Heal")하고,
    ///   런타임에서는 이 값을 기준으로 핸들러를 선택합니다.
    /// </summary>
    public enum TcgAbilityType
    {
        None = 0,

        Damage,
        Heal,
        Draw,
        BuffAttack,
        BuffHealth,
        GainMana,
        ExtraAction,
        BuffAttackHealth
    }
}
