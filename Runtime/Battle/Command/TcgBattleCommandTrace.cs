namespace GGemCo2DTcg
{
    /// <summary>
    /// 커맨드 실행과 그 결과(CommandResult)를 1:1로 기록한 Trace.
    /// UI는 이 Trace 목록을 순서대로 재생하여 연출을 구성할 수 있습니다.
    /// </summary>
    public readonly struct TcgBattleCommandTrace
    {
        public TcgBattleCommand Command { get; }
        public CommandResult Result { get; }

        public TcgBattleCommandTrace(in TcgBattleCommand command, CommandResult result)
        {
            Command = command;
            Result = result;
        }
    }
}