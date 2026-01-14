using System.Collections.Generic;
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
    /// 카드 일러스트 이미지 추가하기
    /// </summary>
    public class SettingTcgCard : DefaultAddressable
    {
        private const string Title = "카드 일러스트/테두리 이미지 추가하기";
        private readonly AddressableEditorTcg _addressableEditor;
        private string _targetGroupNameBorder;

        public SettingTcgCard(AddressableEditorTcg addressableEditorWindow)
        {
            _addressableEditor = addressableEditorWindow;
            targetGroupName = ConfigAddressableGroupNameTcg.CardGroup.ImageArtwork;
            _targetGroupNameBorder = ConfigAddressableGroupNameTcg.CardGroup.ImageBorder;
        }
        public void OnGUI()
        {
            if (TableLoaderManagerTcg.LoadTableTcgCard() == null)
            {
                EditorGUILayout.HelpBox($"{ConfigAddressableTableTcg.TableTcgCard} 테이블이 없습니다.", MessageType.Info);
            }
            else {
                if (GUILayout.Button(Title, GUILayout.Width(_addressableEditor.buttonWidth), GUILayout.Height(_addressableEditor.buttonHeight)))
                {
                    try
                    {
                        Setup();
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogException(e);
                        EditorUtility.DisplayDialog(Title, "카드 Addressable 설정 중 오류가 발생했습니다.\n자세한 내용은 콘솔 로그를 확인해주세요.", "OK");
                    }
                }
            }
        }
        
        /// <summary>
        /// Addressable 설정하기
        /// </summary>
        public void Setup(EditorSetupContext ctx = null)
        {
            bool result = EditorUtility.DisplayDialog(TextDisplayDialogTitle, TextDisplayDialogMessage, "네", "아니요");
            if (!result) return;
            
            // AddressableSettings 가져오기 (없으면 생성)
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (!settings)
            {
                HelperLog.Warn("Addressable 설정을 찾을 수 없습니다. 새로 생성합니다.", ctx);
                settings = CreateAddressableSettings();
            }

            // GGemCo_Tables 그룹 가져오기 또는 생성
            AddressableAssetGroup group = GetOrCreateGroup(settings, targetGroupName);
            if (group != null)
            {
                // 그룹 엔트리 전체 초기화 (스키마/설정은 유지)
                ClearGroupEntries(settings, group);

                // 일러스트 이미지 추가
                Dictionary<int, StruckTableTcgCard> tcgCards = TableLoaderManagerTcg.LoadTableTcgCard().GetDatas();
                foreach (KeyValuePair<int, StruckTableTcgCard> outerPair in tcgCards)
                {
                    var info = outerPair.Value;
                    if (info == null) continue;
                
                    string key = $"{ConfigAddressableKeyTcg.Card.ImageArt}_{info.uid}";
                    string assetPath = $"{ConfigAddressablePathTcg.Card.ImageArt}/{info.type}/{info.imageFileName}.png";
                    string label = $"{ConfigAddressableLabelTcg.Card.ImageArt}";
                
                    Add(settings, group, key, assetPath, label);
                }
            }
            else
            {
                HelperLog.Error($"'{targetGroupName}' 그룹을 설정할 수 없습니다.", ctx);
            }
            
            // 핸드 카드의 테두리 이미지 추가
            // 필드 카드의 테두리 이미지 
            AddressableAssetGroup groupBorder = GetOrCreateGroup(settings, _targetGroupNameBorder);
            if (groupBorder != null)
            {
                // 그룹 엔트리 전체 초기화 (스키마/설정은 유지)
                ClearGroupEntries(settings, groupBorder);
                foreach (var grade in EnumCache<CardConstants.Grade>.Values)
                {
                    if (grade == CardConstants.Grade.None) continue;
                    // 핸드 카드의 테두리 이미지 추가
                    string key = $"{ConfigAddressableKeyTcg.Card.ImageBorderHand}_{grade}";
                    string assetPath = $"{ConfigAddressablePathTcg.Card.ImageBorderHand}/{grade}.png";
                    string label = $"{ConfigAddressableLabelTcg.Card.ImageBorder}";
                
                    Add(settings, groupBorder, key, assetPath, label);
                    
                    // 필드 카드의 테두리 이미지 
                    key = $"{ConfigAddressableKeyTcg.Card.ImageBorderField}_{grade}";
                    assetPath = $"{ConfigAddressablePathTcg.Card.ImageBorderField}/{grade}.png";
                    label = $"{ConfigAddressableLabelTcg.Card.ImageBorder}";
                
                    Add(settings, groupBorder, key, assetPath, label);
                }
            }
            else
            {
                HelperLog.Error($"'{_targetGroupNameBorder}' 그룹을 설정할 수 없습니다.", ctx);
            }
            
            // 설정 저장
            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, null, true);
            
            if (ctx != null)
            {
                HelperLog.Info("[Addressable] 카드 설정 완료", ctx);
            }
            else
            {
                AssetDatabase.SaveAssets();
                EditorUtility.DisplayDialog(Title, "[Addressable] 카드 설정 완료", "OK");
            }
        }

    }
}