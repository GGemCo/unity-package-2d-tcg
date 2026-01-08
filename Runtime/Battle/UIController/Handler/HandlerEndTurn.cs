using System.Collections;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 턴 종료 시점의 UI 연출을 처리하는 프리젠테이션 핸들러.
    /// </summary>
    /// <remarks>
    /// 현재 활성화된 사이드 기준으로 턴 종료 메시지를 표시하고,
    /// 턴 전환 이후 새 턴 타이머를 초기화한다.
    /// </remarks>
    public sealed class HandlerEndTurn : ITcgPresentationHandler
    {
        /// <summary>
        /// 이 핸들러가 처리하는 프리젠테이션 스텝 타입.
        /// </summary>
        public TcgPresentationConstants.TcgPresentationStepType Type =>
            TcgPresentationConstants.TcgPresentationStepType.EndTurn;

        /// <summary>
        /// 턴 종료 연출을 재생한다.
        /// </summary>
        /// <param name="ctx">세션 정보, HUD 및 설정을 제공하는 프리젠테이션 컨텍스트.</param>
        /// <param name="step">
        /// 턴 종료를 나타내는 스텝.
        /// 현재 구현에서는 <paramref name="step"/>의 세부 데이터는 사용하지 않는다.
        /// </param>
        /// <returns>코루틴 이터레이터.</returns>
        /// <remarks>
        /// HUD가 존재하지 않으면 연출을 수행하지 않는다.
        /// <see cref="TcgPresentationContext.Session"/>의 <c>ActiveSide</c> 값을 기준으로 메시지를 표시한다.
        /// </remarks>
        public IEnumerator Play(TcgPresentationContext ctx, TcgPresentationStep step)
        {
            var uiWindowTcgBattleHud = ctx.BattleHud;
            if (uiWindowTcgBattleHud == null) yield break;

            // 턴 종료 메시지 표시(현재 ActiveSide 기준)
            yield return uiWindowTcgBattleHud.ShowEndTurnText(ctx.Session.Context.ActiveSide);

            // 턴이 전환된 직후(ActiveSide가 변경된 상태) 새 턴 타이머를 시작한다.
            uiWindowTcgBattleHud.StartTurnTimer(
                ctx.Settings != null ? ctx.Settings.turnTimeLimitSeconds : 0);
        }
    }
}