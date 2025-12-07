#if UNITY_EDITOR
using System.IO;
using GGemCo2DCore;
using GGemCo2DTcg;
using UnityEditor;
using UnityEngine;

namespace GGemCo2DTcgEditor
{
    /// <summary>
    /// AI 덱 프리셋 샘플을 생성하는 Editor 유틸리티.
    /// - Easy / Normal / Hard 기본 프리셋 3개를 만듭니다.
    /// - 생성 후, Inspector에서 카드 Uid와 규칙만 실제 프로젝트에 맞게 수정하면 됩니다.
    /// </summary>
    public static class AiDeckPresetSampleCreator
    {
        // 프리셋이 저장될 기본 경로
        private const string DefaultFolderPath = ConfigDefine.PathGGemCo+"/"+ConfigPackageInfo.NamePackageTcg+"/AIPresets";

        [MenuItem(ConfigEditorTcg.NameToolCreateSampleAiDeck, false, (int)ConfigEditorTcg.ToolOrdering.CreateSampleAiDeck)]
        public static void CreateSamplePresets()
        {
            // 폴더가 없으면 생성
            EnsureFolder(DefaultFolderPath);

            CreateEasyPreset();
            CreateNormalPreset();
            CreateHardPreset();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[AiDeckPresetSampleCreator] Sample AI Deck Presets created.");
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
                return;

            // 상위 폴더부터 차례대로 생성
            string[] parts = folderPath.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }

        private static void CreateEasyPreset()
        {
            var preset = ScriptableObject.CreateInstance<EnemyDeckPreset>();

            preset.presetId = "AI_Easy_Default";
            preset.displayName = "AI Easy Deck";
            preset.difficulty = AiDeckDifficulty.Easy;
            preset.deckSize = 25;
            preset.randomSeedMode = EnemyDeckPreset.RandomSeedMode.UseFixedSeed;
            preset.fixedSeed = 1001;

            // 고정 카드: 저코스트 Creature 위주 (예시 Uid)
            preset.fixedCardRules.Add(new AiDeckFixedCardRule
            {
                cardUid = 1001, // 예: 1코스트 크리처
                minCopies = 2,
                maxCopies = 3
            });
            preset.fixedCardRules.Add(new AiDeckFixedCardRule
            {
                cardUid = 1002, // 예: 1코스트 크리처
                minCopies = 1,
                maxCopies = 2
            });

            // 필터 규칙: Common, Cost 1~3 Creature로 덱 대부분 채우기
            preset.filterRules.Add(new AiDeckFilterRule
            {
                type = CardConstants.Type.Creature,
                minGrade = CardConstants.Grade.Common,
                maxGrade = CardConstants.Grade.Common,
                minCost = 1,
                maxCost = 3,
                count = 15,
                allowDuplicateSameCard = true
            });

            // 필터 규칙: 저코스트 Common Spell 약간 섞기
            preset.filterRules.Add(new AiDeckFilterRule
            {
                type = CardConstants.Type.Spell,
                minGrade = CardConstants.Grade.Common,
                maxGrade = CardConstants.Grade.Common,
                minCost = 1,
                maxCost = 2,
                count = 5,
                allowDuplicateSameCard = true
            });

            SavePresetAsset(preset, "AiDeckPreset_Easy.asset");
        }

        private static void CreateNormalPreset()
        {
            var preset = ScriptableObject.CreateInstance<EnemyDeckPreset>();

            preset.presetId = "AI_Normal_Default";
            preset.displayName = "AI Normal Deck";
            preset.difficulty = AiDeckDifficulty.Normal;
            preset.deckSize = 25;
            preset.randomSeedMode = EnemyDeckPreset.RandomSeedMode.None; // 게임마다 조금씩 달라지도록

            // 고정 카드: 핵심 시그니처 카드 고정
            preset.fixedCardRules.Add(new AiDeckFixedCardRule
            {
                cardUid = 1005, // 예: 4코스트 레전더리 크리처 (Shadowfang Assassin)
                minCopies = 1,
                maxCopies = 1
            });

            // 필터 규칙: Common~Magic Creature, 코스트 1~4
            preset.filterRules.Add(new AiDeckFilterRule
            {
                type = CardConstants.Type.Creature,
                minGrade = CardConstants.Grade.Common,
                maxGrade = CardConstants.Grade.Magic,
                minCost = 1,
                maxCost = 4,
                count = 14,
                allowDuplicateSameCard = true
            });

            // 필터 규칙: Common~Magic Spell, 코스트 2~4
            preset.filterRules.Add(new AiDeckFilterRule
            {
                type = CardConstants.Type.Spell,
                minGrade = CardConstants.Grade.Common,
                maxGrade = CardConstants.Grade.Magic,
                minCost = 2,
                maxCost = 4,
                count = 6,
                allowDuplicateSameCard = true
            });

            // 필터 규칙: Permanent/Event를 소량 포함 (필드 컨트롤용)
            preset.filterRules.Add(new AiDeckFilterRule
            {
                type = CardConstants.Type.Permanent,
                minGrade = CardConstants.Grade.Common,
                maxGrade = CardConstants.Grade.Magic,
                minCost = 3,
                maxCost = 5,
                count = 3,
                allowDuplicateSameCard = false
            });

            SavePresetAsset(preset, "AiDeckPreset_Normal.asset");
        }

        private static void CreateHardPreset()
        {
            var preset = ScriptableObject.CreateInstance<EnemyDeckPreset>();

            preset.presetId = "AI_Hard_Default";
            preset.displayName = "AI Hard Deck";
            preset.difficulty = AiDeckDifficulty.Hard;
            preset.deckSize = 25;
            preset.randomSeedMode = EnemyDeckPreset.RandomSeedMode.None;

            // 고정 카드: 강한 콤보 카드들을 미리 묶어서 고정
            preset.fixedCardRules.Add(new AiDeckFixedCardRule
            {
                cardUid = 1005, // 강력한 레전더리 크리처
                minCopies = 1,
                maxCopies = 1
            });
            preset.fixedCardRules.Add(new AiDeckFixedCardRule
            {
                cardUid = 2003, // 예: 강력한 스펠 카드 Frost Nova Surge
                minCopies = 1,
                maxCopies = 2
            });

            // 필터 규칙: Common~Legendary Creature, 코스트 1~5 (곡선 전체)
            preset.filterRules.Add(new AiDeckFilterRule
            {
                type = CardConstants.Type.Creature,
                minGrade = CardConstants.Grade.Common,
                maxGrade = CardConstants.Grade.Legendary,
                minCost = 1,
                maxCost = 5,
                count = 12,
                allowDuplicateSameCard = true
            });

            // 필터 규칙: 제어용/피니시용 Spell (Magic 이상 비율 증가)
            preset.filterRules.Add(new AiDeckFilterRule
            {
                type = CardConstants.Type.Spell,
                minGrade = CardConstants.Grade.Magic,
                maxGrade = CardConstants.Grade.Legendary,
                minCost = 2,
                maxCost = 6,
                count = 8,
                allowDuplicateSameCard = true
            });

            // 필터 규칙: Permanent / Event로 시너지 보강
            preset.filterRules.Add(new AiDeckFilterRule
            {
                type = CardConstants.Type.Permanent,
                minGrade = CardConstants.Grade.Magic,
                maxGrade = CardConstants.Grade.Legendary,
                minCost = 3,
                maxCost = 6,
                count = 3,
                allowDuplicateSameCard = false
            });

            SavePresetAsset(preset, "AiDeckPreset_Hard.asset");
        }

        private static void SavePresetAsset(EnemyDeckPreset preset, string fileName)
        {
            string path = Path.Combine(DefaultFolderPath, fileName);
            AssetDatabase.CreateAsset(preset, path);
            EditorUtility.SetDirty(preset);
        }
    }
}
#endif
