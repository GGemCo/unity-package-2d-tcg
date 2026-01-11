using System;
using System.IO;
using GGemCo2DCore;
using GGemCo2DCoreEditor;
using GGemCo2DTcg;
using UnityEditor;
using UnityEngine;

namespace GGemCo2DTcgEditor
{
    public class SettingGGemCoTcg
    {
        private const string Title = "설정 ScriptableObject 추가하기";
        private const string SettingsFolder = "Assets/"+ConfigDefine.NameSDK+"/Settings/";

        public void OnGUI()
        {
            HelperEditorUI.OnGUITitle(Title);

            if (GUILayout.Button("설정 ScriptableObject 생성하기"))
            {
                CreateSettings();
            }
        }

        public void CreateSettings(EditorSetupContext ctx = null)
        {
            foreach (var kvp in ConfigScriptableObjectTcg.SettingsTypes)
            {
                CreateOrSelectSettings(kvp.Key, kvp.Value, ctx);
            }
        }

        private void CreateOrSelectSettings(string fileName, Type type, EditorSetupContext ctx = null)
        {
            if (!Directory.Exists(SettingsFolder))
                Directory.CreateDirectory(SettingsFolder);

            string path = $"{SettingsFolder}{fileName}.asset";
            UnityEngine.Object existing = AssetDatabase.LoadAssetAtPath(path, type);

            if (existing != null)
            {
                Selection.activeObject = existing;
                EditorUtility.FocusProjectWindow();
                HelperLog.Warn($"{fileName} 설정이 이미 존재합니다.", ctx);
            }
            else
            {
                ScriptableObject asset = ScriptableObject.CreateInstance(type);
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Selection.activeObject = asset;
                EditorUtility.FocusProjectWindow();
                
                HelperLog.Info($"{fileName} ScriptableObject 가 생성되었습니다.", ctx);
            }

            ctx?.Logger.Info($"[Add Setting Scriptable Object] ");
        }
    }
}
