using System.Collections;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 특정 인덱스의 유닛 아이콘을 사망 처리(페이드아웃 또는 비활성화)하는 연출 핸들러.
    /// </summary>
    public sealed class HandlerDeathFadeOut : ITcgPresentationHandler
    {
        /// <summary>
        /// 이 핸들러가 처리하는 프레젠테이션 스텝 타입.
        /// </summary>
        public TcgPresentationStepType Type => TcgPresentationStepType.DeathFadeOut;

        /// <summary>
        /// 사망 페이드아웃 연출을 실행합니다.
        /// </summary>
        /// <param name="ctx">연출 실행 컨텍스트.</param>
        /// <param name="step">사망 대상의 진영/인덱스 정보가 포함된 스텝.</param>
        /// <returns>코루틴 이터레이터.</returns>
        public IEnumerator Play(TcgPresentationContext ctx, TcgPresentationStep step)
        {
            var field = ctx.GetFieldWindow(step.Side);
            if (field == null) yield break;

            var icon = field.GetIconByIndex(step.ToIndex);
            if (icon == null)
            {
                // 아이콘이 아직 반영되지 않은 프레임일 수 있으므로 짧게 대기
                yield return new WaitForSecondsRealtime(0.05f);
                yield break;
            }

            var cg = icon.GetComponent<CanvasGroup>();
            if (cg == null)
            {
                // CanvasGroup이 없으면 안전하게 즉시 비활성화로 처리
                icon.gameObject.SetActive(false);
                yield return new WaitForSecondsRealtime(0.05f);
                yield break;
            }

            yield return TcgUiTween.FadeTo(cg, 0f, field.timeToFadeOut);
        }
    }
}