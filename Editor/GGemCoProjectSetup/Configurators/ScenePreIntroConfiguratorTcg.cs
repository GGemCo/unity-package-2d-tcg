using GGemCo2DTcgEditor;
using UnityEngine;

namespace GGemCo2DCoreEditor
{
    /// <summary>
    /// PreIntro 씬에 필요한 최소한의 에디터 오브젝트들을 자동으로 구성하는 설정자입니다.
    /// Canvas와 로딩 퍼센트 표시용 텍스트 구성을 담당합니다.
    /// </summary>
    /// <remarks>
    /// 주로 게임 시작 전 사전 로딩 단계에서 사용되며, Unity Editor 환경에서만 의미를 갖습니다.
    /// </remarks>
    public class ScenePreIntroConfiguratorTcg : DefaultSceneEditor, ISceneConfigurator
    {
        /// <summary>
        /// 에디터 상에서 PreIntro 씬에 필요한 필수 오브젝트들을 생성하고 초기 설정을 적용합니다.
        /// </summary>
        /// <param name="ctx">
        /// 씬 구성에 필요한 에디터 컨텍스트로, 현재 씬 상태와 공통 설정 정보를 포함합니다.
        /// </param>
        /// <param name="needSampleResources">
        /// 샘플 리소스 생성이 필요한지 여부를 나타내며, 현재 구현에서는 사용되지 않습니다.
        /// </param>
        public void ConfigureInEditor(EditorSetupContext ctx, bool needSampleResources = false)
        {
            // PreIntro 씬 구성을 담당하는 에디터용 ScriptableObject를 생성
            var sceneEditorPreIntro = ScriptableObject.CreateInstance<SceneEditorPreIntroTcg>();

            // Canvas 및 로딩 퍼센트 텍스트 등 필수 오브젝트를 씬에 배치 및 초기화
            sceneEditorPreIntro.SetupRequiredObjects(ctx);
        }
    }
}