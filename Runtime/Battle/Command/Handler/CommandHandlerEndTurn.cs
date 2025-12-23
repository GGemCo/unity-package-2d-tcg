using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 턴 종료 명령을 처리하는 핸들러.
    /// 실제 턴 전환 로직은 TcgBattleSession.EndTurn() 에 위임하여
    /// 규칙 변경이 필요할 때 단일 지점만 수정하도록 구성합니다.
    /// </summary>
    public sealed class CommandHandlerEndTurn : ITcgBattleCommandHandler
    {
        public ConfigCommonTcg.TcgBattleCommandType CommandType =>
            ConfigCommonTcg.TcgBattleCommandType.EndTurn;

        public CommandResult Execute(TcgBattleDataMain context, in TcgBattleCommand command)
        {
            // Session 없이 직접 턴 로직을 수행하면 유지보수 비용이 증가하므로,
            // context.owner(TcgBattleManager/TcgBattleSession) 를 활용하여 Session EndTurn을 호출하도록 합니다.

            if (context.Owner is not TcgBattleSession session)
            {
                GcLogger.LogError($"[{nameof(CommandHandlerEndTurn)}] TcgBattleDataMain.Owner 가 Session 이 아닙니다.");
                return CommandResult.Fail("Error_Tcg_NoBattleSession");
            }
            session.EndTurn();
            return CommandResult.Ok();
        }
    }
}