#if UNITY_EDITOR
using GGemCo2DTcgEditor;
using UnityEngine;

namespace GGemCo2DCoreEditor
{
    /// <summary>
    /// Game 씬에 필수적인 에디터 전용 오브젝트들을 자동으로 구성하는 설정자입니다.
    /// SceneGame, Camera/Canvas/World/Black, Popup/Sound, WindowManager 구성을 담당합니다.
    /// </summary>
    /// <remarks>
    /// Unity Editor 환경에서만 사용되며, 런타임 빌드에는 포함되지 않습니다.
    /// </remarks>
    public sealed class SceneGameConfiguratorTcg : DefaultSceneEditor, ISceneConfigurator
    {
        /// <summary>
        /// 에디터 상에서 Game 씬에 필요한 필수 오브젝트들을 생성하고 초기 설정을 적용합니다.
        /// </summary>
        /// <param name="ctx">
        /// 씬 구성에 필요한 에디터 컨텍스트로, 현재 씬 상태와 공통 설정 정보를 포함합니다.
        /// </param>
        /// <param name="needSampleResources">
        /// 샘플 리소스 생성이 필요한지 여부를 나타내며, 현재 구현에서는 사용되지 않습니다.
        /// </param>
        public void ConfigureInEditor(EditorSetupContext ctx, bool needSampleResources = false)
        {
            // Game 씬 구성을 담당하는 에디터용 ScriptableObject를 생성
            var sceneEditorGame = ScriptableObject.CreateInstance<SceneEditorGameTcg>();

            // 필수 오브젝트(Camera, Canvas, World 등)를 씬에 배치 및 초기화
            sceneEditorGame.SetupRequiredObjects(ctx);
        }
    }
}
#endif