using GGemCo2DCore;
using GGemCo2DCoreEditor;

namespace GGemCo2DTcgEditor
{
    public class DefaultSceneEditorTcg : DefaultSceneEditor
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            packageType = ConfigPackageInfo.PackageType.Tcg;
        }
    }
}