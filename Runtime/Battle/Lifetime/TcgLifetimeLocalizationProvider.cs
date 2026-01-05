using System.Collections.Generic;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace GGemCo2DTcg
{
    public sealed class TcgLifetimeLocalizationProvider
    {
        // private const string TableType   = "TCG_Lifetime_Type";
        private const string TableFormat = LocalizationConstantsTcg.Tables.LifetimeDescription;

        /*
        public static string GetTypeName(ConfigCommonTcg.TcgPermanentLifetimeType type)
        {
            return LocalizationSettings.StringDatabase.GetLocalizedString(
                TableType,
                $"lifetime_{type.ToString().ToLowerInvariant()}");
        }
        */

        private static string GetIndefinite()
        {
            return LocalizationSettings.StringDatabase.GetLocalizedString(
                TableFormat,
                $"lifetime_indefinite_format");
        }
        private static string GetDurationTurns(int turns)
        {
            Dictionary<string, object> arg = new Dictionary<string, object>
            {
                { "Turns", turns },
            };
            var abilityString = new LocalizedString(TableFormat, "lifetime_duration_turns_format")
            {
                Arguments = new object[] { arg }
            };
            var result = abilityString.GetLocalizedString();
            return result;
        }

        private static string GetTriggerCount(int count)
        {
            Dictionary<string, object> arg = new Dictionary<string, object>
            {
                { "Count", count },
            };
            var abilityString = new LocalizedString(TableFormat, "lifetime_trigger_count_format")
            {
                Arguments = new object[] { arg }
            };
            var result = abilityString.GetLocalizedString();
            return result;
        }

        private static string GetDurability(int durability)
        {
            Dictionary<string, object> arg = new Dictionary<string, object>
            {
                { "Durability", durability },
            };
            var abilityString = new LocalizedString(TableFormat, "lifetime_durability_format")
            {
                Arguments = new object[] { arg }
            };
            var result = abilityString.GetLocalizedString();
            return result;
        }
        public string BuildLifetimeText(StruckTableTcgCardPermanent data)
        {
            switch (data.lifetimeType)
            {
                case ConfigCommonTcg.TcgPermanentLifetimeType.Indefinite:
                    return GetIndefinite();

                case ConfigCommonTcg.TcgPermanentLifetimeType.DurationTurns:
                    return GetDurationTurns(data.lifetimeParamA);

                case ConfigCommonTcg.TcgPermanentLifetimeType.TriggerCount:
                    return GetTriggerCount(data.lifetimeParamA);

                case ConfigCommonTcg.TcgPermanentLifetimeType.Durability:
                    return GetDurability(data.lifetimeParamA);

                default:
                    return string.Empty;
            }
        }
    }
}