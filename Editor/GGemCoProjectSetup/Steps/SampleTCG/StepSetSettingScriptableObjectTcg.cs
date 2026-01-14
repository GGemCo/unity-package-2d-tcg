using GGemCo2DCore;
using GGemCo2DCoreEditor;
using GGemCo2DTcg;
using UnityEditor;
using UnityEngine;

namespace GGemCo2DTcgEditor
{
    /// <summary>
    /// 샘플 TCG 구성을 위해 각종 Settings ScriptableObject를 찾아 기본값을 적용하는 에디터 설정 스텝입니다.
    /// </summary>
    /// <remarks>
    /// <para>
    /// 이 스텝은 프로젝트에 존재하는 설정 에셋을 검색하여 값들을 갱신하고,
    /// 변경 사항을 저장 대상으로 표시(Dirty)합니다.
    /// </para>
    /// <para>
    /// 설정 에셋이 존재하지 않는 경우에는 경고 로그만 남기고 다음 설정으로 진행합니다.
    /// </para>
    /// </remarks>
    public class StepSetSettingScriptableObjectTcg : SetupStepBase
    {
        /// <summary>
        /// 이 스텝은 별도 사전 조건이 없으므로 항상 유효합니다.
        /// </summary>
        /// <param name="ctx">에디터 설정 컨텍스트로, 로그 출력에 사용됩니다.</param>
        /// <param name="message">검증 메시지 출력용 파라미터로, 항상 <c>null</c>을 반환합니다.</param>
        /// <returns>항상 <c>true</c>를 반환합니다.</returns>
        public override bool Validate(EditorSetupContext ctx, out string message)
        {
            message = null;
            return true;
        }

        /// <summary>
        /// 샘플 TCG 실행을 위한 Settings ScriptableObject들을 순차적으로 구성합니다.
        /// </summary>
        /// <param name="ctx">에디터 설정 컨텍스트로, 로그 출력에 사용됩니다.</param>
        public override void Execute(EditorSetupContext ctx)
        {
            // -----------------------------------------------------------------
            // GGemCoTcgSettings: AI 적 덱 프리셋(EnemyDeckPreset) 지정
            // -----------------------------------------------------------------
            var tcgSettings = FindSettingsAsset<GGemCoTcgSettings>();
            if (tcgSettings == null)
            {
                HelperLog.Warn(
                    $"[{nameof(StepSetSettingScriptableObjectTcg)}] GGemCoTcgSettings asset not found.",
                    ctx);
            }
            else
            {
                // 샘플 AI 덱 프리셋 에셋 경로(예제 프로젝트 기준)
                const string enemyDeckPresetPath =
                    "Assets/GGemCo/Data/Tcg/AiDeckPreset/AiDeckPreset_Easy.asset";

                // 최신 상태로 로드되도록 강제 Import 후 에셋을 로드한다.
                AssetDatabase.ImportAsset(
                    enemyDeckPresetPath,
                    ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);

                var enemyPreset = AssetDatabase.LoadAssetAtPath<EnemyDeckPreset>(enemyDeckPresetPath);
                if (enemyPreset == null)
                {
                    HelperLog.Warn(
                        $"[{nameof(StepSetSettingScriptableObjectTcg)}] EnemyDeckPreset not found at path: {enemyDeckPresetPath}",
                        ctx);
                }
                else
                {
                    tcgSettings.enemyDeckPreset = enemyPreset;
                }

                EditorUtility.SetDirty(tcgSettings);
            }

            // -----------------------------------------------------------------
            // GGemCoTcgUICutsceneSettings: 카드 연출(컷신) 파라미터 기본값 설정
            // -----------------------------------------------------------------
            var cutsceneSettings = FindSettingsAsset<GGemCoTcgUICutsceneSettings>();
            if (cutsceneSettings == null)
            {
                HelperLog.Warn(
                    $"[{nameof(StepSetSettingScriptableObjectTcg)}] GGemCoTcgUICutsceneSettings asset not found.",
                    ctx);
            }
            else
            {
                // 타겟 이동 오프셋(좌하단 기준)
                cutsceneSettings.moveToTargetLeftDownOffset = new Vector2(0, -160f);

                // Hand → Grave 연출(페이드 아웃)
                cutsceneSettings.handToGraveFadeOutDelayTime = 0.5f;
                cutsceneSettings.handToGraveFadeOutEasing = Easing.EaseType.EaseOutSine;
                cutsceneSettings.handToGraveFadeOutDuration = 0.3f;

                // Hand → Field 연출(페이드 인)
                cutsceneSettings.handToFieldFadeInEasing = Easing.EaseType.EaseOutSine;
                cutsceneSettings.handToFieldFadeInDuration = 0.3f;

                // 공격 연출(백스텝/히트/데미지 표시 타이밍)
                cutsceneSettings.attackUnitBackDistance = -90f;
                cutsceneSettings.attackUnitBackEasing = Easing.EaseType.EaseOutSine;
                cutsceneSettings.attackUnitBackDuration = 0.3f;
                cutsceneSettings.attackUnitHitEasing = Easing.EaseType.EaseInQuintic;
                cutsceneSettings.attackUnitHitDuration = 0.2f;
                cutsceneSettings.attackUnitShowDamageDiffDuration = 0f;

                EditorUtility.SetDirty(cutsceneSettings);
            }

            // -----------------------------------------------------------------
            // GGemCoSettings: 입력 시스템 타입 및 Define Symbols 동기화
            // -----------------------------------------------------------------
            var ggemCoSettings = FindSettingsAsset<GGemCoSettings>();
            if (ggemCoSettings == null)
            {
                HelperLog.Warn(
                    $"[{nameof(StepSetSettingScriptableObjectTcg)}] GGemCoSettings asset not found.",
                    ctx);
            }
            else
            {
                ggemCoSettings.inputSystemType = InputSystemType.NewInputSystem;

                // 입력 시스템 타입에 맞춰 스크립팅 Define Symbols를 동기화한다.
                var settingGGemCoInspector = ScriptableObject.CreateInstance<SettingGGemCoInspector>();
                settingGGemCoInspector.SyncInputDefineSymbols(ggemCoSettings.inputSystemType);

                EditorUtility.SetDirty(ggemCoSettings);
            }

            // -----------------------------------------------------------------
            // GGemCoSaveSettings: 세이브 데이터 사용 여부
            // -----------------------------------------------------------------
            var saveSettings = FindSettingsAsset<GGemCoSaveSettings>();
            if (saveSettings == null)
            {
                HelperLog.Warn(
                    $"[{nameof(StepSetSettingScriptableObjectTcg)}] GGemCoSaveSettings asset not found.",
                    ctx);
            }
            else
            {
                saveSettings.useSaveData = true;
                EditorUtility.SetDirty(saveSettings);
            }

            // -----------------------------------------------------------------
            // GGemCoMapSettings: 맵 기능 사용 여부(샘플 TCG 기준 Off)
            // -----------------------------------------------------------------
            var mapSettings = FindSettingsAsset<GGemCoMapSettings>();
            if (mapSettings == null)
            {
                HelperLog.Warn(
                    $"[{nameof(StepSetSettingScriptableObjectTcg)}] GGemCoMapSettings asset not found.",
                    ctx);
            }
            else
            {
                mapSettings.useMap = false;
                EditorUtility.SetDirty(mapSettings);
            }

            HelperLog.Info(
                $"[{nameof(StepSetSettingScriptableObjectTcg)}] Settings ScriptableObjects have been configured for sample TCG.",
                ctx);
        }
    }
}
