using System.Collections;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// Ability 기반 추가 행동(Extra Action) 연출을 처리하는 프리젠테이션 핸들러.
    /// </summary>
    /// <remarks>
    /// 우선적으로 턴 종료 버튼을 강조(펄스)하고 +N 팝업을 표시한다.
    /// 버튼이 존재하지 않는 경우 대상 아이콘(또는 영웅 슬롯)을 폴백으로 사용한다.
    /// </remarks>
    public sealed class HandlerAbilityExtraAction : HandlerBase, ITcgPresentationHandler
    {
        /// <summary>
        /// 이 핸들러가 처리하는 프리젠테이션 스텝 타입.
        /// </summary>
        public TcgPresentationConstants.TcgPresentationStepType Type =>
            TcgPresentationConstants.TcgPresentationStepType.AbilityExtraAction;

        /// <summary>
        /// 추가 행동(Extra Action) 연출을 재생한다.
        /// </summary>
        /// <param name="ctx">HUD, UI 윈도우 및 연출 실행에 필요한 프리젠테이션 컨텍스트.</param>
        /// <param name="step">대상 존/인덱스와 추가 행동 페이로드를 포함하는 스텝.</param>
        /// <returns>코루틴 이터레이터.</returns>
        /// <remarks>
        /// 페이로드가 ExtraAction 타입이 아니거나, 추가 행동 수가 0 이하이면 연출을 수행하지 않는다.
        /// </remarks>
        public IEnumerator Play(TcgPresentationContext ctx, TcgPresentationStep step)
        {
            if (ctx == null) yield break;
            if (step.Payload is not TcgAbilityPayloadExtraAction payload) yield break;
            if (payload.ExtraActionCount <= 0) yield break;

            // 1) 우선 처리: 턴 종료 버튼 강조
            var hud = ctx.BattleHud;
            if (hud != null && hud.buttonTurnOff != null)
            {
                var btn = hud.buttonTurnOff;

                // 추가 행동 횟수를 버튼 위치에 팝업으로 표시
                ShowValueTextAt(btn.transform.position, payload.ExtraActionCount);

                // 버튼 펄스 연출로 강조
                yield return CoPulseTransform(btn.transform, duration: 0.2f, scaleUp: 1.12f);
                yield break;
            }

            // 2) 폴백 처리: 대상 아이콘(없으면 영웅 슬롯)
            var window = ctx.GetUIWindow(step.ToZone);
            if (window == null) yield break;

            var icon = window.GetIconByIndex(step.ToIndex);
            if (icon == null)
                icon = window.GetIconByIndex(ConfigCommonTcg.IndexHeroSlot);

            if (icon != null)
            {
                // NOTE: 현재는 공용 Heal 이펙트를 재사용한다(전용 ExtraAction 이펙트로 교체 가능).
                ShowEffect(icon, EffectUidHeal);

                // 추가 행동 수치를 팝업으로 표시한다.
                ShowDamageText(icon, payload.ExtraActionCount);

                // 아이콘에도 짧은 펄스 연출을 적용한다.
                yield return CoPulseTransform(icon.transform, duration: 0.16f, scaleUp: 1.06f);
            }
        }

        /// <summary>
        /// 트랜스폼에 짧은 펄스(확대/축소) 스케일 애니메이션을 적용한다.
        /// </summary>
        /// <param name="t">펄스를 적용할 대상 트랜스폼.</param>
        /// <param name="duration">전체 애니메이션 길이(초).</param>
        /// <param name="scaleUp">확대 배율(원본 스케일에 곱해짐).</param>
        /// <returns>코루틴 이터레이터.</returns>
        /// <remarks>
        /// <see cref="Time.unscaledDeltaTime"/>을 사용하므로 타임스케일 변화의 영향을 받지 않는다.
        /// 애니메이션 종료 시 스케일은 원래 값으로 복원된다.
        /// </remarks>
        private static IEnumerator CoPulseTransform(Transform t, float duration, float scaleUp)
        {
            if (t == null) yield break;
            if (duration <= 0f) yield break;

            var half = duration * 0.5f;
            var original = t.localScale;
            var peak = original * scaleUp;

            // 확대 구간
            for (float time = 0f; time < half; time += Time.unscaledDeltaTime)
            {
                var k = Mathf.Clamp01(time / half);
                t.localScale = Vector3.LerpUnclamped(original, peak, EaseOutQuad(k));
                yield return null;
            }

            // 축소 구간
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
        private static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);

        /// <summary>
        /// Ease-in 형태의 2차(Quad) 이징 함수.
        /// </summary>
        private static float EaseInQuad(float t) => t * t;
    }
}
