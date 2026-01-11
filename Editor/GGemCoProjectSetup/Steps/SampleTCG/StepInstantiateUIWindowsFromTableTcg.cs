#if UNITY_EDITOR
using GGemCo2DCoreEditor;
using UnityEngine;

namespace GGemCo2DTcgEditor
{
    public sealed class StepInstantiateUIWindowsFromTableTcg : SetupStepBase
    {
        public override bool Validate(EditorSetupContext ctx, out string message)
        {
            message = null;
            return true;
        }

        public override void Execute(EditorSetupContext ctx)
        {
            var sceneEditorGame = ScriptableObject.CreateInstance<SceneEditorGameTcg>();
            sceneEditorGame.SetupAllTestWindow(ctx);
        }
    }
}
#endif
