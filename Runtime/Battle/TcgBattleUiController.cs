using GGemCo2DCore;
using R3;

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
        private readonly CompositeDisposable _disposables = new();
        
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
            var windowManager = SceneGame.Instance?.uIWindowManager;
            if (windowManager == null)
            {
                GcLogger.LogError($"[{nameof(TcgBattleUiController)}] {nameof(UIWindowManager)} 를 찾을 수 없습니다.");
                return false;
            }

            _fieldEnemy  = GetWindow<UIWindowTcgFieldEnemy>(windowManager, UIWindowConstants.WindowUid.TcgFieldEnemy);
            _fieldPlayer = GetWindow<UIWindowTcgFieldPlayer>(windowManager, UIWindowConstants.WindowUid.TcgFieldPlayer);
            _handPlayer  = GetWindow<UIWindowTcgHandPlayer>(windowManager, UIWindowConstants.WindowUid.TcgHandPlayer);
            _handEnemy   = GetWindow<UIWindowTcgHandEnemy>(windowManager, UIWindowConstants.WindowUid.TcgHandEnemy);
            _battleHud   = GetWindow<UIWindowTcgBattleHud>(windowManager, UIWindowConstants.WindowUid.TcgBattleHud);

            if (IsReady) return true;
            
            GcLogger.LogError($"[{nameof(TcgBattleUiController)}] 전투 UI 윈도우 중 일부를 찾을 수 없습니다.");
            return false;

        }

        private static TWindow GetWindow<TWindow>(UIWindowManager windowManager, UIWindowConstants.WindowUid uid)
            where TWindow : UIWindow
        {
            return windowManager.GetUIWindowByUid<TWindow>(uid);
        }

        /// <summary>
        /// 윈도우 활성/비활성.
        /// </summary>
        public void ShowAll(bool isShow)
        {
            if (!IsReady)
                return;

            _fieldEnemy.Show(isShow);
            _fieldPlayer.Show(isShow);
            _handPlayer.Show(isShow);
            _handEnemy.Show(isShow);
            _battleHud.Show(isShow);
        }

        /// <summary>
        /// BattleManager 와 연동하여, 윈도우에 BattleManager/Side 정보를 바인딩합니다.
        /// </summary>
        public void BindBattleManager(TcgBattleManager manager, TcgBattleSession session)
        {
            if (!IsReady || manager == null || session == null)
                return;

            BindMana(session);

            _handPlayer.SetBattleManager(manager);
            // _handEnemy.SetBattleManager(manager, ConfigCommonTcg.TcgPlayerSide.Enemy);
            // _fieldPlayer.SetBattleManager(manager);
            _fieldEnemy.SetBattleManager(manager);
            // _battleHud.SetBattleManager(manager);
        }

        private void BindMana(TcgBattleSession session)
        {
            var context = session.Context;
            var player  = context.Player;
            var enemy   = context.Enemy;

            // Player
            player.CurrentMana
                .Subscribe(_ => UpdatePlayerMana(player))
                .AddTo(_disposables);

            player.CurrentManaMax
                .Subscribe(_ => UpdatePlayerMana(player))
                .AddTo(_disposables);

            // Enemy
            enemy.CurrentMana
                .Subscribe(_ => UpdateEnemyMana(enemy))
                .AddTo(_disposables);

            enemy.CurrentManaMax
                .Subscribe(_ => UpdateEnemyMana(enemy))
                .AddTo(_disposables);
        }

        private void UpdatePlayerMana(TcgBattleDataSide player)
        {
            if (_handPlayer == null)
                return;

            _handPlayer.SetMana(player.CurrentManaValue, player.CurrentManaValueMax);
        }

        private void UpdateEnemyMana(TcgBattleDataSide enemy)
        {
            if (_handEnemy == null)
                return;

            _handEnemy.SetMana(enemy.CurrentManaValue, enemy.CurrentManaValueMax);
        }

        /// <summary>
        /// 현재 전투 데이터를 기준으로 모든 윈도우를 갱신합니다.
        /// </summary>
        public void RefreshAll(TcgBattleDataMain context)
        {
            if (!IsReady || context == null)
                return;

            var player = context.Player;
            var enemy  = context.Enemy;

            _handPlayer.RefreshHand(player);
            _handEnemy.RefreshHand(enemy);
            _fieldPlayer.RefreshBoard(player);
            _fieldEnemy.RefreshBoard(enemy);
            // _battleHud.Refresh(context);
        }

        /// <summary>
        /// 참조를 해제합니다. (씬 전환 등)
        /// </summary>
        public void Release()
        {
            _disposables.Clear(); // 또는 _disposables.Dispose();

            _fieldEnemy.Release();
            _fieldPlayer.Release();
            _handPlayer.Release();
            _handEnemy.Release();
            _battleHud.Release();
            
            _fieldEnemy  = null;
            _fieldPlayer = null;
            _handPlayer  = null;
            _handEnemy   = null;
            _battleHud   = null;
        }
    }
}
