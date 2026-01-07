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
        private const string BaseName = ConfigDefine.NameSDK + ConfigPackageInfo.NamePackageTcg;
        
        private const string BasePath = ConfigDefine.NameSDK + "/" + ConfigPackageInfo.NamePackageTcg + "/";
        private const string BasePathSettings = BasePath + "Settings/";
        
        /// <summary>
        /// 메뉴 순서 정의
        /// </summary>
        private enum MenuOrdering
        {
            None,
            TcgSettings,
            TcgUICutsceneSettings,
            TcgWeightShuffleSettings,
            TcgPhaseShuffleSettings,
            TcgAiPreset,
        }
        
        public static class TcgSettings
        {
            public const string FileName = BaseName + "Settings";
            public const string MenuName = BasePathSettings + FileName;
            public const int Ordering = (int)MenuOrdering.TcgSettings;
        }
        public static class TcgUICutsceneSettings
        {
            public const string FileName = BaseName + "UICutsceneSettings";
            public const string MenuName = BasePathSettings + FileName;
            public const int Ordering = (int)MenuOrdering.TcgUICutsceneSettings;
        }
        public static class TcgWeightShuffleSettings
        {
            public const string FileName = BaseName + "WeightShuffleSettings";
            public const string MenuName = BasePathSettings + FileName;
            public const int Ordering = (int)MenuOrdering.TcgWeightShuffleSettings;
        }
        public static class TcgPhaseShuffleSettings
        {
            public const string FileName = BaseName + "PhaseShuffleSettings";
            public const string MenuName = BasePathSettings + FileName;
            public const int Ordering = (int)MenuOrdering.TcgPhaseShuffleSettings;
        }
        public static class TcgAiPreset
        {
            public const string FileName = BaseName + "AiDeckPreset";
            public const string MenuName = BasePath + FileName;
            public const int Ordering = (int)MenuOrdering.TcgAiPreset;
        }
        
        public static readonly Dictionary<string, Type> SettingsTypes = new()
        {
            { TcgSettings.FileName, typeof(GGemCoTcgSettings) },
        };
    }
}