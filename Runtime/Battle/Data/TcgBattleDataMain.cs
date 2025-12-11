
using GGemCo2DCore;

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
        /// <summary>
        /// 이 전투 컨텍스트를 소유하는 객체(Session).
        /// EndTurnCommandHandler 등에서 Session 기능 호출할 때 사용합니다.
        /// </summary>
        public object Owner { get; private set; }
        
        public TcgBattleDataSide Player { get; }
        public TcgBattleDataSide Enemy  { get; }

        /// <summary>
        /// 현재 턴 번호(1부터 시작이 일반적).
        /// </summary>
        public int TurnCount { get; set; }

        /// <summary>
        /// 현재 턴의 주인.
        /// </summary>
        public ConfigCommonTcg.TcgPlayerSide ActiveSide { get; set; }

        public TcgBattleDataMain(
            object owner,
            TcgBattleDataSide player,
            TcgBattleDataSide enemy,
            SystemMessageManager systemMessageManager)
        {
            Owner  = owner;
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
        /// <summary>
        /// Session.Dispose()에서 GC 정리를 돕기 위해 호출
        /// </summary>
        public void ClearOwner()
        {
            Owner = null;
        }
    }
}