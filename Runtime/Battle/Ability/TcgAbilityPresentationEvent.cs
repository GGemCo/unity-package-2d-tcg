using System;

namespace GGemCo2DTcg
{
    /// <summary>
    /// Ability 실행과 연출을 연결하기 위한 도메인 이벤트.
    /// - UI 레이어는 이 이벤트를 구독하여 <see cref="TcgAbilityConstants.TcgAbilityType"/>별 연출을 재생할 수 있습니다.
    /// - 도메인 레이어는 UI 의존 없이 "무슨 Ability가 언제 실행되었는지"만 알립니다.
    /// </summary>
    public readonly struct TcgAbilityPresentationEvent
    {
        public enum Phase
        {
            /// <summary>Ability 실행 직전</summary>
            Begin,
            /// <summary>Ability 실행 직후</summary>
            End
        }

        public Phase EventPhase { get; }

        public int AbilityUid { get; }
        public TcgAbilityConstants.TcgAbilityType AbilityType { get; }

        public ConfigCommonTcg.TcgPlayerSide CasterSide { get; }

        /// <summary>Ability를 발생시킨 카드(손패/덱/카드 원본). null 가능.</summary>
        public TcgBattleDataCard SourceCard { get; }

        /// <summary>명시적 타겟(필드 카드). null 가능.</summary>
        public TcgBattleDataFieldCard TargetCard { get; }

        /// <summary>트리거 기반 실행 시(예: OnDraw/OnTurnStart). 명시되지 않으면 None.</summary>
        public TcgAbilityConstants.TcgAbilityTriggerType TcgAbilityTriggerType { get; }

        /// <summary>추가 정보(필요 시 UI에서 캐스팅해서 사용). null 가능.</summary>
        public object UserData { get; }

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
