using System.Collections.Generic;
using GGemCo2DCore;
using GGemCo2DCoreEditor;
using UnityEditor;
using UnityEngine;

namespace GGemCo2DTcgEditor
{
    /// <summary>
    /// 인트로 씬 설정 툴
    /// </summary>
    public class SceneEditorGameTcg : DefaultSceneEditorTcg
    {
        private const string Title = "게임 씬 셋팅하기";
        private GameObject _objGGemCoCore;
        
        [MenuItem(ConfigEditorTcg.NameToolSettingSceneGame, false, (int)ConfigEditorTcg.ToolOrdering.SettingSceneGame)]
        public static void ShowWindow()
        {
            GetWindow<SceneEditorGameTcg>(Title);
        }

        private void OnGUI()
        {
            if (!CheckCurrentLoadedScene(ConfigDefine.SceneNameGame))
            {
                EditorGUILayout.HelpBox($"게임 씬을 불러와 주세요.", MessageType.Error);
            }
            else
            {
                DrawRequiredSection();
                HelperEditorUI.GUILine();
                DrawOptionalSection();
            }
        }
        private void DrawRequiredSection()
        {
            HelperEditorUI.OnGUITitle("필수 항목");
            EditorGUILayout.HelpBox($"* TcgPackageManager 오브젝트\n", MessageType.Info);
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
            string sceneName = nameof(SceneGame);
            GGemCo2DCore.SceneGame scene = CreateUIComponent.Find(sceneName, ConfigPackageInfo.PackageType.Core)?.GetComponent<SceneGame>();
            if (scene == null) 
            {
                GcLogger.LogError($"{sceneName} 이 없습니다.\nGGemCoTool > 설정하기 > 게임 씬 셋팅하기에서 필수 항목 셋팅하기를 실행해주세요.");
                return;
            }
            _objGGemCoCore = GetOrCreateRootPackageGameObject();
            // GGemCo2DControl.ControlPackageManager GameObject 만들기
            GGemCo2DTcg.TcgPackageManager tcgPackageManager =
                CreateOrAddComponent<GGemCo2DTcg.TcgPackageManager>(nameof(GGemCo2DTcg.TcgPackageManager));
            
            // ControlPackageManager 은 싱글톤으로 활용하고 있어 root 로 이동
            tcgPackageManager.gameObject.transform.SetParent(null);
            
            // 반드시 SetDirty 처리해야 저장됨
            EditorUtility.SetDirty(scene);
        }

        /// <summary>
        /// 옵션 항목 셋팅 하기
        /// </summary>
        private void DrawOptionalSection()
        {
            HelperEditorUI.OnGUITitle("선택 항목");
            if (GUILayout.Button("TCG 테스트 윈도우 셋팅하기"))
            {
                SetupAllTestWindow();
            }
        }
        private UIWindowManager SetupWindowManager()
        {
            SetupRequiredObjects();
            
            SceneGame scene = CreateOrAddComponent<SceneGame>("SceneGame", ConfigPackageInfo.PackageType.Core);
            if (scene == null) return null;
            UIWindowManager uiWindowManager = CreateOrAddComponent<UIWindowManager>("UIWindowManager", ConfigPackageInfo.PackageType.Core);
            if (!uiWindowManager) return null;
            scene.SetUIWindowManager(uiWindowManager);
            return uiWindowManager;
        }
        public void SetupAllTestWindow(EditorSetupContext ctx = null)
        {
            SetupRequiredObjects();
            
            SceneGame scene = CreateOrAddComponent<SceneGame>("SceneGame", ConfigPackageInfo.PackageType.Core);
            if (scene == null) return;
            UIWindowManager uiWindowManager = SetupWindowManager();
            if (!uiWindowManager) return;
            
            GameObject canvas = CreateUIComponent.Find("Canvas", ConfigPackageInfo.PackageType.Core);
            if (canvas == null)
            {
                Debug.LogError("GGemCo_Core_Canvas 가 없습니다.");
                return;
            }

            List<UIWindow> uiWindows =  new List<UIWindow> { null };
            Dictionary<int, StruckTableWindow> dictionary = tableLoaderManager.LoadWindowTable().GetDatas();
            
            foreach (KeyValuePair<int, StruckTableWindow> outerPair in dictionary)
            {
                var info = outerPair.Value;
                if (info.Uid <= 0) continue;
                if (!info.UseInGame)
                {
                    uiWindows.Add(null);
                    continue;
                }
                string objectName = info.PrefabName;
                
                GameObject prefab = FindPrefabByName(ConfigEditor.PathUIWindow, objectName);
                if (!prefab)
                {
                    Debug.LogError($"{objectName} 프리팹이 없습니다.");
                    continue;
                }
                
                GameObject gameObject = GameObject.Find(objectName);
                UIWindow window;
                if (gameObject)
                {
                    window = gameObject.GetComponent<UIWindow>();
                    if (window)
                    {
                        uiWindows.Add(window);
                    }
                    continue;
                }
                
                // 프리팹 인스턴스화
                gameObject = PrefabUtility.InstantiatePrefab(prefab, canvas.transform) as GameObject;
                if (!gameObject)
                {
                    Debug.LogError("프리팹 인스턴스 생성 실패");
                    continue;
                }

                window = gameObject.GetComponent<UIWindow>();
                if (window)
                {
                    uiWindows.Add(window);
                }
                gameObject.name = objectName;
                // 프리팹 해제
                PrefabUtility.UnpackPrefabInstance(
                    gameObject,
                    PrefabUnpackMode.Completely,
                    InteractionMode.UserAction
                );

                if (info.Uid == (int)UIWindowConstants.WindowUid.Option)
                {
                    //  UIPanelOptionBase 프리팹을 자동으로 listPrefabPanel 에 등록
                    AutoFillPanelPrefabs(window);
                }
            }

            uiWindowManager.SetUIWindow(uiWindows.ToArray());
            scene.SetUIWindowManager(uiWindowManager);
        }
    }
}