#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using GGemCo2DCoreEditor;
using UnityEditor;
using UnityEngine;

namespace GGemCo2DTcgEditor
{
    public sealed class AutoProjectSetupWindowTcg : EditorWindow
    {
        private const string Title = "GGemCo TCG Project Setup";

        [Tooltip("예제 TCG 프로젝트에 맞는 샘플 데이터/리소스를 모두 셋업합니다.")]
        private bool _setAllSampleData;
        
        /// <summary>
        /// 실제로 실행될 SetupStep 들의 파이프라인.
        /// 매 실행마다 빌드되며, EditorWindow 인스턴스와 분리된 순수 데이터 구조로 유지합니다.
        /// </summary>
        private readonly List<SetupStepBase> _setupSteps = new List<SetupStepBase>();

        #region Menu

        [MenuItem(ConfigEditorTcg.NameToolSettingAuto, false, (int)ConfigEditorTcg.ToolOrdering.AutoSetting)]
        public static void Open()
        {
            var window = GetWindow<AutoProjectSetupWindowTcg>(Title);
            window.minSize = new Vector2(520f, 360f);
            window.Show();
        }

        #endregion

        #region Unity Callbacks

        private void OnGUI()
        {
            DrawHeader();
            EditorGUILayout.Space(4);

            DrawOptions();
            EditorGUILayout.Space(10);

            DrawButtons();
        }

        #endregion

        #region GUI

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("프로젝트에 필요한 필수 초기 구성을 자동으로 셋업합니다.");
        }

        private void DrawOptions()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                // 샘플 데이터/리소스 셋업
                _setAllSampleData = HelperEditorUI.ToggleLeft(
                    "샘플 TCG 셋팅",
                    _setAllSampleData,
                    "샘플 TCG 프로젝트에 맞는 데이터 테이블 및 리소스가 복사/셋업됩니다."
                );
            }
        }

        private void DrawButtons()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("유효성 검사"))
                {
                    EditorApplication.delayCall += () =>
                    {
                        Run(validateOnly: true);
                    };
                    GUIUtility.ExitGUI(); // 현재 OnGUI 루프 안전 종료
                }

                if (GUILayout.Button("자동 셋팅 시작"))
                {
                    EditorApplication.delayCall += () =>
                    {
                        Run(validateOnly: false);
                    };
                    GUIUtility.ExitGUI();
                }

                if (GUILayout.Button("로그파일 폴더 열기"))
                {
                    OpenGameDataFolder();
                }
            }
        }
        #endregion

        #region Pipeline Build

        /// <summary>
        /// 현재 옵션 상태를 기반으로 SetupStep 파이프라인을 구성합니다.
        /// </summary>
        private void BuildStepPipeline()
        {
            _setupSteps.Clear();

            // 1. 공통 필수 스텝
            _setupSteps.Add(new StepCreateSettingScriptableObjectTcg());
            _setupSteps.Add(new StepSetSceneRequireObjectTcg());
            _setupSteps.Add(new StepCopyEmptyDataTableTcg());
            // _setupSteps.Add(new StepCopyDefaultLocalizationTcg());
            _setupSteps.Add(new StepSetDefaultUI());
            _setupSteps.Add(new StepSetDefaultSettingScriptableObjectTcg());
            
            // 3. 옵션: 샘플 RPG 리소스/데이터 셋업
            bool needSampleResources = _setAllSampleData;
            if (needSampleResources)
            {
                _setupSteps.Add(new StepCopyAllSampleDataTcg());
                _setupSteps.Add(new StepInstantiateUIWindowsFromTableTcg());
                _setupSteps.Add(new StepSetSettingScriptableObjectTcg());
                _setupSteps.Add(new StepSetCameraTcg());
            }

            // Addressables를 마지막에 등록
            _setupSteps.Add(new StepSetDefaultAddressableDataTcg());
            if (needSampleResources)
            {
                _setupSteps.Add(new StepSetAddressableDataTcg());
            }
        }

        #endregion

        #region Run & Validate

        private void Run(bool validateOnly)
        {
            // 파이프라인 구성
            BuildStepPipeline();

            var steps = _setupSteps
                .Where(s => s is { enabledStep: true })
                .OrderBy(s => s.order)
                .ToArray();

            if (steps.Length == 0)
            {
                EditorUtility.DisplayDialog(Title, "활성화된 스텝이 없습니다.", "OK");
                return;
            }

            int progressId = Progress.Start("GGemCo Project Setup", "Initializing...");

            using var logger = new EditorSetupLogger();
            var ctx = new EditorSetupContext(logger);
            logger.Info($"Steps: {steps.Length}");

            try
            {
                // 1) Validate Phase
                int stepCount = steps.Length;
                for (int i = 0; i < stepCount; i++)
                {
                    var step = steps[i];
                    float pct = (float)i / stepCount;
                    Progress.Report(progressId, pct, $"Validate: {step}");

                    if (!step.Validate(ctx, out var msg))
                    {
                        // Validate 실패는 경고로만 남기고 계속 진행 (설정 상황에 따라 허용)
                        logger.Warn($"[Validate] {step} :: {msg}");
                    }
                }

                if (validateOnly)
                {
                    logger.Info("[Result] Validate Only 완료");
                    EditorUtility.DisplayDialog(Title, "Validate Only 완료(자세한 내용은 로그 참조)", "OK");
                    return;
                }

                // 2) Execute Phase
                for (int i = 0; i < stepCount; i++)
                {
                    var step = steps[i];
                    float pct = (float)(i + 1) / stepCount;
                    Progress.Report(progressId, pct, $"Run: {step}");

                    try
                    {
                        logger.Info($"[Run] {step}");
                        step.Execute(ctx);
                        logger.Info($"[OK ] {step}");
                    }
                    catch (Exception ex)
                    {
                        // 개별 스텝 실패는 로그에 남기고 다음 스텝 계속 수행
                        logger.Error($"[FAIL] {step} :: {ex}");
                    }
                }

                // 3) 열린 씬 저장
                UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
                logger.Info("[Save] Open Scenes saved.");

                logger.Info($"[Done] Log: {logger.LogPath}");
                EditorUtility.DisplayDialog(Title, $"완료\nLog: {logger.LogPath}", "OK");
            }
            finally
            {
                Progress.Remove(progressId);
                EditorUtility.ClearProgressBar();
            }
        }

        #endregion

        private void OpenGameDataFolder()
        {
            string path = ConfigProjectSetup.DirLog;

            if (Directory.Exists(path))
            {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                Process.Start(new ProcessStartInfo()
                {
                    FileName = path,
                    UseShellExecute = true
                });
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                Process.Start("open", path);
#endif
            }
            else
            {
                UnityEngine.Debug.LogError($"폴더를 찾을 수 없습니다: {path}");
            }
        }

    }
}
#endif
