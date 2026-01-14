#if UNITY_EDITOR
using GGemCo2DCoreEditor;
using UnityEditor;
using UnityEngine;

namespace GGemCo2DTcgEditor
{
    /// <summary>
    /// GGemCo TCG 프로젝트 초기 셋업을 자동화하는 에디터 윈도우입니다.
    /// 선택한 옵션에 따라 SetupStep 파이프라인을 구성하고 순차 실행합니다.
    /// </summary>
    /// <remarks>
    /// 기본 베이스(<see cref="AutoProjectSetupWindow"/>)의 UI/실행 흐름을 재사용하며,
    /// TCG 프로젝트에 맞는 옵션 및 스텝 구성을 오버라이드합니다.
    /// </remarks>
    public class AutoProjectSetupWindowTcg : AutoProjectSetupWindow
    {
        /// <summary>
        /// 에디터 윈도우 타이틀 문자열입니다.
        /// </summary>
        protected override string Title => "GGemCo TCG Project Setup";

        /// <summary>
        /// 메뉴에서 이 셋업 윈도우를 엽니다.
        /// </summary>
        /// <remarks>
        /// 베이스 클래스에도 동일한 이름의 정적 메서드가 있을 수 있으므로 <c>new</c>로 숨깁니다.
        /// </remarks>
        [MenuItem(ConfigEditorTcg.NameToolSettingAuto, false, (int)ConfigEditorTcg.ToolOrdering.AutoSetting)]
        public new static void Open()
        {
            var window = GetWindow<AutoProjectSetupWindowTcg>();
            window.titleContent = new GUIContent(window.Title);
            window.minSize = new Vector2(720f, 520f);
            window.Show();
        }

        /// <summary>
        /// TCG 전용 옵션 UI를 그립니다.
        /// 현재는 “샘플 TCG 셋팅” 토글에 따라 샘플 데이터/리소스 복사 및 추가 스텝이 활성화됩니다.
        /// </summary>
        protected override void DrawOptionsArea()
        {
            setAllSampleData = HelperEditorUI.ToggleLeft(
                "샘플 TCG 셋팅",
                setAllSampleData,
                "샘플 TCG 프로젝트에 맞는 데이터/리소스가 복사/셋업됩니다."
            );
        }

        /// <summary>
        /// 현재 옵션 상태를 기반으로 SetupStep 파이프라인을 구성합니다.
        /// </summary>
        /// <remarks>
        /// 스텝 간 순서 의존성이 있으므로, 목록 순서를 변경할 경우 각 스텝의 전제 조건을 함께 검토해야 합니다.
        /// </remarks>
        protected override void BuildStepPipeline()
        {
            bool needSampleResources = setAllSampleData;

            // 기존 구성 초기화
            setupSteps.Clear();

            // -----------------------------------------------------------------
            // 1) 공통 필수 스텝
            // -----------------------------------------------------------------

            // 프로젝트 설정 ScriptableObject 생성/초기화
            setupSteps.Add(new StepCreateSettingScriptableObjectTcg());

            // 샘플 리소스를 복사하지 않는 경우, 빈 테이블 구조(Addressable Tables)를 준비
            if (!needSampleResources)
            {
                setupSteps.Add(new StepCopyEmptyDataTableTcg());
            }

            // 순서 중요: Localization은 UIWindows/프리팹 복사 및 테이블 구성 과정에서 참조될 수 있음
            setupSteps.Add(new StepCopyDefaultLocalizationTcg());

            // -----------------------------------------------------------------
            // 2) 옵션 스텝: 샘플 TCG 리소스/데이터 복사
            // -----------------------------------------------------------------
            if (needSampleResources)
            {
                setupSteps.Add(new StepCopyAllSampleDataTcg());
            }

            // -----------------------------------------------------------------
            // 3) 씬 필수 오브젝트 구성 및 저장
            // -----------------------------------------------------------------
            // 순서 중요: 데이터/리소스 준비 이후 씬에 필요한 오브젝트를 구성
            setupSteps.Add(new StepSetSceneRequireObjectTcg(needSampleResources));

            // 공통 설정값(예: 데미지 텍스트 UI 파라미터 등) 적용
            setupSteps.Add(new StepSetDefaultSettingScriptableObjectTcg());

            // -----------------------------------------------------------------
            // 4) Addressables 구성 (가급적 마지막)
            // -----------------------------------------------------------------
            // Addressables 기본 구성(설정/테이블 등)을 먼저 등록
            setupSteps.Add(new StepSetDefaultAddressableDataTcg());

            // 샘플 리소스를 사용하는 경우에만 추가 Addressables/샘플 전용 셋업 수행
            if (needSampleResources)
            {
                // TCG 카드 Addressables 구성 + 윈도우 테이블 재등록
                setupSteps.Add(new StepSetAddressableDataTcg());

                // 샘플 기준 카메라 설정(MainCamera를 Orthographic으로)
                setupSteps.Add(new StepSetCameraTcg());

                // window 테이블을 사용하여 테스트용 UIWindow 생성
                // NOTE: StepSetAddressableDataTcg 단계에서 window 테이블 내용을 업데이트하므로 이후에 실행해야 함
                setupSteps.Add(new StepInstantiateUIWindowsFromTableTcg());

                // 샘플 TCG용 각종 설정 ScriptableObject 값 적용(덱 프리셋, 컷신 파라미터, 입력 시스템 등)
                setupSteps.Add(new StepSetSettingScriptableObjectTcg());
            }
        }
    }
}
#endif
