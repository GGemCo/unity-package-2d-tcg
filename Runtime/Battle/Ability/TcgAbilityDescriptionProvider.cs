using System.Collections.Generic;
using UnityEngine.Localization;
using AbilityType = GGemCo2DTcg.TcgAbilityConstants.TcgAbilityType;
using AbilityTargetType = GGemCo2DTcg.TcgAbilityConstants.TcgAbilityTargetType;
using AbilityTriggerType = GGemCo2DTcg.TcgAbilityConstants.TcgAbilityTriggerType;

namespace GGemCo2DTcg
{
    /// <summary>
    /// Ability 설명을 Localization Smart String으로 생성하는 Provider.
    ///
    /// - Ability 템플릿: StringTableCollection "TCG_Ability"의 "{Uid}"
    /// - Trigger/Target: Term 테이블("TCG_Term_Trigger", "TCG_Term_Target")의 LocalizedString을 인자로 전달
    /// - Smart String에서 {Trigger.loc}, {Target.loc} 형태로 "중첩 localized"를 수행합니다.
    /// </summary>
    public sealed class TcgAbilityDescriptionProvider
    {
        private readonly Dictionary<int, string> _cache = new(512);

        private readonly string _abilityCollection;
        private readonly string _triggerCollection;
        private readonly string _targetCollection;

        public TcgAbilityDescriptionProvider(
            string abilityCollection = LocalizationConstantsTcg.Tables.AbilityDescription,
            string triggerCollection = LocalizationConstantsTcg.Tables.AbilityTrigger,
            string targetCollection  = LocalizationConstantsTcg.Tables.AbilityTarget)
        {
            _abilityCollection = abilityCollection;
            _triggerCollection = triggerCollection;
            _targetCollection  = targetCollection;
        }

        public void ClearCache() => _cache.Clear();

        public string GetDescription(in StruckTableTcgAbility row)
        {
            if (_cache.TryGetValue(row.uid, out var cached))
                return cached;
            
            var abilityString = new LocalizedString(_abilityCollection, $"{row.uid}")
            {
                Arguments = new object[] { BuildArguments(row) }
            };
            
            var result = abilityString.GetLocalizedString();
            _cache[row.uid] = result;
            return result;
        }
        private Dictionary<string, object> BuildArguments(in StruckTableTcgAbility row)
        {
            var triggerKey = TriggerPrefixKey(row.tcgAbilityTriggerType);
            var targetKey  = TargetKeyForAbility(row.abilityType, row.tcgAbilityTargetType);
            // Term을 먼저 "문자열"로 평가
            var triggerText = new LocalizedString(_triggerCollection, triggerKey).GetLocalizedString();
            var targetText  = new LocalizedString(_targetCollection,  targetKey).GetLocalizedString();

            var args = new Dictionary<string, object>
            {
                { "Trigger", triggerText },
                { "Target",  targetText },

                { "Value",  row.paramA },
                { "ValueA", row.paramA },
                { "ValueB", row.paramB },
                { "ValueC", row.paramC },
            };

            return args;
        }

        private static string TriggerPrefixKey(AbilityTriggerType tcgAbilityTriggerType) => $"trigger_{tcgAbilityTriggerType}_prefix";
        private static string TargetKey(AbilityTargetType tcgAbilityTargetType, string @case) => $"target_{tcgAbilityTargetType}_{@case}";

        private static string TargetKeyForAbility(AbilityType abilityType, AbilityTargetType tcgAbilityTargetType)
        {
            if (abilityType == AbilityType.Damage)
            {
                if (tcgAbilityTargetType is AbilityTargetType.EnemyHero or AbilityTargetType.AllyHero)
                    return TargetKey(tcgAbilityTargetType, "to");
                if (tcgAbilityTargetType is AbilityTargetType.EnemyCreature
                    or AbilityTargetType.AllyCreature
                    or AbilityTargetType.AnyCreature) return TargetKey(tcgAbilityTargetType, "noun1");
                return TargetKey(tcgAbilityTargetType, "noun1");
            }

            if (abilityType == AbilityType.Heal)
            {
                if (tcgAbilityTargetType is AbilityTargetType.EnemyHero or AbilityTargetType.AllyHero)
                    return TargetKey(tcgAbilityTargetType, "obj");
                if (tcgAbilityTargetType is AbilityTargetType.EnemyCreature
                    or AbilityTargetType.AllyCreature
                    or AbilityTargetType.AnyCreature) return TargetKey(tcgAbilityTargetType, "obj");
                return TargetKey(tcgAbilityTargetType, "obj");
            }

            if (abilityType is AbilityType.BuffAttack
                or AbilityType.BuffHealth or AbilityType.BuffAttackHealth)
                return TargetKey(tcgAbilityTargetType, "obj");

            return TargetKey(tcgAbilityTargetType, "noun1");
        }
    }
}
