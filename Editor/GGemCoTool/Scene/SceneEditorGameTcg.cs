using System.Collections.Generic;
using GGemCo2DCore;
using GGemCo2DCoreEditor;
using GGemCo2DTcg;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

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
        public void SetupRequiredObjects(EditorSetupContext ctx = null)
        {
            string sceneName = nameof(SceneGame);
            GGemCo2DCore.SceneGame scene = CreateUIComponent.Find(sceneName, ConfigPackageInfo.PackageType.Core)?.GetComponent<SceneGame>();
            if (scene == null) 
            {
                HelperLog.Error($"[{nameof(SceneEditorGameTcg)}] {nameof(SceneGame)} 이 없습니다.\nGGemCoTool > 설정하기 > 게임 씬 셋팅하기에서 필수 항목 셋팅하기를 실행해주세요.", ctx);
                return;
            }
            _objGGemCoCore = GetOrCreateRootPackageGameObject();
            
            SetupTcgPackageManager(scene, ctx);
            SetupCanvasBlackImage(scene, ctx);
            
            HelperLog.Info($"[{nameof(SceneEditorGameTcg)}] 게임 씬 필수 셋업 완료", ctx);
            // 반드시 SetDirty 처리해야 저장됨
            EditorUtility.SetDirty(scene);
        }

        private void SetupCanvasBlackImage(SceneGame scene, EditorSetupContext ctx = null)
        {
            var canvasBlack = CreateUIComponent.Find("CanvasBlack", ConfigPackageInfo.PackageType.Core);
            if (canvasBlack == null)
            {
                HelperLog.Error($"[{nameof(SceneEditorGameTcg)}] CanvasBlack 오브젝트가 없습니다.", ctx);
                return;
            }

            var image = canvasBlack.transform.GetChild(0).gameObject;
            if (image == null)
            {
                HelperLog.Error($"[{nameof(SceneEditorGameTcg)}] CanvasBlack 하위에 Image 오브젝트가 없습니다.", ctx);
                return;
            }
            var imageComponent = image.GetComponent<Image>();
            imageComponent.color = new Color(0, 0, 0, 0);
            HelperLog.Info($"[{nameof(SceneEditorGameTcg)}] CanvasBlackImage 셋업 완료", ctx);
        }

        private void SetupTcgPackageManager(SceneGame scene, EditorSetupContext ctx = null)
        {
            TcgPackageManager tcgPackageManager =
                CreateOrAddComponent<TcgPackageManager>(nameof(TcgPackageManager));
            
            // 싱글톤으로 활용하고 있어 root 로 이동
            tcgPackageManager.gameObject.transform.SetParent(null);
            HelperLog.Info($"[{nameof(SceneEditorGameTcg)}] {nameof(TcgPackageManager)} 셋업 완료", ctx);
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
                HelperLog.Error($"[{nameof(SceneEditorGameTcg)}] GGemCo_Core_Canvas 가 없습니다.", ctx);
                return;
            }

            List<UIWindow> uiWindows =  new List<UIWindow> { null };
            Dictionary<int, StruckTableWindow> dictionary = GGemCo2DCoreEditor.TableLoaderManager.LoadWindowTable().GetDatas();
            
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
                    HelperLog.Error($"[{nameof(SceneEditorGameTcg)}] {objectName} 프리팹이 없습니다.", ctx);
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
                    HelperLog.Error($"[{nameof(SceneEditorGameTcg)}] {objectName} 프리팹 생성 실패", ctx);
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
            HelperLog.Info($"[{nameof(SceneEditorGameTcg)}] UI 윈도우 셋업 완료", ctx);
        }
    }
}