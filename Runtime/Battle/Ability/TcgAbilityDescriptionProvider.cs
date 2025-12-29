using System.Collections.Generic;
using UnityEngine.Localization;
using AbilityType = GGemCo2DTcg.TcgAbilityConstants.TcgAbilityType;
using AbilityTargetType = GGemCo2DTcg.TcgAbilityConstants.TcgAbilityTargetType;
using AbilityTriggerType = GGemCo2DTcg.TcgAbilityConstants.TcgAbilityTriggerType;

namespace GGemCo2DTcg
{
    /// <summary>
    /// <see cref="TcgAbilityDefinition"/>을 기반으로 Ability 설명 문자열을 생성하는 Provider입니다.
    /// Unity Localization의 Smart String을 사용하며, Trigger/Target 등의 용어는 Term 테이블에서 평가된 문자열을
    /// Smart String 인자로 전달하여 템플릿 내에서 치환되도록 구성합니다.
    ///
    /// 규칙:
    /// - Ability 템플릿: StringTableCollection "TCG_Ability"의 "{Uid}"(예: "123")
    /// - Trigger/Target: Term 테이블("TCG_Term_Trigger", "TCG_Term_Target")에서 키로 조회
    /// - Smart String 인자: Trigger/Target/Value(ValueA~C) 등을 제공
    /// </summary>
    public sealed class TcgAbilityDescriptionProvider
    {
        /// <summary>
        /// uid(AbilityKey) → 최종 로컬라이즈된 결과 문자열 캐시입니다.
        /// </summary>
        private readonly Dictionary<int, string> _cache = new(512);

        private readonly string _abilityCollection;
        private readonly string _triggerCollection;
        private readonly string _targetCollection;

        /// <summary>
        /// Ability 설명 생성에 사용할 StringTableCollection 이름들을 지정합니다.
        /// </summary>
        /// <param name="abilityCollection">Ability 설명 템플릿이 위치한 테이블 컬렉션 이름입니다.</param>
        /// <param name="triggerCollection">Trigger 용어(Term)가 위치한 테이블 컬렉션 이름입니다.</param>
        /// <param name="targetCollection">Target 용어(Term)가 위치한 테이블 컬렉션 이름입니다.</param>
        public TcgAbilityDescriptionProvider(
            string abilityCollection = LocalizationConstantsTcg.Tables.AbilityDescription,
            string triggerCollection = LocalizationConstantsTcg.Tables.AbilityTrigger,
            string targetCollection  = LocalizationConstantsTcg.Tables.AbilityTarget)
        {
            _abilityCollection = abilityCollection;
            _triggerCollection = triggerCollection;
            _targetCollection  = targetCollection;
        }

        /// <summary>
        /// 내부 캐시를 비웁니다.
        /// (언어 변경, 테이블 리로드 등의 이벤트 후 호출하는 용도)
        /// </summary>
        public void ClearCache() => _cache.Clear();

        /// <summary>
        /// Ability 정의로부터 로컬라이즈된 설명 문자열을 반환합니다.
        /// 캐시에 존재하면 캐시 값을 반환하며, 없으면 Smart String을 평가하여 캐시에 저장합니다.
        /// </summary>
        /// <param name="row">설명 생성을 위한 Ability 정의입니다.</param>
        /// <returns>로컬라이즈된 Ability 설명 문자열입니다.</returns>
        public string GetDescription(in TcgAbilityDefinition row)
        {
            if (_cache.TryGetValue(row.uid, out var cached))
                return cached;

            // Ability 템플릿(키: uid)을 Smart String으로 평가하고, 인자(Trigger/Target/Value...)를 주입합니다.
            var abilityString = new LocalizedString(_abilityCollection, $"{row.uid}")
            {
                Arguments = new object[] { BuildArguments(row) }
            };

            var result = abilityString.GetLocalizedString();
            _cache[row.uid] = result;
            return result;
        }

        /// <summary>
        /// Ability 설명 템플릿(Smart String)에 전달할 인자 딕셔너리를 생성합니다.
        /// Trigger/Target은 Term 테이블에서 먼저 "문자열"로 평가한 값을 전달합니다.
        /// </summary>
        /// <param name="row">인자 구성을 위한 Ability 정의입니다.</param>
        /// <returns>Smart String 템플릿에서 참조할 인자 딕셔너리입니다.</returns>
        private Dictionary<string, object> BuildArguments(in TcgAbilityDefinition row)
        {
            var triggerKey = TriggerPrefixKey(row.tcgAbilityTriggerType);
            var targetKey  = TargetKeyForAbility(row.abilityType, row.tcgAbilityTargetType);

            // Trigger/Target 용어는 Term 테이블에서 먼저 평가하여 최종 문자열을 인자로 제공합니다.
            var triggerText = new LocalizedString(_triggerCollection, triggerKey).GetLocalizedString();
            var targetText  = new LocalizedString(_targetCollection,  targetKey).GetLocalizedString();

            return new Dictionary<string, object>
            {
                { "Trigger", triggerText },
                { "Target",  targetText },

                // 수치 파라미터(템플릿 호환을 위해 별칭도 함께 제공)
                { "Value",  row.paramA },
                { "ValueA", row.paramA },
                { "ValueB", row.paramB },
                { "ValueC", row.paramC },
            };
        }

        /// <summary>
        /// Trigger 용어 키(접두어 형태)를 생성합니다.
        /// 예: "trigger_OnPlay_prefix"
        /// </summary>
        /// <param name="tcgAbilityTriggerType">트리거 타입입니다.</param>
        /// <returns>Trigger Term 테이블 조회 키입니다.</returns>
        private static string TriggerPrefixKey(AbilityTriggerType tcgAbilityTriggerType)
            => $"trigger_{tcgAbilityTriggerType}_prefix";

        /// <summary>
        /// Target 용어 키를 생성합니다.
        /// @case는 템플릿에서 요구하는 문법 형태(예: "to", "obj", "noun1")를 구분합니다.
        /// </summary>
        /// <param name="tcgAbilityTargetType">타겟 타입입니다.</param>
        /// <param name="case">타겟 문법 케이스(예: "to", "obj", "noun1")입니다.</param>
        /// <returns>Target Term 테이블 조회 키입니다.</returns>
        private static string TargetKey(AbilityTargetType tcgAbilityTargetType, string @case)
            => $"target_{tcgAbilityTargetType}_{@case}";

        /// <summary>
        /// Ability 타입과 Target 타입 조합에 맞는 Target Term 키를 선택합니다.
        /// (예: Damage는 "to" 또는 "noun1", Heal/Buff는 주로 "obj" 등)
        /// </summary>
        /// <param name="abilityType">능력 타입입니다.</param>
        /// <param name="tcgAbilityTargetType">능력의 타겟 타입입니다.</param>
        /// <returns>Target Term 테이블 조회 키입니다.</returns>
        private static string TargetKeyForAbility(AbilityType abilityType, AbilityTargetType tcgAbilityTargetType)
        {
            if (abilityType == AbilityType.Damage)
            {
                if (tcgAbilityTargetType is AbilityTargetType.EnemyHero or AbilityTargetType.AllyHero)
                    return TargetKey(tcgAbilityTargetType, "to");

                if (tcgAbilityTargetType is AbilityTargetType.EnemyCreature
                    or AbilityTargetType.AllyCreature
                    or AbilityTargetType.AnyCreature)
                    return TargetKey(tcgAbilityTargetType, "noun1");

                return TargetKey(tcgAbilityTargetType, "noun1");
            }

            if (abilityType == AbilityType.Heal)
            {
                if (tcgAbilityTargetType is AbilityTargetType.EnemyHero or AbilityTargetType.AllyHero)
                    return TargetKey(tcgAbilityTargetType, "obj");

                if (tcgAbilityTargetType is AbilityTargetType.EnemyCreature
                    or AbilityTargetType.AllyCreature
                    or AbilityTargetType.AnyCreature)
                    return TargetKey(tcgAbilityTargetType, "obj");

                return TargetKey(tcgAbilityTargetType, "obj");
            }

            if (abilityType is AbilityType.BuffAttack
                or AbilityType.BuffHealth or AbilityType.BuffAttackHealth)
                return TargetKey(tcgAbilityTargetType, "obj");

            return TargetKey(tcgAbilityTargetType, "noun1");
        }
    }
}
