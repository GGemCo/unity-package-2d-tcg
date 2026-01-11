using GGemCo2DCore;
using GGemCo2DCoreEditor;
using GGemCo2DTcgEditor;
using UnityEditor;
using UnityEngine;

namespace GGemCo2DTcgEditor
{
    /// <summary>
    /// 인트로 씬 설정 툴
    /// </summary>
    public class SceneEditorPreIntroTcg : DefaultSceneEditorTcg
    {
        private const string Title = "Pre 인트로 씬 셋팅하기";
        private GameObject _objGGemCoCore;
        
        [MenuItem(ConfigEditorTcg.NameToolSettingScenePreIntro, false, (int)ConfigEditorTcg.ToolOrdering.SettingScenePreIntro)]
        public static void ShowWindow()
        {
            GetWindow<SceneEditorPreIntroTcg>(Title);
        }

        private void OnGUI()
        {
            if (!CheckCurrentLoadedScene(ConfigDefine.SceneNamePreIntro))
            {
                EditorGUILayout.HelpBox($"Pre 인트로 씬을 불러와 주세요.", MessageType.Error);
            }
            else
            {
                DrawRequiredSection();
            }
        }
        private void DrawRequiredSection()
        {
            HelperEditorUI.OnGUITitle("필수 항목");
            EditorGUILayout.HelpBox($"* GameLoaderManagerTcg 오브젝트\n", MessageType.Info);
            if (GUILayout.Button("필수 항목 셋팅하기"))
            {
                SetupRequiredObjects();
            }
        }
        /// <summary>
        /// 필수 항목 셋팅
        /// </summary>
        public void SetupRequiredObjects()
        {
            string sceneName = nameof(ScenePreIntro);
            GGemCo2DCore.ScenePreIntro scene = CreateUIComponent.Find(sceneName, ConfigPackageInfo.PackageType.Core)?.GetComponent<ScenePreIntro>();
            if (scene == null) 
            {
                GcLogger.LogError($"{sceneName} 이 없습니다.\nGGemCoTool > 설정하기 > Pre인트로 씬 셋팅하기에서 필수 항목 셋팅하기를 실행해주세요.");
                return;
            }
            _objGGemCoCore = GetOrCreateRootPackageGameObject();
            // GGemCo2DCore.ScenePreIntro GameObject 만들기
            GGemCo2DTcg.GameLoaderManagerTcg gameLoaderManagerTcg =
                CreateOrAddComponent<GGemCo2DTcg.GameLoaderManagerTcg>(nameof(GGemCo2DTcg.GameLoaderManagerTcg));

            // 반드시 SetDirty 처리해야 저장됨
            EditorUtility.SetDirty(scene);
        }
    }
}