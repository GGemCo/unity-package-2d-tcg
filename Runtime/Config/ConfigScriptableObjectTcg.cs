using System;
using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// TCG 패키지에서 사용하는 ScriptableObject의 생성 규칙(파일명/메뉴 경로/정렬)을 정의합니다.
    /// Unity 에디터의 CreateAssetMenu 경로, 파일 네이밍, 메뉴 표시 순서를 단일 소스로 관리합니다.
    /// </summary>
    public static class ConfigScriptableObjectTcg
    {
        /// <summary>
        /// ScriptableObject 파일명에 사용되는 공통 접두사입니다.
        /// </summary>
        private const string BaseName = ConfigDefine.NameSDK + ConfigPackageInfo.NamePackageTcg;

        /// <summary>
        /// Unity 메뉴 경로의 루트(패키지 경로)입니다.
        /// </summary>
        private const string BasePath = ConfigDefine.NameSDK + "/" + ConfigPackageInfo.NamePackageTcg + "/";

        /// <summary>
        /// 설정(Settings) 카테고리 하위의 Unity 메뉴 경로입니다.
        /// </summary>
        private const string BasePathSettings = BasePath + "Settings/";

        /// <summary>
        /// Unity 에디터 메뉴 정렬 순서를 정의합니다.
        /// 값이 낮을수록 상단에 표시됩니다.
        /// </summary>
        private enum MenuOrdering
        {
            /// <summary>
            /// 미지정 값입니다.
            /// </summary>
            None,

            /// <summary>
            /// TCG 공통 설정 메뉴 순서입니다.
            /// </summary>
            TcgSettings,

            /// <summary>
            /// TCG UI 컷신 설정 메뉴 순서입니다.
            /// </summary>
            TcgUICutsceneSettings,

            /// <summary>
            /// 가중치 셔플(Weighted Shuffle) 설정 메뉴 순서입니다.
            /// </summary>
            TcgWeightShuffleSettings,

            /// <summary>
            /// 페이즈 기반 셔플(Phase Shuffle) 설정 메뉴 순서입니다.
            /// </summary>
            TcgPhaseShuffleSettings,

            /// <summary>
            /// AI 덱 프리셋 메뉴 순서입니다.
            /// </summary>
            TcgAiPreset,
        }

        /// <summary>
        /// TCG 공통 설정(ScriptableObject) 정의입니다.
        /// </summary>
        public static class TcgSettings
        {
            /// <summary>
            /// 에셋 파일명(확장자 제외)입니다.
            /// </summary>
            public const string FileName = BaseName + "Settings";

            /// <summary>
            /// Unity Create 메뉴에 표시될 경로/이름입니다.
            /// </summary>
            public const string MenuName = BasePathSettings + FileName;

            /// <summary>
            /// Unity Create 메뉴 정렬 순서 값입니다.
            /// </summary>
            public const int Ordering = (int)MenuOrdering.TcgSettings;
        }

        /// <summary>
        /// TCG UI 컷신 관련 설정(ScriptableObject) 정의입니다.
        /// </summary>
        public static class TcgUICutsceneSettings
        {
            public const string FileName = BaseName + "UICutsceneSettings";
            public const string MenuName = BasePathSettings + FileName;
            public const int Ordering = (int)MenuOrdering.TcgUICutsceneSettings;
        }

        /// <summary>
        /// 가중치 셔플(Weighted Shuffle) 설정(ScriptableObject) 정의입니다.
        /// </summary>
        public static class TcgWeightShuffleSettings
        {
            public const string FileName = BaseName + "WeightShuffleSettings";
            public const string MenuName = BasePathSettings + FileName;
            public const int Ordering = (int)MenuOrdering.TcgWeightShuffleSettings;
        }

        /// <summary>
        /// 페이즈 기반 셔플(Phase Shuffle) 설정(ScriptableObject) 정의입니다.
        /// </summary>
        public static class TcgPhaseShuffleSettings
        {
            public const string FileName = BaseName + "PhaseShuffleSettings";
            public const string MenuName = BasePathSettings + FileName;
            public const int Ordering = (int)MenuOrdering.TcgPhaseShuffleSettings;
        }

        /// <summary>
        /// AI 덱 프리셋(ScriptableObject) 정의입니다.
        /// </summary>
        public static class TcgAiPreset
        {
            public const string FileName = BaseName + "AiDeckPreset";

            /// <summary>
            /// AI 프리셋은 Settings 카테고리와 분리된 최상위(패키지 루트) 메뉴에 배치합니다.
            /// </summary>
            public const string MenuName = BasePath + FileName;

            public const int Ordering = (int)MenuOrdering.TcgAiPreset;
        }

        /// <summary>
        /// 파일명(확장자 제외)과 실제 ScriptableObject 타입을 매핑합니다.
        /// 에디터 유틸/자동 생성/검증 로직에서 파일명으로 타입을 찾는 용도로 사용됩니다.
        /// </summary>
        public static readonly Dictionary<string, Type> SettingsTypes = new()
        {
            { TcgSettings.FileName, typeof(GGemCoTcgSettings) },
            { TcgUICutsceneSettings.FileName, typeof(GGemCoTcgUICutsceneSettings) },
        };
    }
}
