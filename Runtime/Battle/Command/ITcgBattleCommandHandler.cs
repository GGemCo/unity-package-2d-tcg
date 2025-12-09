using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 전투 중 발생하는 모든 명령(PlayCard, Attack, EndTurn 등)을 처리하기 위한 핸들러 인터페이스.
    /// 각 핸들러는 Stateless 하게 유지하여, BattleManager가 싱글턴처럼 재사용할 수 있도록 설계합니다.
    /// </summary>
    public interface ITcgBattleCommandHandler
    {
        /// <summary>
        /// 해당 핸들러가 처리할 수 있는 명령 타입.
        /// 예: PlayCard, AttackUnit, AttackHero, EndTurn 등
        /// </summary>
        ConfigCommonTcg.TcgBattleCommandType CommandType { get; }

        /// <summary>
        /// 실제 명령을 실행합니다.
        /// </summary>
        /// <param name="context">현재 전투 전체 컨텍스트(TcgBattleDataMain)</param>
        /// <param name="command">실행할 전투 명령 구조체</param>
        void Execute(TcgBattleDataMain context, in TcgBattleCommand command);
    }
}