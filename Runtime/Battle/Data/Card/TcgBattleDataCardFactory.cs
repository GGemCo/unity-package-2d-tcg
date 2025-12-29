using System;
using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    public static class TcgBattleDataCardFactory
    {
        /// <summary>
        /// "Rush|Taunt" 같이 '|' 로 구분된 문자열을 키워드 리스트로 변환.
        /// </summary>
        public static List<ConfigCommonTcg.TcgKeyword> ParseKeywords(string keywordRaw)
        {
            var result = new List<ConfigCommonTcg.TcgKeyword>(4);

            if (string.IsNullOrWhiteSpace(keywordRaw))
                return result;

            var parts = keywordRaw.Split('|');
            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                if (string.IsNullOrEmpty(trimmed))
                    continue;

                if (Enum.TryParse<ConfigCommonTcg.TcgKeyword>(trimmed, true, out var kw))
                {
                    if (kw != ConfigCommonTcg.TcgKeyword.None)
                        result.Add(kw);
                }
                else
                {
                    GcLogger.LogWarning($"[TcgCardRuntimeFactory] Unknown keyword: {trimmed}");
                }
            }

            return result;
        }
        public static List<TcgAbilityData> ParseEffects(string effectsRaw)
        {
            var result = new List<TcgAbilityData>(4);

            if (string.IsNullOrWhiteSpace(effectsRaw))
                return result;

            var items = effectsRaw.Split(';');
            foreach (var item in items)
            {
                var trimmed = item.Trim();
                if (string.IsNullOrEmpty(trimmed))
                    continue;

                // Legacy string format:
                //   "AbilityId" 또는 "AbilityId:..." 형태를 허용합니다.
                //   - 신규 구조에서는 Ability 파라미터는 tcg_ability 테이블의 ParamA/B/C로 관리합니다.
                //   - 구형 테이블의 Value/TargetType/Extra 는 호환을 위해 토큰 파싱만 하되,
                //     실제 적용은 Ability 정의가 우선입니다.
                var tokens = trimmed.Split(':');
                if (tokens.Length < 1)
                    continue;

                var abilityIdRaw = tokens[0].Trim();
                if (!int.TryParse(abilityIdRaw, out int abilityId) || abilityId <= 0)
                {
                    GcLogger.LogWarning($"[TcgCardRuntimeFactory] Invalid AbilityId: {abilityIdRaw}");
                    continue;
                }

                result.Add(new TcgAbilityData { ability = new TcgAbilityDefinition(abilityId, TcgAbilityConstants.TcgAbilityType.None, TcgAbilityConstants.TcgAbilityTriggerType.None, TcgAbilityConstants.TcgAbilityTargetType.None, 0, 0, 0) });
            }

            return result;
        }

        public static TcgBattleDataCard CreateBattleDataCard(StruckTableTcgCard row)
        {
            var keywords         = ParseKeywords(row.keywordRaw);

            return new TcgBattleDataCard(
            
                row,
                keywords);
        }
        
        /// <summary>
        /// Creature 타입 카드를 기반으로 필드에 소환할 유닛 런타임을 생성합니다.
        /// - 실제 스탯/키워드는 카드 테이블/런타임에서 가져와야 합니다.
        /// </summary>
        public static TcgBattleDataFieldCard CreateBattleDataFieldCard(
            ConfigCommonTcg.TcgPlayerSide ownerSide,
            TcgBattleDataCard tcgBattleDataCard)
        {
            if (tcgBattleDataCard == null)
            {
                GcLogger.LogError("[Battle] CreateUnitFromCard: cardRuntime is null.");
                return null;
            }

            // 1) CardRuntime 에서 스탯/키워드 정보 가져오기
            //    (아래는 예시. 실제 필드 이름에 맞게 수정 필요)
            int attack = tcgBattleDataCard.attack.Value; // 예: CardRuntime.Attack
            int hp     = tcgBattleDataCard.health.Value; // 예: CardRuntime.Health

            // 키워드 예시: CardRuntime.Keywords 또는 테이블에서 변환
            List<ConfigCommonTcg.TcgKeyword> keywords = new List<ConfigCommonTcg.TcgKeyword>(4);
            foreach (var kw in tcgBattleDataCard.Keywords) // 예: IEnumerable<TcgKeyword>
            {
                keywords.Add(kw);
            }

            // 2) 유닛 런타임 생성
            var unit = new TcgBattleDataFieldCard(
                tcgBattleDataCard.Uid,
                ownerSide,
                tcgBattleDataCard,
                attack,
                hp,
                keywords);

            // 소환 시점에는 공격 불가 (돌진 키워드가 있으면 예외)
            if (unit.HasKeyword(ConfigCommonTcg.TcgKeyword.Rush))
                unit.CanAttack = true;
            else
                unit.CanAttack = false;

            return unit;
        }
    }
}