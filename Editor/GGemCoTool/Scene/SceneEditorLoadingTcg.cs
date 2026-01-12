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
    public class SceneEditorLoadingTcg : DefaultSceneEditorTcg
    {
        private const string Title = "로딩 씬 셋팅하기";
        private GameObject _objGGemCoCore;
        
        [MenuItem(ConfigEditorTcg.NameToolSettingSceneLoading, false, (int)ConfigEditorTcg.ToolOrdering.SettingSceneLoading)]
        public static void ShowWindow()
        {
            GetWindow<SceneEditorLoadingTcg>(Title);
        }

        private void OnGUI()
        {
            if (!CheckCurrentLoadedScene(ConfigDefine.SceneNameLoading))
            {
                EditorGUILayout.HelpBox($"로딩 씬을 불러와 주세요.", MessageType.Error);
            }
            else
            {
                DrawRequiredSection();
            }
        }
        private void DrawRequiredSection()
        {
            HelperEditorUI.OnGUITitle("필수 항목");
            EditorGUILayout.HelpBox($"* SceneLoadingTcg 오브젝트\n", MessageType.Info);
            if (GUILayout.Button("필수 항목 셋팅하기"))
            {
                SetupRequiredObjects();
            }
        }
        /// <summary>
        /// 필수 항목 셋팅
        /// </summary>
        public void SetupRequiredObjects(EditorSetupContext ctx = null)
        {
            string sceneName = nameof(SceneLoading);
            GGemCo2DCore.SceneLoading scene = CreateUIComponent.Find(sceneName, ConfigPackageInfo.PackageType.Core)?.GetComponent<SceneLoading>();
            if (scene == null) 
            {
                HelperLog.Error($"[{nameof(SceneEditorLoadingTcg)}] {sceneName} 이 없습니다.\nGGemCoTool > 설정하기 > 로딩 씬 셋팅하기에서 필수 항목 셋팅하기를 실행해주세요.", ctx);
                return;
            }
            _objGGemCoCore = GetOrCreateRootPackageGameObject();
            // GGemCo2DCore.SceneLoading GameObject 만들기
            GGemCo2DTcg.SceneLoadingTcg sceneLoadingTcg =
                CreateOrAddComponent<GGemCo2DTcg.SceneLoadingTcg>(nameof(GGemCo2DTcg.SceneLoadingTcg));

            HelperLog.Info($"[{nameof(SceneEditorLoadingTcg)}] 로딩 씬 필수 셋업 완료", ctx);
            // 반드시 SetDirty 처리해야 저장됨
            EditorUtility.SetDirty(scene);
        }
    }
}