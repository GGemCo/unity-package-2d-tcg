using UnityEditor;
using UnityEngine;
using GGemCo2DTcg;

namespace GGemCo2DTcgEditor
{
    public class AddressableEditorTcg : DefaultEditorWindowTcg
    {
        private const string Title = "Addressable 셋팅하기";
        public float buttonWidth;
        public float buttonHeight;
        
        private SettingScriptableObjectTcg _settingScriptableObjectTcg;
        private SettingTableTcg _settingTableTcg;
        private SettingUIElementCard _settingUIElementCard;
        private SettingTcgCard _settingTcgCard;
        
        public TableTcgCard tableTcgCard;
        
        private Vector2 _scrollPosition;

        [MenuItem(ConfigEditorTcg.NameToolSettingAddressable, false, (int)ConfigEditorTcg.ToolOrdering.SettingAddressable)]
        public static void ShowWindow()
        {
            GetWindow<AddressableEditorTcg>(Title);
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            LoadTables();
            
            buttonHeight = 40f;
            _settingScriptableObjectTcg = new SettingScriptableObjectTcg(this);
            _settingTableTcg = new SettingTableTcg(this);
            _settingUIElementCard = new SettingUIElementCard(this);
            _settingTcgCard = new SettingTcgCard(this);
        }

        private void LoadTables()
        {
            tableTcgCard = tableLoaderManagerTcg.LoadTableTcgCard();
        }

        private void OnGUI()
        {
            buttonWidth = position.width / 2f - 10f;
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            EditorGUILayout.BeginHorizontal();
            _settingScriptableObjectTcg.OnGUI();
            _settingTableTcg.OnGUI();
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            _settingUIElementCard.OnGUI();
            _settingTcgCard.OnGUI();
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(20);
            EditorGUILayout.EndScrollView();
        }
    }
}