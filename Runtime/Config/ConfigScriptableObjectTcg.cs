using System;
using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// ScriptableObject 관련 설정 정의
    /// </summary>
    public static class ConfigScriptableObjectTcg
    {
        public const string NameBase = ConfigDefine.NameSDK + "_Tcg";
        
        private const string PathBase = ConfigDefine.NameSDK + "/Tcg";
        
        /// <summary>
        /// 메뉴 순서 정의
        /// </summary>
        public enum MenuOrdering
        {
            None,
        }
        
        public static class TcgSettings
        {
            public const string FileName = ConfigScriptableObject.BaseName + "TcgSettings";
            public const string MenuName = ConfigScriptableObject.BasePath + FileName;
            public const int Ordering = (int)ConfigScriptableObject.MenuOrdering.TcgSettings;
        }
        
        public static readonly Dictionary<string, Type> SettingsTypes = new()
        {
            { TcgSettings.FileName, typeof(GGemCoTcgSettings) },
        };
    }
}