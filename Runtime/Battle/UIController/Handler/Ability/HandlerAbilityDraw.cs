using System.Collections;
using GGemCo2DCore;
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

            var attackerHandWindow = ctx.GetUIHandWindowBySide(step.Side);
            if (attackerHandWindow == null) yield break;
            
            // 손패 슬롯은 제거된 것처럼 보이도록 비활성화한다.
            var attackerSlot = attackerHandWindow.GetSlotByIndex(step.FromIndex);
            if (attackerSlot)
            {
                attackerSlot.gameObject.SetActive(false);
            }

            // 사용하지 않는 슬롯부터 채워야 하기 때문에,
            // Hand 윈도우에서 사용하지 않는 슬롯 중 제일 낮은 슬롯 index 찾기
            int lowestIndex = 99999999;
            foreach (var slot in attackerHandWindow.slots)
            {
                UISlot uiSlot = slot.GetComponent<UISlot>();
                if (uiSlot == null) continue;
                // 사용한 카드 슬롯은 스킵 하기
                if (uiSlot.index == step.FromIndex) continue;
                // 보여지고 있는 슬롯은 스킵 하기
                if (uiSlot.gameObject.activeSelf) continue;
                if (uiSlot.index < lowestIndex) lowestIndex = uiSlot.index;
            }
            
            // 실제로 추가된 카드 인덱스가 전달된 경우, 해당 카드 아이콘을 우선 강조한다.
            if (payload.AddedHandIndices != null && payload.AddedHandIndices.Count > 0)
            {
                for (int i = 0; i < payload.AddedHandIndices.Count; i++)
                {
                    var fromCard = payload.AddedCards[i];
                    
                    // 필드 슬롯에 아이콘을 생성/갱신한다(카드 UID 기준).
                    attackerHandWindow.SetIconCount(lowestIndex, fromCard.Uid, 1);
                    
                    // 2) 아이콘 오브젝트를 찾는다 (한 프레임 대기하면 생성/바인딩 안정성이 올라감)
                    yield return null;
                    
                    var targetSlot = attackerHandWindow.GetSlotByIndex(lowestIndex);
                    if (targetSlot != null)
                    {
                        var defaultFadeOption = UiFadeUtility.FadeOptions.Default;
                        defaultFadeOption.easeType = ctx.UICutsceneSettings.handToFieldFadeInEasing;
                        defaultFadeOption.startAlpha = 1f;
                        yield return UiFadeUtility.FadeIn(
                            attackerHandWindow,
                            targetSlot.gameObject,
                            ctx.UICutsceneSettings.handToFieldFadeInDuration,
                            defaultFadeOption);
                    }
                    else
                    {
                        yield return new WaitForSeconds(0.05f);
                    }

                    lowestIndex++;
                }
            }

            // 손패 윈도우 펄스(짧고 확실하게)
            yield return CoPulseTransform(attackerHandWindow.transform, duration: 0.18f, scaleUp: 1.06f);
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
