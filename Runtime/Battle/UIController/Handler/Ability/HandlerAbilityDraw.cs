using System.Collections;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// Ability 기반 드로우(카드 획득) 연출을 처리하는 프리젠테이션 핸들러.
    /// </summary>
    /// <remarks>
    /// - 대상(대개 영웅 슬롯)에 이펙트 및 카운트 팝업을 표시한다.
    /// - 손패(윈도우)에 짧은 펄스 스케일 애니메이션을 적용해 카드 유입 느낌을 준다.
    /// </remarks>
    public sealed class HandlerAbilityDraw : HandlerBase, ITcgPresentationHandler
    {
        /// <summary>
        /// 이 핸들러가 처리하는 프리젠테이션 스텝 타입.
        /// </summary>
        public TcgPresentationConstants.TcgPresentationStepType Type =>
            TcgPresentationConstants.TcgPresentationStepType.AbilityDraw;

        /// <summary>
        /// 드로우(카드 획득) 연출을 재생한다.
        /// </summary>
        /// <param name="ctx">UI 윈도우 조회 및 연출 실행에 필요한 프리젠테이션 컨텍스트.</param>
        /// <param name="step">대상 존/인덱스와 드로우 페이로드를 포함하는 스텝.</param>
        /// <returns>코루틴 이터레이터.</returns>
        /// <remarks>
        /// 페이로드가 드로우 타입이 아니거나, 드로우 수가 0 이하이면 연출을 수행하지 않는다.
        /// 대상 아이콘이 없으면 영웅 슬롯(<see cref="ConfigCommonTcg.IndexHeroSlot"/>)으로 폴백한다.
        /// </remarks>
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
                // NOTE: 현재 연출 리소스 재사용을 위해 Heal 이펙트를 사용한다(드로우 전용 이펙트로 교체 가능).
                ShowEffect(icon, EffectUidHeal);

                // 드로우 수치를 팝업으로 표시한다(양수 표기).
                ShowDamageText(icon, payload.DrawCount);
            }

            // 손패 윈도우 펄스(짧고 확실하게)
            yield return CoPulseTransform(window.transform, duration: 0.18f, scaleUp: 1.06f);
        }

        /// <summary>
        /// 트랜스폼에 짧은 펄스(스케일 업/다운) 애니메이션을 적용한다.
        /// </summary>
        /// <param name="t">펄스를 적용할 대상 트랜스폼.</param>
        /// <param name="duration">전체 애니메이션 길이(초). 절반은 확대, 절반은 축소에 사용된다.</param>
        /// <param name="scaleUp">확대 배율(원본 스케일에 곱해짐).</param>
        /// <returns>코루틴 이터레이터.</returns>
        /// <remarks>
        /// <see cref="Time.unscaledDeltaTime"/>을 사용하므로 타임스케일이 변해도 일정한 속도로 재생된다.
        /// 애니메이션 종료 시 스케일은 원래 값으로 복원된다.
        /// </remarks>
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

        /// <summary>
        /// Ease-out 형태의 2차(Quad) 이징 함수.
        /// </summary>
        /// <param name="t">0~1 정규화된 진행도.</param>
        /// <returns>이징이 적용된 진행도.</returns>
        private static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);

        /// <summary>
        /// Ease-in 형태의 2차(Quad) 이징 함수.
        /// </summary>
        /// <param name="t">0~1 정규화된 진행도.</param>
        /// <returns>이징이 적용된 진행도.</returns>
        private static float EaseInQuad(float t) => t * t;
    }
}
