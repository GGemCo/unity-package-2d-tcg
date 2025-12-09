namespace GGemCo2DTcg
{
    public class EndTurnCommandHandler : ITcgBattleCommandHandler
    {
        public void Execute(
            TcgBattleManager battleManager,
            TcgBattleDataSide actor,
            TcgBattleDataSide opponent,
            TcgBattleCommand command)
        {
            battleManager.ExecuteEndTurn(command.Side);
        }
    }
}