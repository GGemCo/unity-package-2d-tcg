using System;
using GGemCo2DCore;
using GGemCo2DCoreEditor;
using GGemCo2DTcg;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace GGemCo2DTcgEditor
{
    /// <summary>
    /// 카드 Type별 프리팹 추가하기
    /// </summary>
    public class SettingUIElementCard : DefaultAddressable
    {
        private const string Title = "카드 Type별 프리팹 추가하기";
        private readonly AddressableEditorTcg _addressableEditor;

        public SettingUIElementCard(AddressableEditorTcg addressableEditorWindow)
        {
            _addressableEditor = addressableEditorWindow;
            targetGroupName = ConfigAddressableGroupNameTcg.CardGroup.UIElement;
        }
        public void OnGUI()
        {
            // Common.OnGUITitle(Title);

            if (GUILayout.Button(Title, GUILayout.Width(_addressableEditor.buttonWidth), GUILayout.Height(_addressableEditor.buttonHeight)))
            {
                Setup();
            }
        }
        
        /// <summary>
        /// Addressable 설정하기
        /// </summary>
        private void Setup()
        {
            bool result = EditorUtility.DisplayDialog(TextDisplayDialogTitle, TextDisplayDialogMessage, "네", "아니요");
            if (!result) return;
            
            // AddressableSettings 가져오기 (없으면 생성)
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (!settings)
            {
                Debug.LogWarning("Addressable 설정을 찾을 수 없습니다. 새로 생성합니다.");
                settings = CreateAddressableSettings();
            }

            // GGemCo_Tables 그룹 가져오기 또는 생성
            AddressableAssetGroup group = GetOrCreateGroup(settings, targetGroupName);

            if (!group)
            {
                Debug.LogError($"'{targetGroupName}' 그룹을 설정할 수 없습니다.");
                return;
            }
            // 그룹 엔트리 전체 초기화 (스키마/설정은 유지)
            ClearGroupEntries(settings, group);

            foreach (var type in EnumCache<CardConstants.Type>.Values)
            {
                // Debug.Log(type);
                if (type == CardConstants.Type.None) continue;
                
                string assetPath = $"{ConfigAddressablePathTcg.Card.UIElement}/{type}.prefab";
                // 대상 파일 가져오기
                var asset = AssetDatabase.LoadMainAssetAtPath(assetPath);
                if (!asset)
                {
                    Debug.LogError($"파일을 찾을 수 없습니다: {assetPath}");
                    continue;
                }

                // 기존 Addressable 항목 확인
                AddressableAssetEntry entry = settings.FindAssetEntry(AssetDatabase.AssetPathToGUID(assetPath));

                if (entry == null)
                {
                    // 신규 Addressable 항목 추가
                    entry = settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(assetPath), group);
                    Debug.Log($"Addressable 항목을 추가했습니다: {assetPath}");
                }
                else
                {
                    Debug.Log($"이미 Addressable에 등록된 항목입니다: {assetPath}");
                }

                // 키 값 설정
                entry.address = $"{ConfigAddressableKeyTcg.Card.UIElement}_{type}";
                // 라벨 값 설정
                entry.SetLabel(ConfigAddressableLabelTcg.Card.UIElement, true, true);

                // Debug.Log($"Addressable 키 값 설정: {keyName}");
            }

            // 설정 저장
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, null, true);
            AssetDatabase.SaveAssets();
            // 테이블 다시 로드하기
            // _addressableEditor.LoadTables();
            
            EditorUtility.DisplayDialog(Title, "Addressable 설정 완료", "OK");
        }

    }
}