using System.Collections;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 간단한 UI 연출용 Tween 유틸.
    /// 외부 Tween 라이브러리 의존 없이 Coroutine 기반으로 동작합니다.
    /// </summary>
    public static class TcgUiTween
    {
        public static IEnumerator MoveTo(Transform transform, Vector3 worldDestination, float duration)
        {
            if (transform == null)
                yield break;

            if (duration <= 0f)
            {
                transform.position = worldDestination;
                yield break;
            }

            var start = transform.position;
            var time = 0f;

            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                var t = Mathf.Clamp01(time / duration);
                transform.position = Vector3.LerpUnclamped(start, worldDestination, t);
                yield return null;
            }

            transform.position = worldDestination;
        }

        public static IEnumerator FadeTo(CanvasGroup canvasGroup, float toAlpha, float duration)
        {
            if (canvasGroup == null)
                yield break;

            if (duration <= 0f)
            {
                canvasGroup.alpha = toAlpha;
                yield break;
            }

            var startAlpha = canvasGroup.alpha;
            var time = 0f;

            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                var t = Mathf.Clamp01(time / duration);
                canvasGroup.alpha = Mathf.LerpUnclamped(startAlpha, toAlpha, t);
                yield return null;
            }

            canvasGroup.alpha = toAlpha;
        }
    }
}