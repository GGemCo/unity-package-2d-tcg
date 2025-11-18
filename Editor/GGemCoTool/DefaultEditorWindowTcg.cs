using GGemCo2DCore;
using GGemCo2DCoreEditor;

namespace GGemCo2DTcgEditor
{
    public class DefaultEditorWindowTcg : DefaultEditorWindow
    {
        protected TableLoaderManagerTcg tableLoaderManagerTcg;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            packageType = ConfigPackageInfo.PackageType.Tcg;
            
            tableLoaderManagerTcg = new TableLoaderManagerTcg();
        }
    }
}