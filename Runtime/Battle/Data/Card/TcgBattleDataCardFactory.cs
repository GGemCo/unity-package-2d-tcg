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
        public static List<TcgEffectData> ParseEffects(string effectsRaw)
        {
            var result = new List<TcgEffectData>(4);

            if (string.IsNullOrWhiteSpace(effectsRaw))
                return result;

            var items = effectsRaw.Split(';');
            foreach (var item in items)
            {
                var trimmed = item.Trim();
                if (string.IsNullOrEmpty(trimmed))
                    continue;

                // EffectId:Value:TargetType:Extra
                var tokens = trimmed.Split(':');
                if (tokens.Length < 3)
                {
                    GcLogger.LogWarning($"[TcgCardRuntimeFactory] Invalid effect format: {trimmed}");
                    continue;
                }

                var effectIdRaw   = tokens[0].Trim();
                var valueRaw      = tokens[1].Trim();
                var targetTypeRaw = tokens[2].Trim();
                var extraRaw      = tokens.Length >= 4 ? tokens[3].Trim() : string.Empty;

                if (!Enum.TryParse<TcgEffectId>(effectIdRaw, true, out var effectId))
                {
                    GcLogger.LogWarning($"[TcgCardRuntimeFactory] Unknown EffectId: {effectIdRaw}");
                    continue;
                }

                int value = 0;
                int.TryParse(valueRaw, out value);

                var targetType = CardConstants.TargetType.None;
                if (!string.IsNullOrEmpty(targetTypeRaw))
                {
                    if (!Enum.TryParse<CardConstants.TargetType>(targetTypeRaw, true, out targetType))
                    {
                        GcLogger.LogWarning($"[TcgCardRuntimeFactory] Unknown TargetType: {targetTypeRaw}");
                        targetType = CardConstants.TargetType.None;
                    }
                }

                var extraParams = ParseExtraParams(extraRaw);

                result.Add(new TcgEffectData
                {
                    EffectId = effectId,
                    Value = value,
                    TargetType = targetType,
                    ExtraParams = extraParams
                });
            }

            return result;
        }

        /// <summary>
        /// "Key=Value&Key2=Value2" 형태를 Dictionary 로 파싱.
        /// </summary>
        private static Dictionary<string, string> ParseExtraParams(string extraRaw)
        {
            var dict = new Dictionary<string, string>();
            if (string.IsNullOrWhiteSpace(extraRaw))
                return dict;

            var pairs = extraRaw.Split('&');
            foreach (var p in pairs)
            {
                var trimmed = p.Trim();
                if (string.IsNullOrEmpty(trimmed))
                    continue;

                var kv = trimmed.Split('=');
                if (kv.Length != 2)
                    continue;

                var key = kv[0].Trim();
                var value = kv[1].Trim();
                if (!string.IsNullOrEmpty(key))
                {
                    dict[key] = value;
                }
            }

            return dict;
        }
        public static TcgBattleDataCard CreateCardRuntime(StruckTableTcgCard row)
        {
            var keywords         = ParseKeywords(row.keywordRaw);
            var summonEffects    = ParseEffects(row.summonEffectsRaw);
            var spellEffects     = ParseEffects(row.spellEffectsRaw);
            var deathrattleFx    = ParseEffects(row.deathEffectsRaw);

            return new TcgBattleDataCard(
            
                row,
                keywords,
                summonEffects,
                spellEffects,
                deathrattleFx);
        }
    }
}