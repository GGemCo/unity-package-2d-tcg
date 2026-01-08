namespace GGemCo2DTcg
{
    /// <summary>
    /// 전투 커맨드 실행과 그 결과(<see cref="CommandResult"/>)를 1:1로 기록하는 트레이스 데이터입니다.
    /// </summary>
    /// <remarks>
    /// UI는 <see cref="TcgBattleCommandTrace"/> 목록을 순서대로 재생하여 연출(타임라인)을 구성할 수 있습니다.
    /// </remarks>
    public readonly struct TcgBattleCommandTrace
    {
        /// <summary>
        /// 실행된 전투 커맨드입니다.
        /// </summary>
        public TcgBattleCommand Command { get; }

        /// <summary>
        /// 커맨드 실행 결과입니다(성공/실패 및 연출 Step 포함 가능).
        /// </summary>
        public CommandResult Result { get; }

        /// <summary>
        /// <see cref="TcgBattleCommandTrace"/>를 생성합니다.
        /// </summary>
        /// <param name="command">실행된 전투 커맨드입니다.</param>
        /// <param name="result">커맨드 실행 결과입니다.</param>
        public TcgBattleCommandTrace(in TcgBattleCommand command, CommandResult result)
        {
            Command = command;
            Result = result;
        }
    }
}