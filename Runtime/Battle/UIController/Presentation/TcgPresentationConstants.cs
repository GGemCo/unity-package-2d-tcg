namespace GGemCo2DTcg
{
    public static class TcgPresentationConstants
    {
        /// <summary>
        /// UI 연출 단계 타입.
        /// 도메인 로직에서 직접 애니메이션을 실행하지 않고,
        /// "무슨 일이 일어났는지"를 표현하기 위해 사용합니다.
        /// </summary>
        public enum TcgPresentationStepType
        {
            /// <summary>손패의 카드가 필드(보드)로 이동/배치됨</summary>
            MoveCardToField,
            MoveCardToGrave,
            MoveCardFieldToGrave,
            MoveCardToBack,

            /// <summary>유닛이 유닛을 공격함</summary>
            AttackUnit,

            /// <summary>유닛이 영웅을 공격함</summary>
            AttackHero,

            /// <summary>피해 숫자/피격 연출</summary>
            AbilityDamage,
            HealPopup,

            /// <summary>사망 제거(페이드아웃 등)</summary>
            DeathFadeOut,
        
            // 효과
            MoveCardToTarget,

            // 상태 변화
            ApplyBuff,
            RemoveBuff,

            // 종료
            EndBattle,
            EndTurn,
        }
    }
}