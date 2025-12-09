using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 전투 세션과 전투 관련 UI 윈도우들을 연결/갱신/해제하는 역할을 하는 코디네이터.
    /// </summary>
    public sealed class TcgBattleUiController
    {
        private UIWindowTcgFieldEnemy  _fieldEnemy;
        private UIWindowTcgFieldPlayer _fieldPlayer;
        private UIWindowTcgHandPlayer  _handPlayer;
        private UIWindowTcgHandEnemy   _handEnemy;
        private UIWindowTcgBattleHud   _battleHud;

        public bool IsReady =>
            _fieldEnemy  != null &&
            _fieldPlayer != null &&
            _handPlayer  != null &&
            _handEnemy   != null &&
            _battleHud   != null;

        /// <summary>
        /// SceneGame 의 UIWindowManager 에서 전투 관련 윈도우를 찾아와 보관합니다.
        /// </summary>
        public bool TrySetupWindows()
        {
            var wm = SceneGame.Instance?.uIWindowManager;
            if (wm == null)
            {
                GcLogger.LogError($"[{nameof(TcgBattleUiController)}] {nameof(UIWindowManager)} 를 찾을 수 없습니다.");
                return false;
            }

            _fieldEnemy  = wm.GetUIWindowByUid<UIWindowTcgFieldEnemy>(UIWindowConstants.WindowUid.TcgFieldEnemy);
            _fieldPlayer = wm.GetUIWindowByUid<UIWindowTcgFieldPlayer>(UIWindowConstants.WindowUid.TcgFieldPlayer);
            _handPlayer  = wm.GetUIWindowByUid<UIWindowTcgHandPlayer>(UIWindowConstants.WindowUid.TcgHandPlayer);
            _handEnemy   = wm.GetUIWindowByUid<UIWindowTcgHandEnemy>(UIWindowConstants.WindowUid.TcgHandEnemy);
            _battleHud   = wm.GetUIWindowByUid<UIWindowTcgBattleHud>(UIWindowConstants.WindowUid.TcgBattleHud);

            if (!IsReady)
            {
                GcLogger.LogError($"[{nameof(TcgBattleUiController)}] 전투 UI 윈도우 중 일부를 찾을 수 없습니다.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 윈도우 활성/비활성.
        /// </summary>
        public void ShowAll(bool isShow)
        {
            _fieldEnemy?.Show(isShow);
            _fieldPlayer?.Show(isShow);
            _handPlayer?.Show(isShow);
            _handEnemy?.Show(isShow);
            _battleHud?.Show(isShow);
        }

        /// <summary>
        /// BattleManager 와 연동하여, 윈도우에 BattleManager/Side 정보를 바인딩합니다.
        /// </summary>
        public void BindBattleManager(TcgBattleManager manager)
        {
            if (!IsReady) return;

            _handPlayer.SetBattleManager(manager, ConfigCommonTcg.TcgPlayerSide.Player);
            // _handEnemy.SetBattleManager(manager, ConfigCommonTcg.TcgPlayerSide.Enemy);
            // _fieldPlayer.SetBattleManager(manager);
            _fieldEnemy.SetBattleManager(manager);
            // _battleHud.SetBattleManager(manager);
        }

        /// <summary>
        /// 현재 전투 데이터를 기준으로 모든 윈도우를 갱신합니다.
        /// </summary>
        public void RefreshAll(TcgBattleDataMain context)
        {
            if (!IsReady || context == null) return;

            var player = context.Player;
            var enemy  = context.Enemy;

            _handPlayer.RefreshHand(player.Hand);
            _handEnemy.RefreshHand(enemy.Hand);
            // _fieldPlayer.RefreshBoard(player, enemy);
            _fieldEnemy.RefreshBoard(player, enemy);
            // _battleHud.Refresh(context);
        }

        /// <summary>
        /// 참조를 해제합니다. (씬 전환 등)
        /// </summary>
        public void Release()
        {
            _fieldEnemy  = null;
            _fieldPlayer = null;
            _handPlayer  = null;
            _handEnemy   = null;
            _battleHud   = null;
        }
    }
}
