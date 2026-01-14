using GGemCo2DTcgEditor;
using UnityEditor;

namespace GGemCo2DCoreEditor
{
    public class DefaultSettingToolTcg : DefaultEditorWindow
    {
        private readonly SettingGGemCoTcg _settingGGemCoTcg = new SettingGGemCoTcg();

        [MenuItem(ConfigEditorTcg.NameToolSettingDefault, false, (int)ConfigEditorTcg.ToolOrdering.DefaultSetting)]
        public static void ShowWindow()
        {
            GetWindow<DefaultSettingToolTcg>("기본 셋팅하기");
        }

        private void OnGUI()
        {
            _settingGGemCoTcg.OnGUI();
            
            EditorGUILayout.Space(20);
        }
    }
}