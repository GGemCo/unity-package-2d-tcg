using System.Collections;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// Ability 기반 드로우(카드 획득) 연출 핸들러.
    /// - 대상(대부분 영웅 슬롯)에 이펙트 + 팝업
    /// - 손패 윈도우 펄스(카드 유입 느낌)
    /// </summary>
    public sealed class HandlerAbilityDraw : HandlerBase, ITcgPresentationHandler
    {
        public TcgPresentationConstants.TcgPresentationStepType Type =>
            TcgPresentationConstants.TcgPresentationStepType.AbilityDraw;

        public IEnumerator Play(TcgPresentationContext ctx, TcgPresentationStep step)
        {
            if (ctx == null) yield break;
            if (step.Payload is not TcgAbilityPayloadDraw payload) yield break;
            if (payload.DrawCount <= 0) yield break;

            var window = ctx.GetUIWindow(step.ToZone);
            if (window == null) yield break;

            // 타겟 아이콘(없으면 영웅 슬롯으로 폴백)
            var icon = window.GetIconByIndex(step.ToIndex);
            if (icon == null)
                icon = window.GetIconByIndex(ConfigCommonTcg.IndexHeroSlot);

            if (icon != null)
            {
                ShowEffect(icon, EffectUidHeal);
                ShowDamageText(icon, payload.DrawCount);
            }

            // 손패 윈도우 펄스(짧고 확실하게)
            yield return CoPulseTransform(window.transform, duration: 0.18f, scaleUp: 1.06f);
        }

        private static IEnumerator CoPulseTransform(Transform t, float duration, float scaleUp)
        {
            if (t == null) yield break;
            if (duration <= 0f) yield break;

            var half = duration * 0.5f;
            var original = t.localScale;
            var peak = original * scaleUp;

            // up
            for (float time = 0f; time < half; time += Time.unscaledDeltaTime)
            {
                var k = Mathf.Clamp01(time / half);
                t.localScale = Vector3.LerpUnclamped(original, peak, EaseOutQuad(k));
                yield return null;
            }
            // down
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
