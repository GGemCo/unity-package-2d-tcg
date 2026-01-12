#if UNITY_EDITOR
using GGemCo2DCoreEditor;
using UnityEditor;
using UnityEngine;

namespace GGemCo2DTcgEditor
{
    public sealed class AutoProjectSetupWindowTcg : AutoProjectSetupWindow
    {
        protected override string Title => "GGemCo TCG Project Setup";
        
        [MenuItem(ConfigEditorTcg.NameToolSettingAuto, false, (int)ConfigEditorTcg.ToolOrdering.AutoSetting)]
        public static void Open()
        {
            var window = GetWindow<AutoProjectSetupWindowTcg>();
            window.titleContent = new GUIContent(window.Title);
            window.minSize = new Vector2(720f, 520f);
            window.Show();
        }
        /// <summary>
        /// TCG용 옵션 UI를 그립니다. (베이스 옵션을 재사용하거나 문구만 변경 가능)
        /// </summary>
        protected override void DrawOptionsArea()
        {
            _setAllSampleData = HelperEditorUI.ToggleLeft(
                "샘플 TCG 셋팅",
                _setAllSampleData,
                "샘플 TCG 프로젝트에 맞는 데이터/리소스가 복사/셋업됩니다."
            );
        }
        /// <summary>
        /// 현재 옵션 상태를 기반으로 SetupStep 파이프라인을 구성합니다.
        /// </summary>
        protected override void BuildStepPipeline()
        {
            bool needSampleResources = _setAllSampleData;
            _setupSteps.Clear();

            // 1. 공통 필수 스텝
            _setupSteps.Add(new StepCreateSettingScriptableObjectTcg());
            _setupSteps.Add(new StepCopyEmptyDataTableTcg());
            // _setupSteps.Add(new StepCopyDefaultLocalizationTcg());
            _setupSteps.Add(new StepSetDefaultSettingScriptableObjectTcg());
            
            // 3. 옵션: 샘플 RPG 리소스/데이터 셋업
            if (needSampleResources)
            {
                _setupSteps.Add(new StepCopyAllSampleDataTcg());
                _setupSteps.Add(new StepSetSettingScriptableObjectTcg());
                _setupSteps.Add(new StepSetCameraTcg());
            }
            
            // 순서 중요: 씬 셋업 하기
            _setupSteps.Add(new StepSetSceneRequireObjectTcg(needSampleResources));

            // Addressables를 마지막에 등록
            _setupSteps.Add(new StepSetDefaultAddressableDataTcg());
            if (needSampleResources)
            {
                _setupSteps.Add(new StepSetAddressableDataTcg());
            }
        }
    }
}
#endif
