#if UNITY_EDITOR
using System;
using GGemCo2DCoreEditor;
using GGemCo2DTcg;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;

namespace GGemCo2DTcgEditor
{
    public sealed class TcgLifetimeLocalizationGeneratorWindow : EditorWindow
    {
        private const string Title = "Lifetime Localization 만들기";
        [Header("출력")] private DefaultAsset _outputFolder;

        private const string TableLifetimeFormat = LocalizationConstantsTcg.Tables.LifetimeDescription;
        // private const string TableLifetimeType   = "TCG_Lifetime_Type";
        // private const string TableLifetimeTerm   = "TCG_Lifetime_Term";

        [MenuItem(ConfigEditorTcg.NameToolCreateLifetimeLocalization, false,
            (int)ConfigEditorTcg.ToolOrdering.CreateLifetimeLocalization)]
        public static void Open()
        {
            GetWindow<TcgLifetimeLocalizationGeneratorWindow>(Title);
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox(
                "Lifetime 관련 Localization StringTableCollection을 생성/갱신합니다.\n" +
                "- Lifetime Format (Smart String)",
                MessageType.Info);

            _outputFolder =
                (DefaultAsset)EditorGUILayout.ObjectField("출력 폴더(Assets/)", _outputFolder, typeof(DefaultAsset), false);

            if (GUILayout.Button("Generate / Update Tables"))
            {
                Generate();
            }
        }

        private void Generate()
        {
            if (_outputFolder == null) throw new InvalidOperationException("출력 폴더가 null 입니다.");
            var outputPath = AssetDatabase.GetAssetPath(_outputFolder);
            // GenerateLifetimeTypeTable(outputPath);
            GenerateLifetimeFormatTable(outputPath);
            // GenerateLifetimeTermTable(outputPath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                Title,
                $"완료되었습니다.",
                "확인");
        }

        /*
        private static void GenerateLifetimeTypeTable(string outputPath)
        {
            var collection = HelperLocalization.EnsureStringTableCollection(TableLifetimeType, outputPath);

            Add(collection, "lifetime_indefinite",       "지속",        "Indefinite");
            Add(collection, "lifetime_duration_turns",   "턴 지속",     "Duration");
            Add(collection, "lifetime_trigger_count",    "발동 횟수",   "Trigger Count");
            Add(collection, "lifetime_durability",       "내구도",     "Durability");
        }
        */

        private static void GenerateLifetimeFormatTable(string outputPath)
        {
            var collection = HelperLocalization.EnsureStringTableCollection(TableLifetimeFormat, outputPath);

            AddSmart(collection, "lifetime_indefinite_format",
                "전투 중 지속",
                "Lasts for the battle");

            AddSmart(collection, "lifetime_duration_turns_format",
                "{Turns}턴 동안 지속",
                "Lasts for {Turns} turns");

            AddSmart(collection, "lifetime_trigger_count_format",
                "{Count}회 발동 후 소멸",
                "Expires after {Count} triggers");

            AddSmart(collection, "lifetime_durability_format",
                "내구도 {Durability}",
                "Durability {Durability}");
        }

        /*
        private static void GenerateLifetimeTermTable(string outputPath)
        {
            var collection = HelperLocalization.EnsureStringTableCollection(TableLifetimeTerm, outputPath);

            Add(collection, "lifetime_tick_turn_end", "턴 종료 시", "At turn end");
            Add(collection, "lifetime_tick_on_trigger", "발동 시", "On trigger");
            Add(collection, "lifetime_tick_on_use", "사용 시", "On use");
        }
        */
        private static void Add(StringTableCollection collection, string key, string ko, string en)
        {
            foreach (var table in collection.StringTables)
            {
                var entry = table.GetEntry(key) ?? table.AddEntry(key, "");
                entry.Value = table.LocaleIdentifier.Code == "ko" ? ko : en;
            }
        }

        private static void AddSmart(StringTableCollection collection, string key, string ko, string en)
        {
            foreach (var table in collection.StringTables)
            {
                var value = table.LocaleIdentifier.Code == "ko" ? ko : en;
                
                var entry = table.GetEntry(key);
                if (entry == null)
                {
                    entry = table.AddEntry(key, value);
                }
                else
                {
                    entry.Value = value;
                }
                entry.IsSmart = true;
            }
        }
    }
}
#endif
