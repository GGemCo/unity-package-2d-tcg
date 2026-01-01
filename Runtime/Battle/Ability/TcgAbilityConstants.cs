namespace GGemCo2DTcg
{
    /// <summary>
    /// TCG Ability 시스템에서 공통으로 사용하는 상수 및 열거형 정의 모음입니다.
    /// 테이블 데이터와 런타임 로직을 연결하는 기준 값으로 사용됩니다.
    /// </summary>
    public static class TcgAbilityConstants
    {
        /// <summary>
        /// 테이블 기반 Ability 실행을 위한 상위 분류 타입입니다.
        /// 
        /// - 테이블에서는 문자열(예: "Damage", "Heal")로 관리됩니다.
        /// - 런타임에서는 이 값을 기준으로 Ability 처리 로직(핸들러)을 선택합니다.
        /// </summary>
        public enum TcgAbilityType
        {
            /// <summary>능력이 없거나 정의되지 않은 상태입니다.</summary>
            None = 0,

            /// <summary>대상에게 피해를 줍니다.</summary>
            Damage,

            /// <summary>대상의 체력을 회복합니다.</summary>
            Heal,

            /// <summary>카드를 드로우합니다.</summary>
            Draw,

            /// <summary>공격력을 증가시키는 버프를 적용합니다.</summary>
            BuffAttack,

            /// <summary>체력을 증가시키는 버프를 적용합니다.</summary>
            BuffHealth,

            /// <summary>마나를 획득합니다.</summary>
            GainMana,

            /// <summary>추가 행동 또는 행동 기회를 부여합니다.</summary>
            ExtraAction,

            /// <summary>공격력과 체력을 동시에 증가시키는 버프를 적용합니다.</summary>
            BuffAttackHealth
        }

        /// <summary>
        /// Ability가 적용될 수 있는 대상의 종류를 정의합니다.
        /// 
        /// 전투 시스템 구현에 따라 더 세분화하거나 조합하여 사용할 수 있습니다.
        /// </summary>
        public enum TcgAbilityTargetType
        {
            /// <summary>대상이 없거나 정의되지 않은 상태입니다.</summary>
            None,

            /// <summary>자기 자신을 대상으로 합니다.</summary>
            Self,

            /// <summary>아군 크리처 하나를 대상으로 합니다.</summary>
            AllyCreature,

            /// <summary>적 크리처 하나를 대상으로 합니다.</summary>
            EnemyCreature,

            /// <summary>아군/적 구분 없이 크리처 하나를 대상으로 합니다.</summary>
            AnyCreature,

            /// <summary>적 영웅을 대상으로 합니다.</summary>
            EnemyHero,

            /// <summary>아군 영웅을 대상으로 합니다.</summary>
            AllyHero,

            /// <summary>모든 아군 유닛을 대상으로 합니다.</summary>
            AllAllies,

            /// <summary>모든 적 유닛을 대상으로 합니다.</summary>
            AllEnemies,

            /// <summary>보드 위의 모든 대상에 적용됩니다.</summary>
            FieldAll
        }

        /// <summary>
        /// 카드 능력(Ability) 및 지속 효과, 이벤트 효과가 발동되는 시점을 정의합니다.
        /// </summary>
        public enum TcgAbilityTriggerType
        {
            /// <summary>트리거가 없거나 정의되지 않은 상태입니다.</summary>
            None = 0,

            /// <summary>카드가 플레이(사용)될 때 발동됩니다.</summary>
            OnPlay,

            /// <summary>카드를 드로우할 때 발동됩니다.</summary>
            OnDraw,

            /// <summary>해당 카드 소유자의 턴 시작 시 발동됩니다.</summary>
            OnTurnStart,

            /// <summary>해당 카드 소유자의 턴 종료 시 발동됩니다.</summary>
            OnTurnEnd,

            /// <summary>크리처가 소환될 때 발동됩니다.</summary>
            OnSummon,

            /// <summary>크리처가 사망할 때 발동됩니다.</summary>
            OnDeath,

            /// <summary>피해를 가했을 때 발동됩니다.</summary>
            OnDamageDealt,

            /// <summary>피해를 받았을 때 발동됩니다.</summary>
            OnDamageTaken
        }
    }
}
