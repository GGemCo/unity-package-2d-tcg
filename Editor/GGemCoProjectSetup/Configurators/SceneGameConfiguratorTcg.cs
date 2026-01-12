#if UNITY_EDITOR
using GGemCo2DTcgEditor;
using UnityEngine;

namespace GGemCo2DCoreEditor
{
    /// <summary>Game 씬 필수 구성: SceneGame + Camera/Canvas/World/Black + Popup/Sound/WindowManager</summary>
    public sealed class SceneGameConfiguratorTcg : DefaultSceneEditor, ISceneConfigurator
    {
        public void ConfigureInEditor(EditorSetupContext ctx, bool needSampleResources = false)
        {
            var sceneEditorGame = ScriptableObject.CreateInstance<SceneEditorGameTcg>();
            sceneEditorGame.SetupRequiredObjects(ctx);

            if (needSampleResources)
            {
                sceneEditorGame.SetupAllTestWindow(ctx);
            }
        }
    }
}
#endif