namespace GGemCo2DTcg
{
    /// <summary>
    /// UI 연출 단계 타입.
    /// 도메인 로직에서 직접 애니메이션을 실행하지 않고,
    /// "무슨 일이 일어났는지"를 표현하기 위해 사용합니다.
    /// </summary>
    public enum TcgPresentationStepType
    {
        /// <summary>손패의 카드가 필드(보드)로 이동/배치됨</summary>
        MoveCardHandToBoard,
        MoveCardHandToGrave,
        MoveCardBoardToGrave,

        /// <summary>유닛이 유닛을 공격함</summary>
        AttackUnit,

        /// <summary>유닛이 영웅을 공격함</summary>
        AttackHero,

        /// <summary>피해 숫자/피격 연출</summary>
        DamagePopup,
        HealPopup,

        /// <summary>사망 제거(페이드아웃 등)</summary>
        DeathFadeOut,
        
        // 효과
        PlaySpellCast,
        PlayEffectOnTarget,

        // 상태 변화
        ApplyBuff,
        RemoveBuff,

        // 종료
        UnitDeath
    }

    /// <summary>
    /// UI 연출 단계 데이터.
    /// - Side: 행위자 기준 side
    /// - FromIndex/ToIndex: 손패/보드 인덱스 등
    /// - ValueA/ValueB: 데미지 등 보조 값
    /// </summary>
    public readonly struct TcgPresentationStep
    {
        public TcgPresentationStepType Type { get; }
        public ConfigCommonTcg.TcgPlayerSide Side { get; }
        
        public int FromIndex { get; }
        public int ToIndex { get; }
        
        public int ValueA { get; }
        public int ValueB { get; }
        public int CountSlot { get; }

        public TcgPresentationStep(
            TcgPresentationStepType type,
            ConfigCommonTcg.TcgPlayerSide side,
            int fromIndex,
            int toIndex,
            int countSlot,
            int valueA = 0,
            int valueB = 0)
        {
            Type = type;
            Side = side;
            FromIndex = fromIndex;
            ToIndex = toIndex;
            CountSlot = countSlot;
            ValueA = valueA;
            ValueB = valueB;
        }
    }
}