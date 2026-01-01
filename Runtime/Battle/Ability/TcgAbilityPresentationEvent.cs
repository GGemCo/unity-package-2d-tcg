namespace GGemCo2DTcg
{
    /// <summary>
    /// Ability 실행 흐름과 UI 연출을 연결하기 위한 도메인 이벤트입니다.
    /// 
    /// - 도메인 레이어는 UI에 의존하지 않고,
    ///   "어떤 Ability가 언제 실행되었는지"만 이벤트로 전달합니다.
    /// - UI 레이어는 이 이벤트를 구독하여
    ///   <see cref="TcgAbilityConstants.TcgAbilityType"/> 및 Phase에 따라
    ///   적절한 연출(이펙트, 애니메이션, 사운드 등)을 재생할 수 있습니다.
    /// </summary>
    public readonly struct TcgAbilityPresentationEvent
    {
        /// <summary>
        /// Ability 실행 타이밍을 나타냅니다.
        /// </summary>
        public enum Phase
        {
            /// <summary>Ability 실행 직전(연출 시작 지점).</summary>
            Begin,

            /// <summary>Ability 실행 직후(연출 종료 또는 후처리 지점).</summary>
            End
        }

        /// <summary>
        /// 이벤트가 발생한 실행 단계입니다.
        /// </summary>
        public Phase EventPhase { get; }

        public TcgAbilityDefinition Ability { get; }
        
        /// <summary>
        /// Ability가 어떤 트리거에 의해 실행되었는지를 나타냅니다.
        /// 
        /// - OnDraw, OnTurnStart 등 트리거 기반 실행 시 설정됩니다.
        /// - 명시되지 않은 경우 <see cref="TcgAbilityConstants.TcgAbilityTriggerType.None"/>입니다.
        /// </summary>
        public TcgAbilityConstants.TcgAbilityTriggerType AbilityTriggerType { get; }

        /// <summary>
        /// Ability를 시전한 플레이어의 진영 정보입니다.
        /// </summary>
        public ConfigCommonTcg.TcgPlayerSide CasterSide { get; }

        public ConfigCommonTcg.TcgZone CasterZone { get; }
        public int CasterIndex { get; }
        public ConfigCommonTcg.TcgZone TargetZone { get; }
        public int TargetIndex { get; }

        /// <summary>
        /// UI 전용 추가 데이터입니다.
        /// 
        /// - 도메인 로직에서는 사용하지 않습니다.
        /// - 필요 시 UI 레이어에서 캐스팅하여 연출에 활용할 수 있습니다.
        /// - 사용하지 않는 경우 null입니다.
        /// </summary>
        public object UserData { get; }

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
