namespace GGemCo2DTcg
{
    /// <summary>
    /// 카드 능력(Ability) 정의를 표현하는 데이터 행 인터페이스입니다.
    /// </summary>
    /// <remarks>
    /// 주로 테이블(Row) 기반 데이터(DB/CSV/ScriptableObject 등)를
    /// 공통 형태로 접근하기 위해 사용됩니다.
    /// </remarks>
    public interface ITcgCardAbilityRow
    {
        /// <summary>
        /// 능력 데이터의 고유 식별자(ID)입니다.
        /// </summary>
        int Uid { get; }

        /// <summary>
        /// 능력의 기본 타입(효과 종류)입니다.
        /// </summary>
        TcgAbilityConstants.TcgAbilityType AbilityType { get; }

        /// <summary>
        /// 능력이 발동되는 트리거 타입입니다.
        /// </summary>
        /// <remarks>
        /// 예: 소환 시, 공격 시, 사망 시 등
        /// </remarks>
        TcgAbilityConstants.TcgAbilityTriggerType TcgAbilityTriggerType { get; }

        /// <summary>
        /// 능력이 적용되는 대상 타입입니다.
        /// </summary>
        /// <remarks>
        /// 예: 자신, 적 유닛, 모든 유닛 등
        /// </remarks>
        TcgAbilityConstants.TcgAbilityTargetType TcgAbilityTargetType { get; }

        /// <summary>
        /// 능력에 사용되는 첫 번째 파라미터 값입니다.
        /// </summary>
        /// <remarks>
        /// 데미지, 회복량, 수치 보정 값 등 능력 타입에 따라 의미가 달라집니다.
        /// </remarks>
        int ParamA { get; }

        /// <summary>
        /// 능력에 사용되는 두 번째 파라미터 값입니다.
        /// </summary>
        int ParamB { get; }

        /// <summary>
        /// 능력에 사용되는 세 번째 파라미터 값입니다.
        /// </summary>
        int ParamC { get; }
    }
}