using GGemCo2DTcg;

#if UNITY_EDITOR
namespace GGemCo2DTcgEditor
{
    /// <summary>Term 테이블 키 규칙 유틸</summary>
    public static class TcgTermKey
    {
        // Trigger: trigger_{TriggerType}_prefix
        public static string TriggerPrefixKey(TcgAbilityConstants.TcgAbilityTriggerType tcgAbilityTriggerType) => $"trigger_{tcgAbilityTriggerType}_prefix";

        // Target: target_{TargetType}_{Case}
        public static string TargetKey(TcgAbilityConstants.TcgAbilityTargetType tcgAbilityTargetType, string @case) => $"target_{tcgAbilityTargetType}_{@case}";

        // AbilityType + TargetType에 맞는 "대표 케이스"를 반환(검증/런타임용)
        public static string TargetKeyForAbility(TcgAbilityConstants.TcgAbilityType abilityType, TcgAbilityConstants.TcgAbilityTargetType tcgAbilityTargetType)
        {
            if (abilityType == TcgAbilityConstants.TcgAbilityType.Damage)
            {
                if (tcgAbilityTargetType is TcgAbilityConstants.TcgAbilityTargetType.EnemyHero or TcgAbilityConstants.TcgAbilityTargetType.AllyHero)
                    return TargetKey(tcgAbilityTargetType, "to");
                if (tcgAbilityTargetType is TcgAbilityConstants.TcgAbilityTargetType.EnemyCreature
                    or TcgAbilityConstants.TcgAbilityTargetType.AllyCreature
                    or TcgAbilityConstants.TcgAbilityTargetType.AnyCreature) return TargetKey(tcgAbilityTargetType, "noun1");
                return TargetKey(tcgAbilityTargetType, "noun1");
            }

            if (abilityType == TcgAbilityConstants.TcgAbilityType.Heal)
            {
                if (tcgAbilityTargetType is TcgAbilityConstants.TcgAbilityTargetType.EnemyHero or TcgAbilityConstants.TcgAbilityTargetType.AllyHero) return TargetKey(tcgAbilityTargetType, "obj");
                if (tcgAbilityTargetType is TcgAbilityConstants.TcgAbilityTargetType.EnemyCreature
                    or TcgAbilityConstants.TcgAbilityTargetType.AllyCreature
                    or TcgAbilityConstants.TcgAbilityTargetType.AnyCreature) return TargetKey(tcgAbilityTargetType, "obj");
                return TargetKey(tcgAbilityTargetType, "obj");
            }

            if (abilityType is TcgAbilityConstants.TcgAbilityType.BuffAttack
                or TcgAbilityConstants.TcgAbilityType.BuffHealth or TcgAbilityConstants.TcgAbilityType.BuffAttackHealth)
                return TargetKey(tcgAbilityTargetType, "obj");

            return TargetKey(tcgAbilityTargetType, "noun1");
        }
    }
}
#endif