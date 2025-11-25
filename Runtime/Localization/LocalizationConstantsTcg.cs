using GGemCo2DCore;

namespace GGemCo2DTcg
{
    public static class LocalizationConstantsTcg
    {
        /// <summary>
        /// Localization Table 이름 정의
        /// </summary>
        public static class Tables
        {
            public const string UIWindowGameMenu = ConfigDefine.NameSDK+"_UIWindowTcgGameMenu";
            public const string UIWindowCardInfo = ConfigDefine.NameSDK+"_UIWindowTcgCardInfo";
            
            public const string CardName = ConfigDefine.NameSDK+"_Tcg_Card_Name";
            
            /// <summary>
            /// 모든 테이블 이름을 배열로 제공합니다.
            /// </summary>
            public static readonly string[] All = new[]
            {
                UIWindowGameMenu,
                UIWindowCardInfo,
                CardName,
            };

        }

        /// <summary>
        /// Localization Key 값 정의
        /// </summary>
        public static class Keys
        {
            private const string NameButton = "Button";
            private const string NameText = "Text";
            
        }
    }
}
