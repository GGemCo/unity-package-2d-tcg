namespace GGemCo2DTcg
{
    /// <summary>
    /// Ability 실행 흐름과 UI 연출을 연결하기 위한 도메인 이벤트입니다.
    ///
    /// - 도메인 레이어는 UI에 의존하지 않고,
    ///   "어떤 Ability가 언제 실행되었는지"만 이벤트로 전달합니다.
    /// - UI 레이어는 이 이벤트를 구독하여
    ///   <see cref="TcgAbilityConstants.TcgAbilityType"/> 및 실행 Phase에 따라
    ///   이펙트, 애니메이션, 사운드 등의 연출을 재생할 수 있습니다.
    /// </summary>
    public readonly struct TcgAbilityPresentationEvent
    {
        /// <summary>
        /// Ability 실행 타이밍을 나타냅니다.
        /// </summary>
        public enum Phase
        {
            /// <summary>
            /// Ability 실행 직전 단계입니다.
            /// UI 연출 시작 지점으로 사용됩니다.
            /// </summary>
            Begin,

            /// <summary>
            /// Ability 실행 직후 단계입니다.
            /// UI 연출 종료 또는 후처리 지점으로 사용됩니다.
            /// </summary>
            End
        }

        /// <summary>
        /// 이벤트가 발생한 Ability 실행 단계입니다.
        /// </summary>
        public Phase EventPhase { get; }

        /// <summary>
        /// 실행된 Ability의 정의 정보입니다.
        /// </summary>
        public TcgAbilityDefinition Ability { get; }

        /// <summary>
        /// Ability가 어떤 트리거에 의해 실행되었는지를 나타냅니다.
        ///
        /// - OnDraw, OnTurnStart 등 트리거 기반 실행 시 설정됩니다.
        /// - 명시되지 않은 경우 <see cref="TcgAbilityConstants.TcgAbilityTriggerType.None"/>입니다.
        /// </summary>
        public TcgAbilityConstants.TcgAbilityTriggerType AbilityTriggerType { get; }

        /// <summary>
        /// Ability를 시전한 플레이어의 진영(Side) 정보입니다.
        /// </summary>
        public ConfigCommonTcg.TcgPlayerSide CasterSide { get; }

        /// <summary>
        /// Ability를 시전한 주체가 위치한 Zone입니다.
        /// </summary>
        public ConfigCommonTcg.TcgZone CasterZone { get; }

        /// <summary>
        /// Caster Zone 내에서의 인덱스 값입니다.
        /// (예: 카드 슬롯 인덱스, 유닛 인덱스 등)
        /// </summary>
        public int CasterIndex { get; }

        /// <summary>
        /// Ability의 주요 대상(Target)이 위치한 Zone입니다.
        /// </summary>
        public ConfigCommonTcg.TcgZone TargetZone { get; }

        /// <summary>
        /// Target Zone 내에서의 인덱스 값입니다.
        /// </summary>
        public int TargetIndex { get; }

        /// <summary>
        /// UI 전용 추가 데이터입니다.
        ///
        /// - 도메인 로직에서는 사용하지 않습니다.
        /// - UI 레이어에서만 캐스팅하여 연출 정보로 활용합니다.
        /// - 사용하지 않는 경우 null입니다.
        /// </summary>
        public object UserData { get; }

        /// <summary>
        /// <see cref="TcgAbilityPresentationEvent"/>를 생성합니다.
        /// </summary>
        /// <param name="eventPhase">Ability 실행 단계(Begin/End)입니다.</param>
        /// <param name="ability">실행된 Ability의 정의 정보입니다.</param>
        /// <param name="casterSide">Ability를 시전한 플레이어의 진영입니다.</param>
        /// <param name="casterZone">Ability를 시전한 주체가 위치한 Zone입니다.</param>
        /// <param name="casterIndex">Caster Zone 내 인덱스 값입니다.</param>
        /// <param name="targetZone">Ability의 대상(Target)이 위치한 Zone입니다.</param>
        /// <param name="targetIndex">Target Zone 내 인덱스 값입니다.</param>
        /// <param name="abilityTriggerType">Ability를 실행시킨 트리거 유형입니다.</param>
        /// <param name="userData">UI 연출을 위한 추가 데이터입니다. 사용하지 않는 경우 null입니다.</param>
        public TcgAbilityPresentationEvent(
            Phase eventPhase,
            TcgAbilityDefinition ability,
            ConfigCommonTcg.TcgPlayerSide casterSide,
            ConfigCommonTcg.TcgZone casterZone,
            int casterIndex,
            ConfigCommonTcg.TcgZone targetZone,
            int targetIndex,
            TcgAbilityConstants.TcgAbilityTriggerType abilityTriggerType,
            object userData = null)
        {
            EventPhase = eventPhase;
            Ability = ability;
            AbilityTriggerType = abilityTriggerType;
            CasterSide = casterSide;
            CasterZone = casterZone;
            CasterIndex = casterIndex;
            TargetZone = targetZone;
            TargetIndex = targetIndex;
            UserData = userData;
        }
    }
}
