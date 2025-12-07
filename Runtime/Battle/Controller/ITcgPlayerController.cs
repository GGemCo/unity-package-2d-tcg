
namespace GGemCo2DTcg
{
    /// <summary>
    /// 사람이든 AI든, 전투에서 "행동 결정을 내리는 주체" 를 추상화한 인터페이스.
    /// - BattleManager 는 이 인터페이스만 보고 동작합니다.
    /// </summary>
    public interface ITcgPlayerController
    {
        /// <summary>
        /// 이 컨트롤러가 담당하는 플레이어 측.
        /// </summary>
        ConfigCommonTcg.TcgPlayerSide Side { get; }

        /// <summary>
        /// 플레이어 타입(사람/AI 난이도 등).
        /// </summary>
        ConfigCommonTcg.TcgPlayerKind Kind { get; }

        /// <summary>
        /// 이 턴에 어떤 행동을 할지 결정합니다.
        /// - outCommands 에 순서대로 명령을 채워 넣습니다.
        /// - 명령 실행 자체는 BattleManager 에서 담당합니다.
        /// </summary>
        void DecideTurnActions();
    }
}