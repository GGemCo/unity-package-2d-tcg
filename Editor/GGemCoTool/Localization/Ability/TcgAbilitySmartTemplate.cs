using AbilityType = GGemCo2DTcg.TcgAbilityConstants.TcgAbilityType;
using AbilityTargetType = GGemCo2DTcg.TcgAbilityConstants.TcgAbilityTargetType;

#if UNITY_EDITOR
namespace GGemCo2DTcgEditor
{
    /// <summary>
    /// Ability 템플릿 생성기
    /// - Damage/Heal의 한국어 문장 구조 차이를 반영합니다(영웅: ~에게 / 크리처: ~에).
    /// - OnPlay는 Trigger Term에서 "사용 시"로 처리합니다.
    /// - EnemyCreature / AnyCreature 등의 문장 차이는 Target Term에서 "적/대상 + 1체"로 처리합니다.
    /// </summary>
    public static class TcgAbilitySmartTemplate
    {
        public static string BuildKorean(in TcgAbilityTsvRow row)
        {
            return row.AbilityType switch
            {
                AbilityType.Damage => BuildKoDamage(row.TcgAbilityTargetType),
                AbilityType.Heal   => BuildKoHeal(row.TcgAbilityTargetType),

                AbilityType.Draw   => "{Trigger}카드 {Value}장을 드로우합니다.",
                AbilityType.GainMana => "{Trigger}마나를 {Value} 증가시킵니다.",
                AbilityType.ExtraAction => "{Trigger}이번 턴 추가 행동을 {Value}회 부여합니다.",

                AbilityType.BuffAttack => "{Trigger}{Target} 공격력 {Value}를 부여합니다.",
                AbilityType.BuffHealth => "{Trigger}{Target} 체력 {Value}를 부여합니다.",
                AbilityType.BuffAttackHealth => "{Trigger}{Target} {ValueA}/{ValueB}를 부여합니다.",

                _ => "{Trigger}{Target} 값({ValueA},{ValueB},{ValueC})"
            };

            static string BuildKoDamage(AbilityTargetType tcgAbilityTargetType)
            {
                // 영웅: "...에게 3 피해"
                if (tcgAbilityTargetType is AbilityTargetType.EnemyHero or AbilityTargetType.AllyHero)
                    return "{Trigger}{Target} {Value} 피해를 줍니다.";

                // 크리처: "적 크리처 1체에 2 피해"
                if (tcgAbilityTargetType is AbilityTargetType.EnemyCreature
                    or AbilityTargetType.AllyCreature or AbilityTargetType.AnyCreature)
                    return "{Trigger}{Target}에 {Value} 피해를 줍니다.";

                return "{Trigger}{Target} {Value} 피해를 줍니다.";
            }

            static string BuildKoHeal(AbilityTargetType tcgAbilityTargetType)
            {
                // 영웅: "아군 영웅을 5 회복"
                // (TargetTerm: target_AllyHero_obj = "아군 영웅을")
                if (tcgAbilityTargetType is AbilityTargetType.AllyHero or AbilityTargetType.EnemyHero)
                    return "{Trigger}{Target} {Value} 회복합니다.";

                // 크리처: "아군 크리처 1체를 1 회복"
                // (TargetTerm: target_AllyCreature_obj = "아군 크리처 1체를")
                if (tcgAbilityTargetType is AbilityTargetType.AllyCreature
                    or AbilityTargetType.EnemyCreature or AbilityTargetType.AnyCreature)
                    return "{Trigger}{Target} {Value} 회복합니다.";

                return "{Trigger}{Target} {Value} 회복합니다.";
            }
        }

        public static string BuildEnglish(in TcgAbilityTsvRow row)
        {
            return row.AbilityType switch
            {
                AbilityType.Damage => "{Trigger}Deal {Value} damage to {Target}.",
                AbilityType.Heal   => "{Trigger}Restore {Value} Health to {Target}.",

                AbilityType.Draw   => "{Trigger}Draw {Value} card(s).",
                AbilityType.GainMana => "{Trigger}Gain {Value} mana.",
                AbilityType.ExtraAction => "{Trigger}Gain {Value} extra action(s) this turn.",

                AbilityType.BuffAttack => "{Trigger}Give {Target} {Value} Attack.",
                AbilityType.BuffHealth => "{Trigger}Give {Target} {Value} Health.",
                AbilityType.BuffAttackHealth => "{Trigger}Give {Target} {ValueA}/{ValueB}.",

                _ => "{Trigger}{Target} values({ValueA},{ValueB},{ValueC})"
            };
        }
    }
}
#endif
