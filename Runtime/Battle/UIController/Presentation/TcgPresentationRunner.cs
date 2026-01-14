using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 커맨드 트레이스(<see cref="TcgBattleCommandTrace"/>)의 프레젠테이션 Step을 타입별 핸들러로 매핑하여
    /// 순차적으로 재생하는 러너(Runner)입니다.
    /// </summary>
    /// <remarks>
    /// - 각 Step은 <see cref="ITcgPresentationHandler"/>가 담당하며, StepType → Handler 매핑 테이블로 조회합니다.
    /// - 연출은 코루틴으로 실행되며, 외부 중단 조건(<paramref name="shouldStop"/>)에 의해 안전하게 종료될 수 있습니다.
    /// </remarks>
    public sealed class TcgPresentationRunner
    {
        /// <summary>
        /// StepType → Handler 매핑 테이블입니다.
        /// </summary>
        private readonly Dictionary<TcgPresentationConstants.TcgPresentationStepType, ITcgPresentationHandler> _handlers;

        /// <summary>
        /// 주어진 핸들러 목록을 Step 타입 기준으로 등록하여 러너를 초기화합니다.
        /// </summary>
        /// <param name="handlers">등록할 핸들러 시퀀스입니다. 동일 타입이 중복되면 마지막 항목이 우선합니다.</param>
        /// <exception cref="System.ArgumentNullException">
        /// TODO: 호출 계약에 따라 <paramref name="handlers"/>가 null일 수 없다면, null 체크 후 예외를 던지도록 보강할 수 있습니다.
        /// </exception>
        public TcgPresentationRunner(IEnumerable<ITcgPresentationHandler> handlers)
        {
            _handlers = new Dictionary<TcgPresentationConstants.TcgPresentationStepType, ITcgPresentationHandler>(16);
            foreach (var h in handlers)
            {
                if (h == null) continue;
                _handlers[h.Type] = h;
            }
        }

        /// <summary>
        /// 트레이스 목록을 순회하며, 성공한 커맨드 결과의 프레젠테이션 Step을 순차 실행합니다.
        /// </summary>
        /// <param name="ctx">연출 실행에 필요한 세션/UI/설정이 담긴 컨텍스트입니다.</param>
        /// <param name="traces">실행된 커맨드 트레이스 목록입니다.</param>
        /// <param name="coroutineHost">
        /// 코루틴 실행 호스트입니다.
        /// </param>
        /// <param name="shouldStop">
        /// 즉시 중단해야 하는지 판단하는 조건 함수입니다(전투 종료, 세션 해제 등).
        /// </param>
        /// <param name="perStepEnded">
        /// 각 Step 종료 직후 호출되는 콜백입니다(전투 종료 체크/상태 동기화 등).
        /// </param>
        /// <returns>Unity 코루틴에서 사용할 이터레이터를 반환합니다.</returns>
        /// <remarks>
        /// - <paramref name="traces"/>가 null이면 즉시 종료합니다.
        /// - StepType에 대응하는 핸들러가 없으면 해당 Step은 무시합니다(안전 동작).
        /// - EndTurn 커맨드가 아닌 경우, 커맨드 단위로 대기 시간을 삽입할 수 있습니다.
        /// </remarks>
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
                        // Step 재생은 핸들러 코루틴에 위임합니다.
                        yield return handler.Play(ctx, step);
                    }
                    else
                    {
                        // 알 수 없는 StepType은 무시(안전)
                        yield return null;
                    }

                    perStepEnded?.Invoke();
                }

                // 턴 종료 커맨드 외에는 커맨드 단위의 간격을 둡니다.
                if (trace.Command.CommandType != ConfigCommonTcg.TcgBattleCommandType.EndTurn)
                    yield return new WaitForSeconds(ctx.UICutsceneSettings.timeWaitAfterCommand);
            }
        }
    }
}
