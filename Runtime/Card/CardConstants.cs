namespace GGemCo2DTcg
{
    public class CardConstants
    {
        /// <summary>
        /// 카드의 상위 타입.
        /// Creature, Spell, Equipment, Permanent, Event 등
        /// </summary>
        public enum Type
        {
            None,
            Creature,
            Spell,
            Equipment,
            Permanent,
            Event,
        }

        /// <summary>
        /// 카드 희귀도. 하스스톤/MTG식으로 확장 가능.
        /// </summary>
        public enum Grade
        {
            Common,
            Rare,
            Epic,
            Legendary
        }

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
    }
}