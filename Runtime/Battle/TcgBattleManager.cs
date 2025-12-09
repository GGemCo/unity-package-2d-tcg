using System;
using System.Collections.Generic;
using GGemCo2DCore;
using UnityEngine;

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
        
        private SystemMessageManager _systemMessageManager;
        
        private UIWindowTcgFieldEnemy _uiWindowTcgFieldEnemy;
        private UIWindowTcgFieldPlayer _uiWindowTcgFieldPlayer;
        private UIWindowTcgHandEnemy _uiWindowTcgHandEnemy;
        private UIWindowTcgHandPlayer _uiWindowTcgHandPlayer;
        private UIWindowTcgBattleHud _uiWindowTcgBattleHud;

        // === 전투 런타임 & 컨트롤러 추가 ===
        private TcgBattleDataMain _battleDataMain;
        private ITcgPlayerController _playerController;
        private ITcgPlayerController _enemyController;
        // 턴 처리용 임시 버퍼
        private readonly List<TcgBattleCommand> _commandBuffer = new List<TcgBattleCommand>(32);

        // 현재 턴 진행 여부
        private bool _isPlayerTurn;
        
        private GGemCoTcgSettings _tcgSettings;
        private SeedManager _seedManager;
        private TableTcgCard _tableTcgCards;
        private SaveDataManagerTcg _saveDataManagerTcg;
        
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
            _tcgSettings = AddressableLoaderSettingsTcg.Instance.tcgSettings;
            _seedManager = new SeedManager();
            _tableTcgCards = TableLoaderManagerTcg.Instance.TableTcgCard;
            _saveDataManagerTcg = packageManager.saveDataManagerTcg;
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
            var buildPlayerDeckRuntime = BuildPlayerDeckRuntime();
            var playerSide = new TcgBattleDataSide(ConfigCommonTcg.TcgPlayerSide.Player, buildPlayerDeckRuntime);
            playerSide.SetHeroHp(100, 100);
            playerSide.SetMana(1, 10);

            var enemyDeckRuntime = BuildEnemyDeckRuntime();
            var enemySide = new TcgBattleDataSide(ConfigCommonTcg.TcgPlayerSide.Enemy, enemyDeckRuntime);
            enemySide.SetHeroHp(100, 100);
            enemySide.SetMana(1, 10);

            _battleDataMain = new TcgBattleDataMain(
                this,
                playerSide,
                enemySide)
            {
                TurnNumber = 1,
                ActiveSide = ConfigCommonTcg.TcgPlayerSide.Player
            };

            // 컨트롤러 생성
            var human = new TcgBattleControllerPlayer(ConfigCommonTcg.TcgPlayerSide.Player);
            var ai    = new TcgBattleControllerEnemy(ConfigCommonTcg.TcgPlayerSide.Enemy, ConfigCommonTcg.TcgPlayerKind.AiEasy);

            _playerController = human;
            _enemyController  = ai;

            _playerController.Initialize(_battleDataMain);
            _enemyController.Initialize(_battleDataMain);

            // === 여기서 초기 손패 드로우 ===
            DrawCards(playerSide, _tcgSettings.startingHandCardCount);
            DrawCards(enemySide,  _tcgSettings.startingHandCardCount);
            
            // UI에 BattleManager / 플레이어 상태 연결
            InitializeUiForBattle();
        }

        private TcgBattleDataDeck<TcgBattleDataCard> BuildPlayerDeckRuntime()
        {
            var myDeckData = _saveDataManagerTcg.MyDeck;
            var playerDataTcg = _saveDataManagerTcg.PlayerTcg;
            var deck = myDeckData.GetDeckInfoByIndex(playerDataTcg.defaultDeckIndex);
            // 2. 셔플 컨텍스트 생성
            var shuffleContext = BuildShuffleContext();
            // 3. 런타임 덱 생성 및 셔플
            List<TcgBattleDataCard> runtimeCardList = TcgBattleDataDeckBuilder.BuildRuntimeDeck(deck.cardList);
            var deckRuntime = new TcgBattleDataDeck<TcgBattleDataCard>(shuffleContext);
            deckRuntime.SetCards(runtimeCardList);
            deckRuntime.Shuffle();
#if UNITY_EDITOR
            // 에디터에서만 디버그 로그 출력
            LogShuffledDeckForDebug(deckRuntime);
#endif
            return deckRuntime;
        }

        private void UpdateSeedManager()
        {
            _seedManager.SetFixedSeed(null);
            _seedManager.ApplySeed();
        }
        /// <summary>
        /// 설정값과 테스트 시드를 기반으로 셔플 메타데이터를 생성합니다.
        /// - 테스트 시드가 0보다 크면 고정 시드 사용
        /// - costWeights 설정이 있으면 Weighted 모드, 없으면 PureRandom 모드
        /// </summary>
        private ShuffleMetaData BuildShuffleContext()
        {
            // 1) 시드 설정
            UpdateSeedManager();
            
            // 2) 셔플 모드 및 설정
            var shuffleMode = ConfigCommonTcg.ShuffleMode.PureRandom;
            var shuffleConfig = new ShuffleConfig();

            int[] costWeights = _tcgSettings.costWeights;

            // costWeights 에 값이 정의되어 있으면 Weighted 모드로 전환
            if (costWeights == null || costWeights.Length <= 0)
                return new ShuffleMetaData(shuffleMode, _seedManager, shuffleConfig);
            
            shuffleMode = ConfigCommonTcg.ShuffleMode.Weighted;

            shuffleConfig.FrontLoadedCount = 10;

            if (shuffleConfig.CostWeights == null || shuffleConfig.CostWeights.Count <= 0)
                return new ShuffleMetaData(shuffleMode, _seedManager, shuffleConfig);
            int length = Mathf.Min(costWeights.Length, shuffleConfig.CostWeights.Count);
            for (int i = 0; i < length; i++)
            {
                if (costWeights[i] <= 0) continue;
                shuffleConfig.CostWeights[i] = costWeights[i];
            }

            return new ShuffleMetaData(shuffleMode, _seedManager, shuffleConfig);
        }
        private TcgBattleDataDeck<TcgBattleDataCard> BuildEnemyDeckRuntime()
        {
            
            var shuffleContext = BuildShuffleContext();
            
            // TableTcgCard에서 전체 카드 리스트 가져오기
            var tableTcgCards = _tableTcgCards.GetAll();

            // AI 덱 구성 (Random은 외부 주입 or 내부 생성)
            List<int> cardUids = new List<int>();
            cardUids.Clear();
            cardUids = _tcgSettings.testDeckPreset.BuildDeckUids(tableTcgCards);

            Dictionary<int, int> cardList = new Dictionary<int, int>();
            cardList.Clear();
            foreach (int uid in cardUids)
            {
                cardList.TryAdd(uid, 1);
            }
            List<TcgBattleDataCard> runtimeCardList = TcgBattleDataDeckBuilder.BuildRuntimeDeck(cardList);

            var enemyDeckRuntime = new TcgBattleDataDeck<TcgBattleDataCard>(shuffleContext);
            enemyDeckRuntime.SetCards(runtimeCardList);
            enemyDeckRuntime.Shuffle();
            return enemyDeckRuntime;
        }

        private void InitializeUiForBattle()
        {
            // MyHand/Field에 전투 관련 정보 전달
            _uiWindowTcgHandPlayer.SetBattleManager(this, ConfigCommonTcg.TcgPlayerSide.Player);
            _uiWindowTcgFieldEnemy.SetBattleManager(this);

            // 처음에는 플레이어의 손패/필드 상황을 표시
            var playerState  = _battleDataMain.Player;
            var enemyState   = _battleDataMain.Enemy;

            _uiWindowTcgHandPlayer.RefreshHand(playerState.Hand);
            _uiWindowTcgHandEnemy.RefreshHand(enemyState.Hand);
            _uiWindowTcgFieldEnemy.RefreshBoard(playerState, enemyState);

            // 플레이어 턴 시작
            _isPlayerTurn = true;
            _battleDataMain.ActiveSide = ConfigCommonTcg.TcgPlayerSide.Player;
            
            // _uiWindowTcgFieldPlayer.SetBattleManager(this, ConfigCommonTcg.TcgPlayerSide.Player);
            // _uiWindowTcgFieldEnemy.SetBattleManager(this, _battleControllerEnemy);
            // _uiWindowTcgHandPlayer.SetBattleManager(this, _battleControllerPlayer);
            // _uiWindowTcgHandEnemy.SetBattleManager(this, _battleControllerEnemy);
        }

        private void ProcessFirstDraw()
        {
            // var playerHand = _battleControllerPlayer.GetBattleDataSide().Hand;
            // _uiWindowTcgHandPlayer.RefreshHand(playerHand);
            // _uiWindowTcgHandEnemy.RefreshHand();
        }

        /// <summary>
        /// 한 턴에 대한 명령 목록을 순서대로 실행합니다.
        /// </summary>
        private void ExecuteCommands(List<TcgBattleCommand> commands)
        {
            if (commands == null || commands.Count == 0)
                return;
        
            foreach (var cmd in commands)
            {
                ExecuteCommand(cmd);
        
                // TODO: 연출을 위해 코루틴으로 바꾸고,
                //       명령당 대기/애니메이션을 넣고 싶다면 여기서 처리.
            }
        }

        /// <summary>
        /// 단일 명령을 실제 전투 상태/체력/마나/보드/UI 에 반영합니다.
        /// </summary>
        public void ExecuteCommand(TcgBattleCommand cmd)
        {
            if (cmd == null)
                return;

            var actor    = _battleDataMain.GetSideState(cmd.Side);
            var opponent = _battleDataMain.GetOpponentState(cmd.Side);
            
            if (!_commandHandlers.TryGetValue(cmd.CommandType, out var handler))
            {
                GcLogger.LogWarning($"[Battle] 등록되지 않은 커맨드 타입: {cmd.CommandType}");
                return;
            }
            handler.Execute(this, actor, opponent, cmd);
            
            // onExecuteCommand?.Invoke();
            //
            // _uiWindowTcgFieldEnemy.RefreshBoard();
            // _uiWindowTcgFieldPlayer.RefreshBoard();
            // _uiWindowTcgHandEnemy.RefreshHand();
            // _uiWindowTcgHandPlayer.RefreshHand();
            
            _uiWindowTcgHandPlayer.RefreshHand(actor.Hand);
            _uiWindowTcgFieldEnemy.RefreshBoard(
                _battleDataMain.Player,
                _battleDataMain.Enemy);
        }

        public void ExecuteEndTurn(ConfigCommonTcg.TcgPlayerSide side)
        {
            // 턴 주인이 맞는지 검증
            if (_battleDataMain.ActiveSide != side)
                return;

            // 턴 수 증가/액티브 변경
            if (side == ConfigCommonTcg.TcgPlayerSide.Player)
            {
                _battleDataMain.ActiveSide = ConfigCommonTcg.TcgPlayerSide.Enemy;
                _isPlayerTurn = false;
            }
            else
            {
                _battleDataMain.ActiveSide = ConfigCommonTcg.TcgPlayerSide.Player;
                _isPlayerTurn = true;
                _battleDataMain.TurnNumber++;
            }

            // 새 턴 시작 시 마나/공격 가능 여부 리셋
            // 1) 마나 갱신
            var newActor = _battleDataMain.GetSideState(_battleDataMain.ActiveSide);
            var maxMana = Mathf.Min( newActor.MaxMana + 1, _tcgSettings.countMaxManaInBattle);
            newActor.SetMana(maxMana, maxMana);

            // 2) 턴 시작 드로우 1장
            DrawOneCard(newActor);
            
            // 3) 유닛 공격 가능 리셋
            foreach (var unit in newActor.Board)
            {
                unit.CanAttack = true;
            }

            // UI 토글 (내 턴에만 조작 가능하도록)
            _uiWindowTcgHandPlayer.SetInteractable(_battleDataMain.ActiveSide == ConfigCommonTcg.TcgPlayerSide.Player);

            // AI 턴이면 바로 AI 명령 결정/실행
            if (_battleDataMain.ActiveSide == ConfigCommonTcg.TcgPlayerSide.Enemy)
            {
                RunAiTurn();
            }
        }

        private void RunAiTurn()
        {
            _commandBuffer.Clear();
            _enemyController.DecideTurnActions(_battleDataMain, _commandBuffer);
            ExecuteCommands(_commandBuffer);
        }
        /// <summary>
        /// 대결 강제 종료
        /// </summary>
        public void EndBattleForce()
        {
            ShowWindows(false);
            
            // 컨트롤러 쪽 참조 해제
            _playerController?.Dispose();
            _enemyController?.Dispose();
            _playerController = null;
            _enemyController = null;

            // UI가 BattleContext를 들고 있는 경우, 거기도 해제 필요
            ReleaseUiReferences();

            // 마지막으로 Context 해제
            _battleDataMain = null;

            EndBattleForGcTest();
        }

        private void EndBattleForGcTest()
        {
            // (테스트용) 강제 GC
#if UNITY_EDITOR
            if (_tcgSettings.testMemoryProfile)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                Debug.Log("[GC Test] BattleContext 참조 해제 및 GC 강제 호출 완료");
            }
#endif
        }

        /// <summary>
        /// UI에서 "카드 사용" 요청이 들어왔을 때 호출.
        /// </summary>
        public void OnUiRequestPlayCard(ConfigCommonTcg.TcgPlayerSide side, TcgBattleDataCard card)
        {
            if (!_isPlayerTurn || side != ConfigCommonTcg.TcgPlayerSide.Player)
                return;

            if (_playerController is TcgBattleControllerPlayer human)
            {
                var cmd = TcgBattleCommand.PlayCard(side, card);
                human.EnqueueCommand(cmd);
            }
        }

        /// <summary>
        /// UI에서 "유닛 공격" 요청이 들어왔을 때 호출.
        /// </summary>
        public void OnUiRequestAttackUnit(
            ConfigCommonTcg.TcgPlayerSide side,
            TcgBattleDataFieldCard attacker,
            TcgBattleDataFieldCard target)
        {
            if (!_isPlayerTurn || side != ConfigCommonTcg.TcgPlayerSide.Player)
                return;

            if (_playerController is TcgBattleControllerPlayer human)
            {
                var cmd = TcgBattleCommand.AttackUnit(side, attacker, target);
                human.EnqueueCommand(cmd);
            }
        }

        public void OnUiRequestAttackHero(
            ConfigCommonTcg.TcgPlayerSide side,
            TcgBattleDataFieldCard attacker)
        {
            if (!_isPlayerTurn || side != ConfigCommonTcg.TcgPlayerSide.Player)
                return;

            if (_playerController is TcgBattleControllerPlayer human)
            {
                var cmd = TcgBattleCommand.AttackHero(side, attacker);
                human.EnqueueCommand(cmd);
            }
        }

        /// <summary>
        /// 턴 종료 버튼이 눌렸을 때 호출.
        /// - HumanController 큐를 비워 명령을 가져온 후,
        ///   ExecuteCommands 로 실제 처리.
        /// </summary>
        public void OnUiRequestEndTurn(ConfigCommonTcg.TcgPlayerSide side)
        {
            if (!_isPlayerTurn || side != ConfigCommonTcg.TcgPlayerSide.Player)
                return;

            _commandBuffer.Clear();
            _playerController.DecideTurnActions(_battleDataMain, _commandBuffer);

            // 마지막으로 EndTurn 명령이 없다면 자동 추가
            bool hasEndTurn = false;
            foreach (var cmd in _commandBuffer)
            {
                if (cmd.CommandType == ConfigCommonTcg.TcgBattleCommandType.EndTurn)
                {
                    hasEndTurn = true;
                    break;
                }
            }
            if (!hasEndTurn)
            {
                _commandBuffer.Add(TcgBattleCommand.EndTurn(side));
            }

            ExecuteCommands(_commandBuffer);
        }
        private TcgBattleDataCard DrawOneCard(TcgBattleDataSide battleDataSide)
        {
            var deck = battleDataSide.TcgBattleDataDeck;
            if (deck == null || deck.Count == 0)
            {
                // 추후: 피로 데미지 등 처리 가능
                return null;
            }

            if (!deck.TryDraw(out var card)) return null;
            
            battleDataSide.AddCardToHand(card);
            return card;
        }
        private void DrawCards(TcgBattleDataSide battleDataSide, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var card = DrawOneCard(battleDataSide);
                if (card == null)
                    break; // 덱이 비었으면 중단
            }
        }

        private void ReleaseUiReferences()
        {
            // 예시: UI 윈도우들이 _battleContext 내부의 SideState, DeckRuntime 등을
            // 들고 있다면, 여기서 null 처리 해주는 패턴
            // _uiWindowTcgFieldPlayer?.SetContext(null);
            // _uiWindowTcgFieldEnemy?.SetContext(null);
            // _uiWindowTcgHandPlayer?.SetContext(null);
            // _uiWindowTcgHandEnemy?.SetContext(null);
        }

#if UNITY_EDITOR
        /// <summary>
        /// 에디터 환경에서 셔플된 덱 구성을 로그로 출력합니다.
        /// </summary>
        private void LogShuffledDeckForDebug(TcgBattleDataDeck<TcgBattleDataCard> tcgBattleDataDeck)
        {
            if (tcgBattleDataDeck == null)
                return;

            tcgBattleDataDeck.DebugCard();
        }
#endif
    }
}
