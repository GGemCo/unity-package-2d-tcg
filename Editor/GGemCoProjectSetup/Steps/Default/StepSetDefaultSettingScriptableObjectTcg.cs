using GGemCo2DCore;
using GGemCo2DCoreEditor;
using UnityEditor;
using UnityEngine;

namespace GGemCo2DTcgEditor
{
    /// <summary>
    /// 프로젝트의 <see cref="GGemCoSettings"/> 에셋을 찾아,
    /// TCG 기본 설정(데미지 텍스트 UI 표시 관련)을 적용하는 에디터 설정 스텝입니다.
    /// </summary>
    /// <remarks>
    /// TCG에서는 데미지 표시를 UI로 처리하므로,
    /// Render Mode 및 애니메이션/표시 파라미터를 TCG 권장값으로 덮어씁니다.
    /// </remarks>
    public class StepSetDefaultSettingScriptableObjectTcg : SetupStepBase
    {
        /// <summary>
        /// 설정 적용 스텝은 별도 사전 조건이 없으므로 항상 유효합니다.
        /// </summary>
        /// <param name="ctx">에디터 설정 컨텍스트로, 로그 출력에 사용됩니다.</param>
        /// <param name="msg">검증 메시지 출력용 파라미터로, 항상 <c>null</c>을 반환합니다.</param>
        /// <returns>항상 <c>true</c>를 반환합니다.</returns>
        public override bool Validate(EditorSetupContext ctx, out string msg)
        {
            msg = null;
            return true;
        }

        /// <summary>
        /// <see cref="GGemCoSettings"/> 에셋을 찾아 TCG 기본값을 설정하고,
        /// 변경 사항을 에디터에 반영(Dirty 표시)합니다.
        /// </summary>
        /// <param name="ctx">에디터 설정 컨텍스트로, 로그 출력에 사용됩니다.</param>
        public override void Execute(EditorSetupContext ctx)
        {
            // GGemCoSettings 에셋을 검색하여 프로젝트 전역 설정을 갱신한다.
            var settings = FindSettingsAsset<GGemCoSettings>();
            if (settings == null)
            {
                // 설정 에셋이 없으면 적용할 수 없으므로 경고만 출력한다.
                HelperLog.Warn($"[{nameof(StepSetDefaultSettingScriptableObjectTcg)}] GGemCoTcgSettings asset not found.", ctx);
            }
            else
            {
                // TCG에서는 데미지 표시를 UI로 처리하므로, Screen Space Overlay 기준으로 렌더링한다.
                settings.damageTextCanvasRenderMode = RenderMode.ScreenSpaceOverlay;

                // 데미지 텍스트의 기본 표시/애니메이션 파라미터(TCG 권장값)
                settings.damageTextFontSize = 38f;
                settings.damageTextEasingType = Easing.EaseType.EaseOutCubic;
                settings.damageTextMoveUpTime = 0.5f;
                settings.damageTextMoveUpDistance = 20f;
                settings.damageTextFadeOutTime = 0.5f;
                settings.damageTextRandomXRange = 10f;

                // 에디터에서 변경 사항 저장 대상으로 표시
                EditorUtility.SetDirty(settings);
            }
        }
    }
}
