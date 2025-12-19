using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 커맨드 트레이스의 프레젠테이션 스텝을 타입별 핸들러로 매핑하여 순차 실행하는 러너.
    /// </summary>
    public sealed class TcgPresentationRunner
    {
        /// <summary>
        /// 스텝 타입 → 핸들러 매핑 테이블.
        /// </summary>
        private readonly Dictionary<TcgPresentationStepType, ITcgPresentationHandler> _handlers;

        /// <summary>
        /// 주어진 핸들러들을 스텝 타입 기준으로 등록합니다.
        /// </summary>
        /// <param name="handlers">등록할 핸들러 목록.</param>
        public TcgPresentationRunner(IEnumerable<ITcgPresentationHandler> handlers)
        {
            _handlers = new Dictionary<TcgPresentationStepType, ITcgPresentationHandler>(16);
            foreach (var h in handlers)
            {
                if (h == null) continue;
                _handlers[h.Type] = h;
            }
        }

        /// <summary>
        /// 트레이스 목록을 순회하며, 성공한 커맨드 결과의 프레젠테이션 스텝을 순차 실행합니다.
        /// </summary>
        /// <param name="ctx">연출 실행 컨텍스트.</param>
        /// <param name="traces">커맨드 트레이스 목록.</param>
        /// <param name="coroutineHost">코루틴 실행 호스트(외부에서 중단/정리할 수 있음).</param>
        /// <param name="shouldStop">중단 조건(전투 종료, 세션 해제 등).</param>
        /// <param name="perStepEnded">각 스텝 종료 후 호출할 콜백(전투 종료 체크 등).</param>
        /// <returns>코루틴 이터레이터.</returns>
        public IEnumerator Run(
            TcgPresentationContext ctx,
            IReadOnlyList<TcgBattleCommandTrace> traces,
            MonoBehaviour coroutineHost,
            System.Func<bool> shouldStop,
            System.Action perStepEnded)
        {
            if (traces == null) yield break;

            foreach (var trace in traces)
            {
                if (shouldStop()) yield break;

                var result = trace.Result;
                if (result == null || !result.Success || !result.HasPresentation) continue;

                foreach (var step in result.PresentationSteps)
                {
                    if (shouldStop()) yield break;

                    if (_handlers.TryGetValue(step.Type, out var handler) && handler != null)
                    {
                        yield return handler.Play(ctx, step);
                    }
                    else
                    {
                        // 알 수 없는 StepType은 무시(안전)
                        yield return null;
                    }

                    perStepEnded?.Invoke();
                }
            }
        }
    }
}
