
namespace GGemCo2DTcg
{
    /// <summary>
    /// BattleCommandType 별 실행 로직을 분리하기 위한 핸들러 인터페이스.
    /// </summary>
    public interface ITcgBattleCommandHandler
    {
        /// <summary>
        /// 단일 전투 명령을 실행합니다.
        /// </summary>
        /// <param name="battleManager">전투 매니저 인스턴스.</param>
        /// <param name="actor">명령을 수행하는 쪽의 전투 상태.</param>
        /// <param name="opponent">상대 쪽의 전투 상태.</param>
        /// <param name="command">실제 명령 데이터.</param>
        void Execute(
            TcgBattleManager battleManager,
            TcgBattleDataSide actor,
            TcgBattleDataSide opponent,
            TcgBattleCommand command);
    }
}