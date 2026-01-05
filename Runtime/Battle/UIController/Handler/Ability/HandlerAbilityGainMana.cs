using System.Collections;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// Ability 기반 마나 획득 연출 핸들러.
    /// - 마나 토글 컨테이너 위치에 +N 팝업
    /// - 컨테이너 펄스
    /// </summary>
    public sealed class HandlerAbilityGainMana : HandlerBase, ITcgPresentationHandler
    {
        public TcgPresentationConstants.TcgPresentationStepType Type =>
            TcgPresentationConstants.TcgPresentationStepType.AbilityGainMana;

        public IEnumerator Play(TcgPresentationContext ctx, TcgPresentationStep step)
        {
            if (ctx == null) yield break;
            if (step.Payload is not TcgAbilityPayloadGainMana payload) yield break;
            if (payload.ManaValue <= 0) yield break;

            var window = ctx.GetUIWindow(step.ToZone);
            if (window == null) yield break;

            // 1) 우선: Player Hand의 마나 컨테이너
            if (window is UIWindowTcgHandPlayer playerHand && playerHand.containerToggleMana != null)
            {
                var pos = playerHand.containerToggleMana.position;
                ShowValueTextAt(pos, payload.ManaValue);

                // 마나 “획득” 느낌: 버프 계열 이펙트를 컨테이너 기준으로 보여주기 어려우므로,
                // 우선 펄스만 적용(이펙트는 추후 전용 EffectUid 추가 시 확장 권장)
                yield return CoPulseTransform(playerHand.containerToggleMana, duration: 0.18f, scaleUp: 1.08f);
                yield break;
            }

            // 2) 폴백: 대상 아이콘(또는 영웅 슬롯)
            var icon = window.GetIconByIndex(step.ToIndex);
            if (icon == null)
                icon = window.GetIconByIndex(ConfigCommonTcg.IndexHeroSlot);

            if (icon != null)
            {
                // 마나 획득은 “긍정 효과”이므로 heal 이펙트 재사용
                ShowEffect(icon, EffectUidHeal);
                ShowDamageText(icon, payload.ManaValue);
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
