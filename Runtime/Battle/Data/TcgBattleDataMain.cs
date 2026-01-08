using System;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// TCG 전투 전체 상태를 나타내는 런타임 컨텍스트(전투 세션 스냅샷)입니다.
    /// 양측 플레이어 상태와 턴 진행 정보, 전투를 소유하는 세션(Owner) 참조 등을 보관합니다.
    /// </summary>
    /// <remarks>
    /// - BattleManager/CommandHandler 등에서 전투 전역 상태에 접근할 때 사용합니다.
    /// - UI/윈도우/매니저 등의 외부 시스템과의 결합은 최소화하고, 필요한 참조만 주입/보관하는 형태를 권장합니다.
    /// </remarks>
    public sealed class TcgBattleDataMain
    {
        /// <summary>
        /// 이 전투 컨텍스트를 소유하는 객체(세션/매니저 등) 참조입니다.
        /// EndTurnCommandHandler 등에서 Owner 기능 호출이 필요할 때 사용됩니다.
        /// </summary>
        public object Owner { get; private set; }

        /// <summary>
        /// Player(아군) 진영의 전투 상태입니다.
        /// </summary>
        public TcgBattleDataSide Player { get; }

        /// <summary>
        /// Enemy(상대) 진영의 전투 상태입니다.
        /// </summary>
        public TcgBattleDataSide Enemy { get; }

        /// <summary>
        /// 현재 턴 번호입니다(일반적으로 1부터 시작).
        /// </summary>
        public int TurnCount { get; set; }

        /// <summary>
        /// 현재 턴의 주인(턴을 진행 중인 진영)입니다.
        /// </summary>
        public ConfigCommonTcg.TcgPlayerSide ActiveSide { get; set; }

        /// <summary>
        /// 전투 메인 컨텍스트를 생성합니다.
        /// </summary>
        /// <param name="owner">이 전투를 소유/관리하는 객체(세션) 참조입니다.</param>
        /// <param name="player">Player 진영의 전투 상태입니다.</param>
        /// <param name="enemy">Enemy 진영의 전투 상태입니다.</param>
        /// <param name="systemMessageManager">시스템 메시지 출력/관리 객체입니다. (현재 클래스에서는 보관/사용하지 않습니다)</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="player"/> 또는 <paramref name="enemy"/>가 null인 경우(계약을 강제하고 싶을 때) 발생할 수 있습니다.
        /// </exception>
        /// <remarks>
        /// 현재 구현에서는 <paramref name="systemMessageManager"/>를 필드로 보관하지 않습니다.
        /// 향후 사용할 계획이라면 필드/프로퍼티로 승격하거나, 불필요하다면 생성자 인자에서 제거하는 것이 좋습니다.
        /// </remarks>
        public TcgBattleDataMain(
            object owner,
            TcgBattleDataSide player,
            TcgBattleDataSide enemy,
            SystemMessageManager systemMessageManager)
        {
            Owner = owner;
            Player = player;
            Enemy = enemy;
        }

        /// <summary>
        /// 지정한 진영(Side)에 해당하는 전투 상태를 반환합니다.
        /// </summary>
        /// <param name="side">조회할 진영입니다.</param>
        /// <returns><paramref name="side"/>에 해당하는 <see cref="TcgBattleDataSide"/>입니다.</returns>
        public TcgBattleDataSide GetSideState(ConfigCommonTcg.TcgPlayerSide side)
        {
            return side == ConfigCommonTcg.TcgPlayerSide.Player ? Player : Enemy;
        }

        /// <summary>
        /// 지정한 진영(Side)의 상대 진영 전투 상태를 반환합니다.
        /// </summary>
        /// <param name="side">기준이 되는 진영입니다.</param>
        /// <returns>기준 진영의 상대편 <see cref="TcgBattleDataSide"/>입니다.</returns>
        public TcgBattleDataSide GetOpponentState(ConfigCommonTcg.TcgPlayerSide side)
        {
            return side == ConfigCommonTcg.TcgPlayerSide.Player ? Enemy : Player;
        }

        /// <summary>
        /// Owner 참조를 해제합니다.
        /// </summary>
        /// <remarks>
        /// 세션 종료 시(예: Session.Dispose) GC가 참조 그래프를 정리하기 쉽게 하기 위한 용도입니다.
        /// </remarks>
        public void ClearOwner()
        {
            Owner = null;
        }
    }
}
