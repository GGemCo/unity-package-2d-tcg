using System.Collections.Generic;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 영구 효과(Permanent)의 수명 타입에 따라 표시용 로컬라이즈 문자열을 생성하는 제공자입니다.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Unity Localization(StringDatabase/LocalizedString)을 사용하여
    /// 수명 설명 포맷 테이블에서 문자열을 조회하고, 필요한 경우 파라미터를 전달합니다.
    /// </para>
    /// <para>
    /// 각 포맷 키는 테이블(<c>LocalizationConstantsTcg.Tables.LifetimeDescription</c>)에 존재해야 합니다.
    /// </para>
    /// </remarks>
    public sealed class TcgLifetimeLocalizationProvider
    {
        // private const string TableType   = "TCG_Lifetime_Type";
        /// <summary>
        /// 수명 설명(포맷) 문자열이 정의된 로컬라이제이션 테이블 이름입니다.
        /// </summary>
        private const string TableFormat = LocalizationConstantsTcg.Tables.LifetimeDescription;

        /*
        public static string GetTypeName(ConfigCommonTcg.TcgPermanentLifetimeType type)
        {
            return LocalizationSettings.StringDatabase.GetLocalizedString(
                TableType,
                $"lifetime_{type.ToString().ToLowerInvariant()}");
        }
        */

        /// <summary>
        /// "만료 없음(무기한)" 수명 타입의 표시 문자열을 반환합니다.
        /// </summary>
        /// <returns>무기한 수명에 대한 로컬라이즈된 표시 문자열입니다.</returns>
        private static string GetIndefinite()
        {
            return LocalizationSettings.StringDatabase.GetLocalizedString(
                TableFormat,
                $"lifetime_indefinite_format");
        }

        /// <summary>
        /// "지정 턴 수" 수명 타입의 표시 문자열을 반환합니다.
        /// </summary>
        /// <param name="turns">남은 턴 수(또는 지속 턴 수)로 포맷팅에 사용될 값입니다.</param>
        /// <returns>턴 수를 포함한 로컬라이즈된 표시 문자열입니다.</returns>
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

        /// <summary>
        /// "발동 횟수" 수명 타입의 표시 문자열을 반환합니다.
        /// </summary>
        /// <param name="count">남은(또는 허용된) 발동 횟수로 포맷팅에 사용될 값입니다.</param>
        /// <returns>발동 횟수를 포함한 로컬라이즈된 표시 문자열입니다.</returns>
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

        /// <summary>
        /// "내구도" 수명 타입의 표시 문자열을 반환합니다.
        /// </summary>
        /// <param name="durability">내구도 값으로 포맷팅에 사용될 값입니다.</param>
        /// <returns>내구도 값을 포함한 로컬라이즈된 표시 문자열입니다.</returns>
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

        /// <summary>
        /// 카드 영구 효과 데이터의 수명 타입과 파라미터를 기반으로 표시용 수명 텍스트를 생성합니다.
        /// </summary>
        /// <param name="data">
        /// 수명 타입(<c>lifetimeType</c>) 및 수명 파라미터(<c>lifetimeParamA</c>)를 포함한 영구 효과 데이터입니다.
        /// </param>
        /// <returns>
        /// 수명 타입에 대응하는 로컬라이즈된 표시 문자열을 반환하며,
        /// 알 수 없는 타입인 경우 빈 문자열을 반환합니다.
        /// </returns>
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
