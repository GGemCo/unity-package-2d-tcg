using GGemCo2DTcgEditor;
using UnityEngine;

namespace GGemCo2DCoreEditor
{
    /// <summary>Loading 씬 필수 구성: SceneLoading + Canvas + 퍼센트 텍스트</summary>
    public sealed class SceneLoadingConfiguratorTcg : DefaultSceneEditor, ISceneConfigurator
    {
        public void ConfigureInEditor(EditorSetupContext ctx, bool needSampleResources = false)
        {
            var sceneEditorLoading = ScriptableObject.CreateInstance<SceneEditorLoadingTcg>();
            sceneEditorLoading.SetupRequiredObjects(ctx);
        }
    }
}
