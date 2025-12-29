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

        /// <summary>
        /// Ability 설명/연출 식별에 사용되는 Ability UID입니다.
        /// (일반적으로 Localization StringTable 키와 동일)
        /// </summary>
        public int AbilityUid { get; }

        /// <summary>
        /// 실행된 Ability의 타입입니다.
        /// UI 연출 분기 기준으로 사용됩니다.
        /// </summary>
        public TcgAbilityConstants.TcgAbilityType AbilityType { get; }

        /// <summary>
        /// Ability를 시전한 플레이어의 진영 정보입니다.
        /// </summary>
        public ConfigCommonTcg.TcgPlayerSide CasterSide { get; }

        /// <summary>
        /// Ability를 발생시킨 원본 카드 데이터입니다.
        /// 
        /// - 손패 / 덱 / 영구 카드 등일 수 있습니다.
        /// - 트리거 기반 실행 등 카드가 명확하지 않은 경우 null일 수 있습니다.
        /// </summary>
        public TcgBattleDataCard SourceCard { get; }

        /// <summary>
        /// Ability의 명시적 타겟 필드 카드입니다.
        /// 
        /// - 단일 타겟 Ability의 경우 설정됩니다.
        /// - 광역 / 비타겟 Ability의 경우 null일 수 있습니다.
        /// </summary>
        public TcgBattleDataFieldCard TargetCard { get; }

        /// <summary>
        /// Ability가 어떤 트리거에 의해 실행되었는지를 나타냅니다.
        /// 
        /// - OnDraw, OnTurnStart 등 트리거 기반 실행 시 설정됩니다.
        /// - 명시되지 않은 경우 <see cref="TcgAbilityConstants.TcgAbilityTriggerType.None"/>입니다.
        /// </summary>
        public TcgAbilityConstants.TcgAbilityTriggerType TcgAbilityTriggerType { get; }

        /// <summary>
        /// UI 전용 추가 데이터입니다.
        /// 
        /// - 도메인 로직에서는 사용하지 않습니다.
        /// - 필요 시 UI 레이어에서 캐스팅하여 연출에 활용할 수 있습니다.
        /// - 사용하지 않는 경우 null입니다.
        /// </summary>
        public object UserData { get; }

        /// <summary>
        /// Ability 연출 이벤트를 생성합니다.
        /// </summary>
        /// <param name="eventPhase">Ability 실행 단계(Begin/End)입니다.</param>
        /// <param name="abilityUid">Ability 식별 UID입니다.</param>
        /// <param name="abilityType">Ability 타입입니다.</param>
        /// <param name="casterSide">Ability 시전자 진영입니다.</param>
        /// <param name="sourceCard">Ability를 발생시킨 원본 카드입니다. null 가능.</param>
        /// <param name="targetCard">명시적 타겟 필드 카드입니다. null 가능.</param>
        /// <param name="tcgAbilityTriggerType">Ability 실행 트리거 타입입니다.</param>
        /// <param name="userData">UI 전용 추가 데이터입니다. null 가능.</param>
        public TcgAbilityPresentationEvent(
            Phase eventPhase,
            int abilityUid,
            TcgAbilityConstants.TcgAbilityType abilityType,
            ConfigCommonTcg.TcgPlayerSide casterSide,
            TcgBattleDataCard sourceCard,
            TcgBattleDataFieldCard targetCard,
            TcgAbilityConstants.TcgAbilityTriggerType tcgAbilityTriggerType,
            object userData = null)
        {
            EventPhase = eventPhase;
            AbilityUid = abilityUid;
            AbilityType = abilityType;
            CasterSide = casterSide;
            SourceCard = sourceCard;
            TargetCard = targetCard;
            TcgAbilityTriggerType = tcgAbilityTriggerType;
            UserData = userData;
        }
    }
}
