using GGemCo2DCore;
using GGemCo2DCoreEditor;
using GGemCo2DTcg;
using UnityEditor;
using UnityEngine;

namespace GGemCo2DTcgEditor
{
    /// <summary>
    /// 예제 RPG 설정으로 settings 스크립터블 오브젝트 설정
    /// </summary>
    public class StepSetSettingScriptableObjectTcg : SetupStepBase
    {
        public override bool Validate(EditorSetupContext ctx, out string message)
        {
            message = null;
            return true;
        }

        public override void Execute(EditorSetupContext ctx)
        {
            // GGemCoTcgSettings
            var tcgSettings = FindSettingsAsset<GGemCoTcgSettings>();
            if (tcgSettings == null)
            {
                HelperLog.Warn($"[StepSetSettingScriptableObject] GGemCoTcgSettings asset not found.", ctx);
            }
            else
            {
                const string enemyDeckPresetPath =
                    "Assets/GGemCo/Data/Tcg/AiDeckPreset/AiDeckPreset_Easy.asset";
                
                var enemyPreset =
                    AssetDatabase.LoadAssetAtPath<EnemyDeckPreset>(enemyDeckPresetPath);

                if (enemyPreset == null)
                {
                    HelperLog.Warn(
                        $"[StepSetSettingScriptableObject] EnemyDeckPreset not found at path: {enemyDeckPresetPath}",
                        ctx);
                }
                else
                {
                    tcgSettings.enemyDeckPreset = enemyPreset;
                    EditorUtility.SetDirty(tcgSettings);
                }

                EditorUtility.SetDirty(tcgSettings);
            }
            
            // GGemCoTcgUICutsceneSettings
            var cutsceneSettings = FindSettingsAsset<GGemCoTcgUICutsceneSettings>();
            if (cutsceneSettings == null)
            {
                HelperLog.Warn($"[StepSetSettingScriptableObject] GGemCoTcgUICutsceneSettings asset not found.", ctx);
            }
            else
            {
                cutsceneSettings.moveToTargetLeftDownOffset = new Vector2(0, -160f);

                cutsceneSettings.handToGraveFadeOutDelayTime = 0.5f;

                cutsceneSettings.handToGraveFadeOutEasing = Easing.EaseType.EaseOutSine;
                cutsceneSettings.handToGraveFadeOutDuration = 0.3f;
                
                cutsceneSettings.handToFieldFadeInEasing = Easing.EaseType.EaseOutSine;
                cutsceneSettings.handToFieldFadeInDuration = 0.3f;

                cutsceneSettings.attackUnitBackDistance = -90f;
                cutsceneSettings.attackUnitBackEasing = Easing.EaseType.EaseOutSine;
                cutsceneSettings.attackUnitBackDuration = 0.3f;
                cutsceneSettings.attackUnitHitEasing = Easing.EaseType.EaseInQuintic;
                cutsceneSettings.attackUnitHitDuration = 0.2f;
                cutsceneSettings.attackUnitShowDamageDiffDuration = 0f;
                
                EditorUtility.SetDirty(cutsceneSettings);
            }

            // GGemCoSettings
            var ggemCoSettings = FindSettingsAsset<GGemCoSettings>();
            if (ggemCoSettings == null)
            {
                HelperLog.Warn($"[StepSetSettingScriptableObject] GGemCoSettings asset not found.", ctx);
            }
            else
            {
                ggemCoSettings.inputSystemType = InputSystemType.NewInputSystem;

                var settingGGemCoInspector = ScriptableObject.CreateInstance<SettingGGemCoInspector>();
                settingGGemCoInspector.SyncInputDefineSymbols(ggemCoSettings.inputSystemType);

                EditorUtility.SetDirty(ggemCoSettings);
            }
            
            // GGemCoSaveSettings
            var saveSettings = FindSettingsAsset<GGemCoSaveSettings>();
            if (saveSettings == null)
            {
                HelperLog.Warn($"[StepSetSettingScriptableObject] GGemCoSaveSettings asset not found.", ctx);
            }
            else
            {
                saveSettings.useSaveData = true;

                EditorUtility.SetDirty(saveSettings);
            }
            
            // GGemCoMapSettings
            var mapSettings = FindSettingsAsset<GGemCoMapSettings>();
            if (mapSettings == null)
            {
                HelperLog.Warn($"[StepSetSettingScriptableObject] GGemCoMapSettings asset not found.", ctx);
            }
            else
            {
                mapSettings.useMap = false;

                EditorUtility.SetDirty(mapSettings);
            }
            // 변경된 에셋 저장
            AssetDatabase.SaveAssets();

            HelperLog.Warn($"[StepSetSettingScriptableObject] Settings ScriptableObjects have been configured for sample TCG.", ctx);
        }
    }
}