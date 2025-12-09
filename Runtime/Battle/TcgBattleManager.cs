using System;
using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// TCG 전투 시작을 관리하는 메인 엔트리 클래스.
    /// - 저장된 덱/플레이어 데이터 검증
    /// - 셔플 컨텍스트 생성
    /// - 런타임 덱 생성 및 셔플
    /// - UI 윈도우 초기화
    /// </summary>
    public class TcgBattleManager
    {
        public Action onExecuteCommand;
        
        private UIWindowTcgFieldEnemy _uiWindowTcgFieldEnemy;
        private UIWindowTcgFieldPlayer _uiWindowTcgFieldPlayer;
        private UIWindowTcgHandEnemy _uiWindowTcgHandEnemy;
        private UIWindowTcgHandPlayer _uiWindowTcgHandPlayer;
        private UIWindowTcgBattleHud _uiWindowTcgBattleHud;

        // === 컨트롤러 추가 ===
        private TcgBattleControllerPlayer _battleControllerPlayer;
        private TcgBattleControllerEnemy _battleControllerEnemy;

        private SystemMessageManager _systemMessageManager;
        
        // 현재 턴 진행 여부
        private bool _isPlayerTurn;
        private int _turnNumber;
        
        /// <summary>
        /// BattleCommandType 별 실행 핸들러 목록.
        /// </summary>
        private readonly Dictionary<ConfigCommonTcg.TcgBattleCommandType, ITcgBattleCommandHandler> _commandHandlers
            = new Dictionary<ConfigCommonTcg.TcgBattleCommandType, ITcgBattleCommandHandler>();
        
        /// <summary>
        /// 외부에서 커스텀 BattleCommandType 핸들러를 등록할 수 있도록 합니다.
        /// 같은 타입이 이미 등록되어 있으면 덮어씁니다.
        /// </summary>
        public void RegisterCommandHandler(
            ConfigCommonTcg.TcgBattleCommandType commandType,
            ITcgBattleCommandHandler handler)
        {
            _commandHandlers[commandType] = handler ?? throw new ArgumentNullException(nameof(handler));
        }
        
        /// <summary>
        /// 기본 제공 BattleCommandType 핸들러들을 등록합니다.
        /// </summary>
        private void InitializeDefaultCommandHandlers()
        {
            RegisterCommandHandler(
                ConfigCommonTcg.TcgBattleCommandType.PlayCardFromHand,
                new PlayCardCommandHandler());

            RegisterCommandHandler(
                ConfigCommonTcg.TcgBattleCommandType.AttackUnit,
                new AttackUnitCommandHandler());

            RegisterCommandHandler(
                ConfigCommonTcg.TcgBattleCommandType.AttackHero,
                new AttackHeroCommandHandler());

            RegisterCommandHandler(
                ConfigCommonTcg.TcgBattleCommandType.EndTurn,
                new EndTurnCommandHandler());
        }
        /// <summary>
        /// TCG 패키지 매니저로부터 필수 의존성을 주입합니다.
        /// </summary>
        /// <param name="packageManager">TCG 패키지 매니저 인스턴스.</param>
        public void Initialize(TcgPackageManager packageManager)
        {
            // BattleCommandType 기본 핸들러 등록
            InitializeDefaultCommandHandlers();
        }

        public void InitializeByStart()
        {
            if (!SceneGame.Instance) return;
            _systemMessageManager = SceneGame.Instance.systemMessageManager;
        }

        /// <summary>
        /// 저장된 정보(기본 덱)를 기반으로 전투를 시작합니다.
        /// </summary>
        public void StartBattle()
        {
            // todo. 대결 시작 단계 적용하기. 로딩(대결 준비) 효과가 필요할 수 있음
            
            // 1. 필드 윈도우 준비
            if (!EnsureBattleWindows())
                return;
            
            // 2. 컨트롤러 초기화
            SetupBattleRuntime();
            
            // 3. UI에 BattleManager / 플레이어 상태 연결
            InitializeUiForBattle();
            
            // 4. 필드 윈도우 표시
            ShowWindows(true);
            
            // 5. Hand 윈도우 새로고침
            ProcessFirstDraw();
        }

        #region UI Windows

        /// <summary>
        /// 전투에 필요한 UI 윈도우(필드, 내 패)를 확보합니다.
        /// 없으면 로그를 남기고 false 를 반환합니다.
        /// </summary>
        private bool EnsureBattleWindows()
        {
            var windowManager = SceneGame.Instance?.uIWindowManager;
            if (windowManager == null)
            {
                GcLogger.LogError("SceneGame 또는 UIWindowManager 가 없습니다. 전투 UI를 열 수 없습니다.");
                return false;
            }

            if (_uiWindowTcgFieldEnemy == null)
            {
                _uiWindowTcgFieldEnemy = windowManager.GetUIWindowByUid<UIWindowTcgFieldEnemy>(UIWindowConstants.WindowUid.TcgFieldEnemy);
            }

            if (_uiWindowTcgFieldEnemy == null)
            {
                GcLogger.LogError($"{nameof(UIWindowTcgFieldEnemy)} 윈도우가 UI 매니저에 없습니다.");
                return false;
            }
            if (_uiWindowTcgFieldPlayer == null)
            {
                _uiWindowTcgFieldPlayer = windowManager.GetUIWindowByUid<UIWindowTcgFieldPlayer>(UIWindowConstants.WindowUid.TcgFieldPlayer);
            }

            if (_uiWindowTcgFieldPlayer == null)
            {
                GcLogger.LogError($"{nameof(UIWindowTcgFieldPlayer)} 윈도우가 UI 매니저에 없습니다.");
                return false;
            }
            
            if (_uiWindowTcgHandEnemy == null)
            {
                _uiWindowTcgHandEnemy = windowManager.GetUIWindowByUid<UIWindowTcgHandEnemy>(UIWindowConstants.WindowUid.TcgHandEnemy);
            }

            if (_uiWindowTcgHandEnemy == null)
            {
                GcLogger.LogError($"{nameof(UIWindowTcgHandEnemy)} 윈도우가 UI 매니저에 없습니다.");
                return false;
            }
            if (_uiWindowTcgHandPlayer == null)
            {
                _uiWindowTcgHandPlayer = windowManager.GetUIWindowByUid<UIWindowTcgHandPlayer>(UIWindowConstants.WindowUid.TcgHandPlayer);
            }

            if (_uiWindowTcgHandPlayer == null)
            {
                GcLogger.LogError($"{nameof(UIWindowTcgHandPlayer)} 윈도우가 UI 매니저에 없습니다.");
                return false;
            }
            
            if (_uiWindowTcgBattleHud == null)
            {
                _uiWindowTcgBattleHud = windowManager.GetUIWindowByUid<UIWindowTcgBattleHud>(UIWindowConstants.WindowUid.TcgBattleHud);
            }

            if (_uiWindowTcgBattleHud == null)
            {
                GcLogger.LogError($"{nameof(UIWindowTcgBattleHud)} 윈도우가 UI 매니저에 없습니다.");
                return false;
            }
            return true;
        }

        /// <summary>
        /// UI 윈도우를 표시합니다.
        /// </summary>
        private void ShowWindows(bool show)
        {
            _uiWindowTcgFieldEnemy?.Show(show);
            _uiWindowTcgFieldPlayer?.Show(show);
            _uiWindowTcgBattleHud?.Show(show);
        }
        #endregion
        
        /// <summary>
        /// 플레이어/적 측 런타임 상태와 전투 컨텍스트를 생성하고,
        /// 사람/AI 컨트롤러를 초기화합니다.
        /// </summary>
        private void SetupBattleRuntime()
        {
            // 컨트롤러 생성
            var human = new TcgBattleControllerPlayer(ConfigCommonTcg.TcgPlayerSide.Player);
            var ai    = new TcgBattleControllerEnemy(ConfigCommonTcg.TcgPlayerSide.Enemy, ConfigCommonTcg.TcgPlayerKind.AiEasy);

            _battleControllerPlayer = human;
            _battleControllerEnemy  = ai;

            _battleControllerPlayer.Initialize(this);
            _battleControllerEnemy.Initialize(this);
            
            // 플레이어 턴 시작
            _isPlayerTurn = true;
        }

        private void InitializeUiForBattle()
        {
            _uiWindowTcgFieldPlayer.SetBattleManager(this, _battleControllerPlayer);
            _uiWindowTcgFieldEnemy.SetBattleManager(this, _battleControllerEnemy);
            _uiWindowTcgHandPlayer.SetBattleManager(this, _battleControllerPlayer);
            _uiWindowTcgHandEnemy.SetBattleManager(this, _battleControllerEnemy);
        }

        private void ProcessFirstDraw()
        {
            _uiWindowTcgHandPlayer.RefreshHand();
            _uiWindowTcgHandEnemy.RefreshHand();
        }

        // /// <summary>
        // /// 한 턴에 대한 명령 목록을 순서대로 실행합니다.
        // /// </summary>
        // private void ExecuteCommands(List<TcgBattleCommand> commands)
        // {
        //     if (commands == null || commands.Count == 0)
        //         return;
        //
        //     foreach (var cmd in commands)
        //     {
        //         ExecuteCommand(cmd);
        //
        //         // TODO: 연출을 위해 코루틴으로 바꾸고,
        //         //       명령당 대기/애니메이션을 넣고 싶다면 여기서 처리.
        //     }
        // }

        private TcgBattleDataSide GetSideState(ConfigCommonTcg.TcgPlayerSide side)
        {
            return side == ConfigCommonTcg.TcgPlayerSide.Player ? _battleControllerPlayer.GetBattleDataSide() : _battleControllerEnemy.GetBattleDataSide();
        }

        private TcgBattleDataSide GetOpponentState(ConfigCommonTcg.TcgPlayerSide side)
        {
            return side == ConfigCommonTcg.TcgPlayerSide.Player ? _battleControllerEnemy.GetBattleDataSide() : _battleControllerPlayer.GetBattleDataSide();
        }
        /// <summary>
        /// 단일 명령을 실제 전투 상태/체력/마나/보드/UI 에 반영합니다.
        /// </summary>
        public void ExecuteCommand(TcgBattleCommand cmd)
        {
            if (cmd == null)
                return;

            var actor    = GetSideState(cmd.Side);
            var opponent = GetOpponentState(cmd.Side);
            
            if (!_commandHandlers.TryGetValue(cmd.CommandType, out var handler))
            {
                GcLogger.LogWarning($"[Battle] 등록되지 않은 커맨드 타입: {cmd.CommandType}");
                return;
            }
            handler.Execute(this, actor, opponent, cmd);
            
            onExecuteCommand?.Invoke();
            
            _uiWindowTcgFieldEnemy.RefreshBoard();
            _uiWindowTcgFieldPlayer.RefreshBoard();
            _uiWindowTcgHandEnemy.RefreshHand();
            _uiWindowTcgHandPlayer.RefreshHand();
        }

        public void ExecuteEndTurn(ConfigCommonTcg.TcgPlayerSide side)
        {
            // 턴 수 증가/액티브 변경
            if (side == ConfigCommonTcg.TcgPlayerSide.Player)
            {
                _isPlayerTurn = false;
            }
            else
            {
                _isPlayerTurn = true;
                _turnNumber++;
            }
            // 턴을 끝낸 쪽은 false
            var newActor = GetSideState(side);
            foreach (var unit in newActor.Board)
            {
                unit.CanAttack = false;
            }
            // 턴을 시작하는 쪽은 true
            newActor = GetOpponentState(side);
            foreach (var unit in newActor.Board)
            {
                unit.CanAttack = true;
            }
            
            // 새 턴 시작 시 마나/공격 가능 여부 리셋
            if (_isPlayerTurn) return;
            
            _battleControllerPlayer.IncreaseMaxMana(1);
            _battleControllerEnemy.IncreaseMaxMana(1);
            // AI 턴이면 바로 AI 명령 결정/실행
            RunAiTurn();
        }
        private void RunAiTurn()
        {
            _battleControllerEnemy.DecideTurnActions();
        }
        /// <summary>
        /// 대결 강제 종료
        /// </summary>
        public void EndBattleForce()
        {
            ShowWindows(false);
        }

        public void OnUiRequestPlayCard(ConfigCommonTcg.TcgPlayerSide side, TcgBattleDataCard tcgBattleDataCard)
        {
            if (!_isPlayerTurn || side != ConfigCommonTcg.TcgPlayerSide.Player)
                return;

            if (_battleControllerPlayer == null) return;
            var cmd = TcgBattleCommand.PlayCard(side, tcgBattleDataCard);
            ExecuteCommand(cmd);
        }
    }
}
