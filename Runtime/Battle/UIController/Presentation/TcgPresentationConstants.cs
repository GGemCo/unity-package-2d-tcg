namespace GGemCo2DTcg
{
    /// <summary>
    /// 프레젠테이션(UI) 계층에서 사용되는 상수 정의를 모아둔 클래스입니다.
    /// </summary>
    /// <remarks>
    /// 도메인 로직은 실제 애니메이션이나 UI 동작을 직접 실행하지 않고,
    /// 본 상수에 정의된 StepType을 통해
    /// "무슨 일이 일어났는지(What happened)"만을 전달하는 역할을 합니다.
    /// </remarks>
    public static class TcgPresentationConstants
    {
        /// <summary>
        /// UI 연출 단계의 종류를 나타내는 열거형입니다.
        /// </summary>
        /// <remarks>
        /// 각 값은 하나의 <c>TcgPresentationStep</c>이 어떤 의미의 연출을 수행해야 하는지를
        /// UI/Presentation 시스템에 전달하기 위한 식별자입니다.
        /// </remarks>
        public enum TcgPresentationStepType
        {
            /// <summary>
            /// 손패의 카드가 필드(보드)로 이동하거나 배치되는 연출입니다.
            /// </summary>
            MoveCardToField,

            /// <summary>
            /// 카드가 묘지로 이동하는 연출입니다.
            /// </summary>
            MoveCardToGrave,

            /// <summary>
            /// 카드가 덱 뒤쪽(백존)으로 이동하는 연출입니다.
            /// </summary>
            MoveCardToBack,

            /// <summary>
            /// 유닛이 다른 유닛을 공격하는 연출입니다.
            /// </summary>
            AttackUnit,

            /// <summary>
            /// 유닛이 영웅을 공격하는 연출입니다.
            /// </summary>
            AttackHero,

            /// <summary>
            /// 피해 수치 표시 및 피격 효과를 표현하는 연출입니다.
            /// </summary>
            AbilityDamage,

            /// <summary>
            /// 회복 수치 표시를 위한 팝업 연출입니다.
            /// </summary>
            HealPopup,

            /// <summary>
            /// 카드가 특정 대상(타겟)으로 이동하는 연출입니다.
            /// </summary>
            MoveCardToTarget,

            /// <summary>
            /// 유닛 또는 대상에게 버프(상태 효과)를 적용하는 연출입니다.
            /// </summary>
            ApplyBuff,

            /// <summary>
            /// 기존에 적용된 버프(상태 효과)를 제거하는 연출입니다.
            /// </summary>
            RemoveBuff,

            /// <summary>
            /// 전투가 종료되었음을 알리는 연출입니다.
            /// </summary>
            EndBattle,

            /// <summary>
            /// 턴 종료를 표현하는 연출입니다.
            /// </summary>
            EndTurn,

            /// <summary>
            /// 카드 드로우 시 드로우 팝업 또는 손패 강조를 표현하는 연출입니다.
            /// </summary>
            AbilityDraw,

            /// <summary>
            /// 마나 획득 시 팝업 표시 또는 마나 UI 펄스를 표현하는 연출입니다.
            /// </summary>
            AbilityGainMana,

            /// <summary>
            /// 추가 행동(Action)을 획득했음을 알리는 팝업 또는 턴 UI 강조 연출입니다.
            /// </summary>
            AbilityExtraAction
        }
    }
}
