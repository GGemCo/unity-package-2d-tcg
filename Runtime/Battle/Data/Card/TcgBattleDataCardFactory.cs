using System;
using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 카드 테이블/런타임 문자열 데이터를 전투용 런타임 객체로 변환(파싱/생성)하는 팩토리입니다.
    /// 키워드/효과(Ability) 문자열을 파싱하고, 손패/필드 카드 런타임을 생성합니다.
    /// </summary>
    public static class TcgBattleDataCardFactory
    {
        /// <summary>
        /// <c>"Rush|Taunt"</c>처럼 <c>'|'</c>로 구분된 문자열을 키워드 목록으로 변환합니다.
        /// </summary>
        /// <param name="keywordRaw">키워드 원문 문자열(예: <c>Rush|Taunt</c>)입니다.</param>
        /// <returns>파싱된 키워드 목록입니다. 입력이 비어있으면 빈 리스트를 반환합니다.</returns>
        /// <remarks>
        /// - 대소문자를 무시하고 열거형으로 변환합니다.
        /// - <see cref="ConfigCommonTcg.TcgKeyword.None"/>은 결과에서 제외합니다.
        /// - 알 수 없는 토큰은 경고 로그를 남기고 무시합니다.
        /// </remarks>
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

        /// <summary>
        /// <c>;</c>로 구분된 효과(Ability) 문자열을 <see cref="TcgAbilityData"/> 목록으로 변환합니다.
        /// </summary>
        /// <param name="effectsRaw">
        /// 효과 원문 문자열입니다. 각 항목은 <c>AbilityId</c> 또는 <c>AbilityId:...</c> 형태를 허용합니다.
        /// </param>
        /// <returns>파싱된 효과(Ability) 목록입니다. 입력이 비어있으면 빈 리스트를 반환합니다.</returns>
        /// <remarks>
        /// Legacy 포맷 호환을 위해 <c>AbilityId</c> 뒤의 토큰(<c>:</c> 이후)은 파싱만 수행하며,
        /// 실제 파라미터/적용 로직은 Ability 정의(예: tcg_ability 테이블)가 우선한다는 전제입니다.
        /// </remarks>
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

                // NOTE: 현재 구현은 AbilityId만 보관하는 최소 런타임을 생성합니다.
                //       (AbilityType/TriggerType/TargetType/ParamA~C 등은 후속 로딩/정의가 우선될 수 있습니다.)
                result.Add(new TcgAbilityData
                {
                    ability = new TcgAbilityDefinition(
                        abilityId,
                        TcgAbilityConstants.TcgAbilityType.None,
                        TcgAbilityConstants.TcgAbilityTriggerType.None,
                        TcgAbilityConstants.TcgAbilityTargetType.None,
                        0, 0, 0)
                });
            }

            return result;
        }

        /// <summary>
        /// 카드 테이블 행 데이터를 기반으로 손패(Hand) 카드 런타임을 생성합니다.
        /// </summary>
        /// <param name="row">카드 테이블의 단일 행 데이터입니다.</param>
        /// <returns>생성된 손패 카드 런타임입니다.</returns>
        /// <remarks>
        /// 현재는 키워드만 파싱하여 주입하며, 기타 런타임 값은 <paramref name="row"/> 및
        /// <see cref="TcgBattleDataCardInHand"/> 생성자 구현에 의존합니다.
        /// </remarks>
        public static TcgBattleDataCardInHand CreateBattleDataCard(StruckTableTcgCard row)
        {
            var keywords = ParseKeywords(row.keywordRaw);

            return new TcgBattleDataCardInHand(
                row,
                keywords);
        }

        /// <summary>
        /// 손패의 Creature 카드 런타임을 기반으로 필드(Field)에 소환될 유닛 런타임을 생성합니다.
        /// </summary>
        /// <param name="ownerSide">소유 플레이어 진영입니다.</param>
        /// <param name="tcgBattleDataCardInHand">소환에 사용할 손패 카드 런타임입니다.</param>
        /// <returns>생성된 필드 카드(유닛) 런타임이며, 입력이 null이면 null을 반환합니다.</returns>
        /// <remarks>
        /// - 공격력/체력/키워드는 현재 손패 런타임에서 가져오는 것으로 가정합니다.
        /// - 기본적으로 소환 턴에는 공격 불가이며, <see cref="ConfigCommonTcg.TcgKeyword.Rush"/>가 있으면 예외로 공격 가능 처리합니다.
        /// </remarks>
        public static TcgBattleDataCardInField CreateBattleDataFieldCard(
            ConfigCommonTcg.TcgPlayerSide ownerSide,
            TcgBattleDataCardInHand tcgBattleDataCardInHand)
        {
            if (tcgBattleDataCardInHand == null)
            {
                GcLogger.LogError("[Battle] CreateUnitFromCard: cardRuntime is null.");
                return null;
            }

            // 1) 손패 카드 런타임에서 스탯/키워드 정보 가져오기
            //    (필드/프로퍼티 명은 실제 구현에 맞게 유지/조정 필요)
            int attack = tcgBattleDataCardInHand.attack.Value; // 예: CardRuntime.Attack
            int hp     = tcgBattleDataCardInHand.health.Value; // 예: CardRuntime.Health

            // 키워드 목록 복사(참조 공유를 피하고, 이후 변형 가능성을 열어둠)
            var keywords = new List<ConfigCommonTcg.TcgKeyword>(4);
            foreach (var kw in tcgBattleDataCardInHand.Keywords) // 예: IEnumerable<TcgKeyword>
            {
                keywords.Add(kw);
            }

            // 2) 유닛 런타임 생성
            var unit = new TcgBattleDataCardInField(
                tcgBattleDataCardInHand.Uid,
                ownerSide,
                tcgBattleDataCardInHand,
                attack,
                hp,
                keywords);

            // 소환 시점에는 기본적으로 공격 불가 (돌진 키워드가 있으면 공격 가능)
            unit.CanAttack = unit.HasKeyword(ConfigCommonTcg.TcgKeyword.Rush);

            return unit;
        }
    }
}
