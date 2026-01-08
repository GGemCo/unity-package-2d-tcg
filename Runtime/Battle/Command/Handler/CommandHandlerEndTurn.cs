using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 턴 종료(EndTurn) 커맨드를 처리하는 핸들러입니다.
    /// </summary>
    /// <remarks>
    /// 실제 턴 전환/트리거 처리 로직은 <see cref="TcgBattleSession.EndTurn"/>에 위임합니다.
    /// 이렇게 하면 턴 규칙 변경 시 수정 지점을 세션으로 집중시켜 유지보수 비용을 낮출 수 있습니다.
    /// </remarks>
    public sealed class CommandHandlerEndTurn : ITcgBattleCommandHandler
    {
        /// <summary>
        /// 이 핸들러가 처리하는 커맨드 타입입니다.
        /// </summary>
        public ConfigCommonTcg.TcgBattleCommandType CommandType =>
            ConfigCommonTcg.TcgBattleCommandType.EndTurn;

        /// <summary>
        /// 턴 종료를 수행하고, UI 연출 타임라인에 포함될 <see cref="TcgPresentationStep"/>을 반환합니다.
        /// </summary>
        /// <param name="context">현재 전투 상태 컨텍스트입니다.</param>
        /// <param name="cmd">실행할 전투 커맨드 데이터입니다.</param>
        /// <returns>
        /// 처리 결과입니다. 연출이 필요한 경우 <see cref="CommandResult.OkPresentation"/> 형태로 Step을 포함할 수 있습니다.
        /// </returns>
        /// <remarks>
        /// 턴 종료 중 발생하는 트리거(EndTurn/Draw/StartTurn 등)의 Ability 연출이
        /// 동일한 타임라인에 합류할 수 있도록 <see cref="TcgBattleSession.BeginPresentationCapture"/>를 사용합니다.
        /// </remarks>
        public CommandResult Execute(TcgBattleDataMain context, in TcgBattleCommand cmd)
        {
            // Session 없이 직접 턴 로직을 수행하면 규칙 분산으로 유지보수 비용이 증가하므로,
            // context.Owner(TcgBattleManager/TcgBattleSession)를 통해 Session EndTurn을 호출합니다.
            if (context.Owner is not TcgBattleSession session)
            {
                GcLogger.LogError($"[{nameof(CommandHandlerEndTurn)}] TcgBattleDataMain.Owner 가 Session 이 아닙니다.");
                return CommandResult.Fail("Error_Tcg_NoBattleSession");
            }

            // UI 연출: 턴 종료 스텝을 타임라인에 먼저 추가합니다.
            var steps = new System.Collections.Generic.List<TcgPresentationStep>(8)
            {
                new TcgPresentationStep(
                    TcgPresentationConstants.TcgPresentationStepType.EndTurn,
                    cmd.Side,
                    fromZone: ConfigCommonTcg.TcgZone.None,
                    fromIndex: 0,
                    toZone: ConfigCommonTcg.TcgZone.None,
                    toIndex: 0)
            };

            // 턴 종료 중(EndTurn/Draw/StartTurn 등) 발생하는 트리거 Ability 연출도 동일 타임라인에 합류합니다.
            using (session.BeginPresentationCapture(steps))
            {
                session.EndTurn();
            }

            return steps.Count > 0
                ? CommandResult.OkPresentation(steps.ToArray())
                : CommandResult.Ok();
        }
    }
}
