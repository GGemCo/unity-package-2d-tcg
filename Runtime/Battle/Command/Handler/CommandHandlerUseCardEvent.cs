namespace GGemCo2DTcg
{
    /// <summary>
    /// 손패의 이벤트(Event) 카드를 사용(Play)하는 커맨드를 처리합니다.
    /// </summary>
    public sealed class CommandHandlerUseCardEvent : CommandHandlerBase, ITcgBattleCommandHandler
    {
        public ConfigCommonTcg.TcgBattleCommandType CommandType => ConfigCommonTcg.TcgBattleCommandType.UseCardEvent;
        public CommandResult Execute(TcgBattleDataMain context, in TcgBattleCommand command)
        {
            return CommandResult.Ok();
        }
    }
}
