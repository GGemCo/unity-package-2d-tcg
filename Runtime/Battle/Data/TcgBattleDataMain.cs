
namespace GGemCo2DTcg
{
    /// <summary>
    /// TCG 전투 전체 상태를 나타내는 런타임 컨텍스트.
    /// - 양측 플레이어 상태
    /// - 전체 턴 정보, 설정값 참조
    /// - BattleManager/설정/윈도우 등 전역 참조
    /// </summary>
    public sealed class TcgBattleDataMain
    {
        public TcgBattleDataSide Player { get; }
        public TcgBattleDataSide Enemy  { get; }

        /// <summary>
        /// 현재 턴 번호(1부터 시작이 일반적).
        /// </summary>
        public int TurnNumber { get; set; }

        /// <summary>
        /// 현재 턴의 주인.
        /// </summary>
        public ConfigCommonTcg.TcgPlayerSide ActiveSide { get; set; }

        /// <summary>
        /// 전투를 소유하는 매니저(명령 실행 시 사용).
        /// </summary>
        public TcgBattleManager BattleManager { get; }

        public TcgBattleDataMain(
            TcgBattleManager battleManager,
            TcgBattleDataSide player,
            TcgBattleDataSide enemy)
        {
            BattleManager = battleManager;
            Player = player;
            Enemy = enemy;
        }

        /// <summary>
        /// 특정 Side 에 해당하는 상태를 반환합니다.
        /// </summary>
        public TcgBattleDataSide GetSideState(ConfigCommonTcg.TcgPlayerSide side)
        {
            return side == ConfigCommonTcg.TcgPlayerSide.Player ? Player : Enemy;
        }

        public TcgBattleDataSide GetOpponentState(ConfigCommonTcg.TcgPlayerSide side)
        {
            return side == ConfigCommonTcg.TcgPlayerSide.Player ? Enemy : Player;
        }
    }
}