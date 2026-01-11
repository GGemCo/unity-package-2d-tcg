using GGemCo2DCore;
using GGemCo2DCoreEditor;
using UnityEditor;
using UnityEngine;

namespace GGemCo2DTcgEditor
{
    public class StepSetDefaultSettingScriptableObjectTcg : SetupStepBase
    {
        public override bool Validate(EditorSetupContext ctx, out string msg)
        {
            msg = null;
            return true;
        }
        public override void Execute(EditorSetupContext ctx)
        {
            // GGemCoSettings
            var settings = FindSettingsAsset<GGemCoSettings>();
            if (settings == null)
            {
                HelperLog.Warn($"[StepSetSettingScriptableObject] GGemCoTcgSettings asset not found.", ctx);
            }
            else
            {
                // TCG 에서는 데미지 표시를 UI에 하기 때문에, Render Mode 를 변경한다.
                settings.damageTextCanvasRenderMode = RenderMode.ScreenSpaceOverlay;
                settings.damageTextFontSize = 38f;
                settings.damageTextEasingType = Easing.EaseType.EaseOutCubic;
                settings.damageTextMoveUpTime = 0.5f;
                settings.damageTextMoveUpDistance = 20f;
                settings.damageTextFadeOutTime = 0.5f;
                settings.damageTextRandomXRange = 10f;

                EditorUtility.SetDirty(settings);
            }
        }
    }
}