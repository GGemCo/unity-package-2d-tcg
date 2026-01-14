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
    /// 게임(SceneGame) 씬의 필수/선택 오브젝트를 자동 구성하는 에디터 윈도우입니다.
    /// </summary>
    /// <remarks>
    /// 현재 열린 씬이 게임 씬인지 검사한 뒤, 필수 구성(TcgPackageManager, CanvasBlack 등) 및
    /// 테스트용 UI 윈도우(테이블 기반 프리팹 인스턴스)를 일괄 셋업합니다.
    /// </remarks>
    public class SceneEditorGameTcg : DefaultSceneEditorTcg
    {
        /// <summary>
        /// 에디터 윈도우 타이틀입니다.
        /// </summary>
        private const string Title = "게임 씬 셋팅하기";

        /// <summary>
        /// 패키지 루트(GameObject) 캐시입니다. (필수 셋업 과정에서 생성/획득)
        /// </summary>
        private GameObject _objGGemCoCore;

        /// <summary>
        /// 메뉴에서 게임 씬 셋업 윈도우를 엽니다.
        /// </summary>
        [MenuItem(ConfigEditorTcg.NameToolSettingSceneGame, false, (int)ConfigEditorTcg.ToolOrdering.SettingSceneGame)]
        public static void ShowWindow()
        {
            GetWindow<SceneEditorGameTcg>(Title);
        }

        /// <summary>
        /// 에디터 윈도우 GUI를 렌더링합니다.
        /// </summary>
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

        /// <summary>
        /// 필수 항목 섹션 UI를 그립니다.
        /// </summary>
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
        /// 게임 씬에서 반드시 필요한 오브젝트/컴포넌트를 셋업합니다.
        /// </summary>
        /// <param name="ctx">로그/에러 출력에 사용할 셋업 컨텍스트(선택)입니다.</param>
        /// <remarks>
        /// - SceneGame 존재 여부를 확인합니다.
        /// - 패키지 루트 오브젝트를 확보합니다.
        /// - <see cref="TcgPackageManager"/> 및 CanvasBlack Image 알파값을 초기화합니다.
        /// </remarks>
        public void SetupRequiredObjects(EditorSetupContext ctx = null)
        {
            string sceneName = nameof(SceneGame);
            GGemCo2DCore.SceneGame scene = CreateUIComponent.Find(sceneName, ConfigPackageInfo.PackageType.Core)?.GetComponent<SceneGame>();
            if (scene == null)
            {
                HelperLog.Error(
                    $"[{nameof(SceneEditorGameTcg)}] {nameof(SceneGame)} 이 없습니다.\n" +
                    "GGemCoTool > 설정하기 > 게임 씬 셋팅하기에서 필수 항목 셋팅하기를 실행해주세요.",
                    ctx);
                return;
            }

            _objGGemCoCore = GetOrCreateRootPackageGameObject();

            SetupTcgPackageManager(scene, ctx);
            SetupCanvasBlackImage(scene, ctx);

            HelperLog.Info($"[{nameof(SceneEditorGameTcg)}] 게임 씬 필수 셋업 완료", ctx);

            // 반드시 SetDirty 처리해야 저장됨
            EditorUtility.SetDirty(scene);
        }

        /// <summary>
        /// CanvasBlack 하위 Image의 색상을 투명(알파 0)으로 초기화합니다.
        /// </summary>
        /// <param name="scene">현재 씬의 <see cref="SceneGame"/> 컴포넌트입니다.</param>
        /// <param name="ctx">로그/에러 출력에 사용할 셋업 컨텍스트(선택)입니다.</param>
        /// <remarks>
        /// CanvasBlack 오브젝트의 첫 번째 자식에 Image가 존재한다는 전제에 의존합니다.
        /// </remarks>
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

        /// <summary>
        /// <see cref="TcgPackageManager"/>를 생성/추가하고 루트(씬 최상단)로 이동시킵니다.
        /// </summary>
        /// <param name="scene">현재 씬의 <see cref="SceneGame"/> 컴포넌트입니다.</param>
        /// <param name="ctx">로그/에러 출력에 사용할 셋업 컨텍스트(선택)입니다.</param>
        /// <remarks>
        /// 싱글톤처럼 사용한다는 전제 하에 parent를 null로 설정합니다.
        /// </remarks>
        private void SetupTcgPackageManager(SceneGame scene, EditorSetupContext ctx = null)
        {
            TcgPackageManager tcgPackageManager =
                CreateOrAddComponent<TcgPackageManager>(nameof(TcgPackageManager));

            // 싱글톤으로 활용하고 있어 root 로 이동
            tcgPackageManager.gameObject.transform.SetParent(null);
            HelperLog.Info($"[{nameof(SceneEditorGameTcg)}] {nameof(TcgPackageManager)} 셋업 완료", ctx);
        }

        /// <summary>
        /// 선택 항목 섹션 UI를 그립니다.
        /// </summary>
        private void DrawOptionalSection()
        {
            HelperEditorUI.OnGUITitle("선택 항목");
            if (GUILayout.Button("TCG 테스트 윈도우 셋팅하기"))
            {
                SetupAllTestWindow();
            }
        }

        /// <summary>
        /// 필수 셋업을 수행한 뒤 <see cref="UIWindowManager"/>를 생성/연결합니다.
        /// </summary>
        /// <returns>생성/획득한 <see cref="UIWindowManager"/>이며, 실패 시 null입니다.</returns>
        /// <remarks>
        /// SceneGame과 UIWindowManager를 생성/획득 후 scene에 연결합니다.
        /// </remarks>
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

        /// <summary>
        /// 윈도우 테이블을 기반으로 게임 씬의 테스트용 UI 윈도우들을 일괄 생성/배치하고 <see cref="UIWindowManager"/>에 등록합니다.
        /// </summary>
        /// <param name="ctx">로그/에러 출력에 사용할 셋업 컨텍스트(선택)입니다.</param>
        /// <remarks>
        /// - 필수 셋업을 먼저 수행합니다.
        /// - Canvas 오브젝트 하위로 각 UIWindow 프리팹을 인스턴스화합니다.
        /// - 이미 씬에 존재하면 재생성하지 않고 참조만 수집합니다.
        /// - Option 윈도우는 패널 프리팹 리스트를 자동 채우는 후처리를 수행합니다.
        /// </remarks>
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

            // NOTE: 인덱스/UID 매핑을 고려해 0번(또는 placeholder) 자리를 비워두는 구조로 보입니다.
            List<UIWindow> uiWindows = new List<UIWindow> { null };

            Dictionary<int, StruckTableWindow> dictionary =
                GGemCo2DCoreEditor.TableLoaderManager.LoadWindowTable(true).GetDatas();

            foreach (KeyValuePair<int, StruckTableWindow> outerPair in dictionary)
            {
                var info = outerPair.Value;
                if (info.Uid <= 0) continue;

                if (!info.UseInGame)
                {
                    // 게임 씬에서 사용하지 않는 윈도우는 자리만 유지합니다.
                    uiWindows.Add(null);
                    continue;
                }

                string objectName = info.PrefabName;

                GameObject prefab = FindPrefabUIWindowByName(objectName);
                if (!prefab)
                {
                    HelperLog.Error($"[{nameof(SceneEditorGameTcg)}] {objectName} 프리팹이 없습니다.", ctx);
                    continue;
                }

                // 이미 존재하는 윈도우는 재생성하지 않고 수집합니다.
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

                // 프리팹 해제(완전 Unpack)하여 씬 오브젝트로 고정합니다.
                PrefabUtility.UnpackPrefabInstance(
                    gameObject,
                    PrefabUnpackMode.Completely,
                    InteractionMode.UserAction
                );

                if (info.Uid == (int)UIWindowConstants.WindowUid.Option)
                {
                    // UIPanelOptionBase 프리팹을 자동으로 listPrefabPanel 에 등록
                    AutoFillPanelPrefabs(window);
                }

                HelperLog.Info($"[{nameof(SceneEditorGameTcg)}] {objectName} 윈도우 셋업", ctx);
            }

            uiWindowManager.SetUIWindow(uiWindows.ToArray());
            scene.SetUIWindowManager(uiWindowManager);
            HelperLog.Info($"[{nameof(SceneEditorGameTcg)}] UI 윈도우 셋업 완료", ctx);
        }
    }
}
