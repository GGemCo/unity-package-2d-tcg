using System.Collections;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// Ability 기반 추가 행동(Extra Action) 연출 핸들러.
    /// - 턴 종료 버튼을 강조(펄스) + +N 팝업
    /// </summary>
    public sealed class HandlerAbilityExtraAction : HandlerBase, ITcgPresentationHandler
    {
        public TcgPresentationConstants.TcgPresentationStepType Type =>
            TcgPresentationConstants.TcgPresentationStepType.AbilityExtraAction;

        public IEnumerator Play(TcgPresentationContext ctx, TcgPresentationStep step)
        {
            if (ctx == null) yield break;
            if (step.Payload is not TcgAbilityPayloadExtraAction payload) yield break;
            if (payload.ExtraActionCount <= 0) yield break;

            // 1) 우선: 턴 종료 버튼 강조
            var hud = ctx.BattleHud;
            if (hud != null && hud.buttonTurnOff != null)
            {
                var btn = hud.buttonTurnOff;
                ShowValueTextAt(btn.transform.position, payload.ExtraActionCount);
                yield return CoPulseTransform(btn.transform, duration: 0.2f, scaleUp: 1.12f);
                yield break;
            }

            // 2) 폴백: 대상 아이콘(또는 영웅 슬롯)
            var window = ctx.GetUIWindow(step.ToZone);
            if (window == null) yield break;

            var icon = window.GetIconByIndex(step.ToIndex);
            if (icon == null)
                icon = window.GetIconByIndex(ConfigCommonTcg.IndexHeroSlot);

            if (icon != null)
            {
                ShowEffect(icon, EffectUidHeal);
                ShowDamageText(icon, payload.ExtraActionCount);
                yield return CoPulseTransform(icon.transform, duration: 0.16f, scaleUp: 1.06f);
            }
        }

        private static IEnumerator CoPulseTransform(Transform t, float duration, float scaleUp)
        {
            if (t == null) yield break;
            if (duration <= 0f) yield break;

            var half = duration * 0.5f;
            var original = t.localScale;
            var peak = original * scaleUp;

            for (float time = 0f; time < half; time += Time.unscaledDeltaTime)
            {
                var k = Mathf.Clamp01(time / half);
                t.localScale = Vector3.LerpUnclamped(original, peak, EaseOutQuad(k));
                yield return null;
            }
            for (float time = 0f; time < half; time += Time.unscaledDeltaTime)
            {
                var k = Mathf.Clamp01(time / half);
                t.localScale = Vector3.LerpUnclamped(peak, original, EaseInQuad(k));
                yield return null;
            }

            t.localScale = original;
        }

        private static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
        private static float EaseInQuad(float t) => t * t;
    }
}
