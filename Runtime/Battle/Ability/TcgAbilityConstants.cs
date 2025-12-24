namespace GGemCo2DTcg
{
    public class TcgAbilityConstants
    {
        /// <summary>
        /// 카드가 효과를 적용할 때 선택할 수 있는 대상 종류.
        /// 실제 전투 시스템에서는 이걸 더 쪼개서 사용하면 됩니다.
        /// </summary>
        public enum TargetType
        {
            None,
            Self,
            AllyCreature,
            EnemyCreature,
            AnyCreature,
            EnemyHero,
            AllyHero,
            AllAllies,
            AllEnemies,
            BoardAll
        }

        /// <summary>
        /// 카드 능력(Ability) 및 각종 지속/이벤트 효과가 발동되는 시점을 나타냅니다.
        /// </summary>
        public enum TriggerType
        {
            None = 0,

            /// <summary>카드가 플레이(사용)될 때.</summary>
            OnPlay,

            /// <summary>카드를 드로우할 때.</summary>
            OnDraw,

            /// <summary>소유자 턴 시작 시.</summary>
            OnTurnStart,

            /// <summary>소유자 턴 종료 시.</summary>
            OnTurnEnd,

            /// <summary>크리처가 소환될 때.</summary>
            OnSummon,

            /// <summary>크리처가 사망할 때.</summary>
            OnDeath,

            /// <summary>피해를 줄 때.</summary>
            OnDamageDealt,

            /// <summary>피해를 받을 때.</summary>
            OnDamageTaken
        }
    }
}