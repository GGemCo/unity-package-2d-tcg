using System.Collections.Generic;
using GGemCo2DCore;
using UnityEngine; // Mathf 사용

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
        private TcgPackageManager _packageManager;
        private SaveDataManagerTcg _saveDataManagerTcg;
        private GGemCoTcgSettings _tcgSettings;

        private UIWindowTcgField _uiWindowTcgField;
        private UIWindowTcgMyHand _uiWindowTcgMyHand;

        private int _currentMana;
        private int _maxMana;
        
        // === 전투 런타임 & 컨트롤러 추가 ===
        private TcgBattleContext _battleContext;
        private ITcgPlayerController _playerController;
        private ITcgPlayerController _enemyController;

        // 턴 진행용 임시 버퍼
        private readonly List<TcgBattleCommand> _commandBuffer = new List<TcgBattleCommand>(32);

        // 현재 턴 진행 여부
        private bool _isPlayerTurn;
        /// <summary>
        /// TCG 패키지 매니저로부터 필수 의존성을 주입합니다.
        /// </summary>
        /// <param name="packageManager">TCG 패키지 매니저 인스턴스.</param>
        public void Initialize(TcgPackageManager packageManager)
        {
            _packageManager = packageManager;
            _saveDataManagerTcg = _packageManager?.saveDataManagerTcg;

            if (_packageManager == null)
            {
                GcLogger.LogError($"{nameof(TcgPackageManager)} 가 null 입니다. {nameof(TcgBattleManager)} 초기화 실패.");
            }

            if (_saveDataManagerTcg == null)
            {
                GcLogger.LogError($"{nameof(SaveDataManagerTcg)} 가 null 입니다. 저장 데이터를 사용할 수 없습니다.");
            }

            _tcgSettings = AddressableLoaderSettingsTcg.Instance.tcgSettings;
            if (_tcgSettings == null)
            {
                GcLogger.LogError($"{nameof(GGemCoTcgSettings)} 가 null 입니다.");
            }
        }

        /// <summary>
        /// 저장된 정보(기본 덱)를 기반으로 전투를 시작합니다.
        /// </summary>
        public void StartBattle()
        {
            _currentMana = 1;
            _maxMana = _tcgSettings.countMaxManaInBattle;
            
            // 1. 기본 의존성 및 저장 데이터 검증
            if (!ValidateCoreDependencies())
                return;

            var myDeckData = _saveDataManagerTcg.MyDeck;
            if (!ValidateMyDeckData(myDeckData))
                return;

            var playerDataTcg = _saveDataManagerTcg.PlayerTcg;
            if (!ValidatePlayerData(playerDataTcg))
                return;

            var deck = myDeckData.GetDeckInfoByIndex(playerDataTcg.defaultDeckIndex);
            if (!ValidateDeck(deck, playerDataTcg.defaultDeckIndex))
                return;

            // 2. 셔플 컨텍스트 생성
            var shuffleContext = BuildShuffleContext();

            // 3. 런타임 덱 생성 및 셔플
            List<CardRuntime> runtimeCardList = DeckBuilder.BuildRuntimeDeck(deck.cardList);
            var deckRuntime = new DeckRuntime<CardRuntime>(shuffleContext);
            deckRuntime.SetCards(runtimeCardList);
            deckRuntime.Shuffle();

#if UNITY_EDITOR
            // 에디터에서만 디버그 로그 출력
            LogShuffledDeckForDebug(deckRuntime);
#endif

            // 4. UI 윈도우 준비
            if (!EnsureBattleWindows())
                return;

            // 5. 윈도우에 초기 데이터 전달
            InitializeBattleWindows(deckRuntime);
            
            // 6. 전투 런타임 컨텍스트 생성 및 컨트롤러 초기화
            SetupBattleRuntime(deckRuntime);
        }

        #region Validation

        /// <summary>
        /// 필수 필드(패키지 매니저, 세이브 매니저)가 모두 유효한지 검사합니다.
        /// </summary>
        private bool ValidateCoreDependencies()
        {
            if (_packageManager == null)
            {
                GcLogger.LogError($"{nameof(TcgBattleManager)} 가 초기화되지 않았습니다. {nameof(TcgPackageManager)} 가 null 입니다.");
                return false;
            }

            if (_saveDataManagerTcg == null)
            {
                GcLogger.LogError($"{nameof(SaveDataManagerTcg)} 가 null 입니다. 저장 데이터를 사용할 수 없습니다.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// MyDeckData가 존재하고, 최소 1개 이상의 덱이 있는지 검사합니다.
        /// </summary>
        private bool ValidateMyDeckData(MyDeckData myDeckData)
        {
            if (myDeckData == null)
            {
                GcLogger.LogError($"{nameof(MyDeckData)} 클래스가 없습니다.");
                return false;
            }

            if (myDeckData.GetCurrentCount() <= 0)
            {
                GcLogger.LogError("저장된 덱 데이터가 없습니다.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 플레이어 TCG 데이터가 유효한지 검사합니다.
        /// </summary>
        private bool ValidatePlayerData(PlayerDataTcg playerDataTcg)
        {
            if (playerDataTcg == null)
            {
                GcLogger.LogError($"{nameof(PlayerDataTcg)} 클래스가 없습니다.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 기본 덱 정보와 카드 리스트가 유효한지 검사합니다.
        /// </summary>
        private bool ValidateDeck(MyDeckSaveData deck, int deckIndex)
        {
            if (deck == null)
            {
                GcLogger.LogError($"저장된 덱 정보가 없습니다. index: {deckIndex}");
            }
            else if (deck.cardList == null || deck.cardList.Count == 0)
            {
                GcLogger.LogError($"덱 안에 카드 정보가 없습니다. index: {deckIndex}");
            }
            else
            {
                return true;
            }

            return false;
        }

        #endregion

        #region Shuffle

        /// <summary>
        /// 설정값과 테스트 시드를 기반으로 셔플 메타데이터를 생성합니다.
        /// - 테스트 시드가 0보다 크면 고정 시드 사용
        /// - costWeights 설정이 있으면 Weighted 모드, 없으면 PureRandom 모드
        /// </summary>
        private ShuffleMetaData BuildShuffleContext()
        {
            // 1) 시드 설정
            var seedManager = CreateSeedManager();

            // 2) 셔플 모드 및 설정
            var shuffleMode = ConfigCommonTcg.ShuffleMode.PureRandom;
            var shuffleConfig = new ShuffleConfig();

            int[] costWeights = _tcgSettings.costWeights;

            // costWeights 에 값이 정의되어 있으면 Weighted 모드로 전환
            if (costWeights == null || costWeights.Length <= 0)
                return new ShuffleMetaData(shuffleMode, seedManager, shuffleConfig);
            
            shuffleMode = ConfigCommonTcg.ShuffleMode.Weighted;

            shuffleConfig.FrontLoadedCount = 10;

            if (shuffleConfig.CostWeights == null || shuffleConfig.CostWeights.Count <= 0)
                return new ShuffleMetaData(shuffleMode, seedManager, shuffleConfig);
            int length = Mathf.Min(costWeights.Length, shuffleConfig.CostWeights.Count);
            for (int i = 0; i < length; i++)
            {
                if (costWeights[i] <= 0) continue;
                shuffleConfig.CostWeights[i] = costWeights[i];
            }

            return new ShuffleMetaData(shuffleMode, seedManager, shuffleConfig);
        }

        /// <summary>
        /// 설정된 테스트 시드가 있으면 고정 시드를 사용하는 SeedManager를 생성합니다.
        /// </summary>
        private SeedManager CreateSeedManager()
        {
            int serverSeed = _tcgSettings.testSeed;

            if (serverSeed > 0)
                return new SeedManager(serverSeed);

            return new SeedManager();
        }

#if UNITY_EDITOR
        /// <summary>
        /// 에디터 환경에서 셔플된 덱 구성을 로그로 출력합니다.
        /// </summary>
        private void LogShuffledDeckForDebug(DeckRuntime<CardRuntime> deckRuntime)
        {
            if (deckRuntime == null)
                return;

            deckRuntime.DebugCard();
        }
#endif

        #endregion

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

            if (_uiWindowTcgField == null)
            {
                _uiWindowTcgField = windowManager.GetUIWindowByUid<UIWindowTcgField>(UIWindowConstants.WindowUid.TcgField);
            }

            if (_uiWindowTcgField == null)
            {
                GcLogger.LogError($"{nameof(UIWindowTcgField)} 윈도우가 UI 매니저에 없습니다.");
                return false;
            }

            if (_uiWindowTcgMyHand == null)
            {
                _uiWindowTcgMyHand = windowManager.GetUIWindowByUid<UIWindowTcgMyHand>(UIWindowConstants.WindowUid.TcgMyHand);
            }

            if (_uiWindowTcgMyHand == null)
            {
                GcLogger.LogError($"{nameof(UIWindowTcgMyHand)} 윈도우가 UI 매니저에 없습니다.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// UI 윈도우를 표시하고, 초기 패 정보를 전달합니다.
        /// </summary>
        private void InitializeBattleWindows(DeckRuntime<CardRuntime> deckRuntime)
        {
            // 필드 윈도우 표시
            _uiWindowTcgField.Show(true);

            // 내 패 윈도우에 덱 런타임 전달 (첫 패 등 초기화)
            _uiWindowTcgMyHand.SetFirstCard(deckRuntime);
        }

        #endregion
        
        /// <summary>
        /// 플레이어/적 측 런타임 상태와 전투 컨텍스트를 생성하고,
        /// 사람/AI 컨트롤러를 초기화합니다.
        /// </summary>
        private void SetupBattleRuntime(DeckRuntime<CardRuntime> playerDeckRuntime)
        {
            // TODO: 실제 프로젝트에서는 적 덱도 SaveData/테이블에서 불러오기
            var enemyDeckRuntime = BuildEnemyDeckRuntime();

            var playerSide = new TcgBattleSideState(
                ConfigCommonTcg.TcgPlayerSide.Player,
                playerDeckRuntime)
            {
                // todo. 정리 필요
                HeroHp = 100, //_tcgSettings.defaultHeroHp,
                HeroHpMax = 100, //_tcgSettings.defaultHeroHp,
                CurrentMana = 1,
                MaxMana = _tcgSettings.countMaxManaInBattle
            };

            var enemySide = new TcgBattleSideState(
                ConfigCommonTcg.TcgPlayerSide.Enemy,
                enemyDeckRuntime)
            {
                // todo. 정리 필요
                HeroHp = 100, //_tcgSettings.defaultHeroHp,
                HeroHpMax = 100, //_tcgSettings.defaultHeroHp,
                CurrentMana = 1,
                MaxMana = _tcgSettings.countMaxManaInBattle
            };

            _battleContext = new TcgBattleContext(
                _tcgSettings,
                this,
                playerSide,
                enemySide)
            {
                TurnNumber = 1,
                ActiveSide = ConfigCommonTcg.TcgPlayerSide.Player
            };

            // 컨트롤러 생성
            var human = new TcgHumanController(ConfigCommonTcg.TcgPlayerSide.Player);
            var ai    = new TcgAiController(ConfigCommonTcg.TcgPlayerSide.Enemy, ConfigCommonTcg.TcgPlayerKind.AiEasy);

            _playerController = human;
            _enemyController  = ai;

            _playerController.Initialize(_battleContext);
            _enemyController.Initialize(_battleContext);

            // UI에 BattleManager / 플레이어 상태 연결
            InitializeUiForBattle();
        }
        private void InitializeUiForBattle()
        {
            // MyHand/Field에 전투 관련 정보 전달
            _uiWindowTcgMyHand.SetBattleManager(this, ConfigCommonTcg.TcgPlayerSide.Player);
            _uiWindowTcgField.SetBattleManager(this);

            // 처음에는 플레이어의 손패/필드 상황을 표시
            var playerState  = _battleContext.Player;
            var enemyState   = _battleContext.Enemy;

            _uiWindowTcgMyHand.RefreshHand(playerState.ReadOnlyHand);
            _uiWindowTcgField.RefreshBoard(playerState, enemyState);

            // 플레이어 턴 시작
            _isPlayerTurn = true;
            _battleContext.ActiveSide = ConfigCommonTcg.TcgPlayerSide.Player;
        }

        /// <summary>
        /// AI용 덱을 구성하는 부분.
        /// 현재 구조에서는 임시로 플레이어 덱을 복사하거나,
        /// 별도의 세이브/테이블을 통해 생성하도록 확장할 수 있습니다.
        /// </summary>
        private DeckRuntime<CardRuntime> BuildEnemyDeckRuntime()
        {
            // TODO: 실제 구현에서는 적 덱 데이터를 받아서
            //       DeckBuilder.BuildRuntimeDeck(...) 으로 생성.
            var enemyDeckList = new List<CardRuntime>(); // 임시
            var shuffleContext = BuildShuffleContext();

            var enemyDeckRuntime = new DeckRuntime<CardRuntime>(shuffleContext);
            enemyDeckRuntime.SetCards(enemyDeckList);
            enemyDeckRuntime.Shuffle();
            return enemyDeckRuntime;
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
        private void ExecuteCommand(TcgBattleCommand cmd)
        {
            if (cmd == null)
                return;

            var actor    = _battleContext.GetSideState(cmd.Side);
            var opponent = _battleContext.GetOpponentState(cmd.Side);

            switch (cmd.CommandType)
            {
                case ConfigCommonTcg.TcgBattleCommandType.PlayCardFromHand:
                    ExecutePlayCard(actor, opponent, cmd);
                    break;

                case ConfigCommonTcg.TcgBattleCommandType.AttackUnit:
                    ExecuteAttackUnit(actor, opponent, cmd);
                    break;

                case ConfigCommonTcg.TcgBattleCommandType.AttackHero:
                    ExecuteAttackHero(actor, opponent, cmd);
                    break;

                case ConfigCommonTcg.TcgBattleCommandType.EndTurn:
                    ExecuteEndTurn(cmd.Side);
                    break;
            }

            // 명령 실행 후 UI 리프레시
            _uiWindowTcgMyHand.RefreshHand(actor.ReadOnlyHand);
            _uiWindowTcgField.RefreshBoard(
                _battleContext.Player,
                _battleContext.Enemy);
        }
        private void ExecutePlayCard(
            TcgBattleSideState actor,
            TcgBattleSideState opponent,
            TcgBattleCommand cmd)
        {
            var card = cmd.Card;
            if (card == null)
                return;

            if (!actor.Hand.Contains(card))
            {
                GcLogger.LogWarning("[Battle] ExecutePlayCard: Hand does not contain card.");
                return;
            }

            if (actor.CurrentMana < card.Cost)
            {
                GcLogger.LogWarning("[Battle] ExecutePlayCard: Not enough mana.");
                return;
            }

            // 마나 차감 + 손에서 제거
            actor.CurrentMana -= card.Cost;
            actor.Hand.Remove(card);

            // 카드 타입에 따라 분기 (예시)
            switch (card.Type)
            {
                case CardConstants.Type.Creature:
                {
                    // 1) 유닛 런타임 생성
                    var unit = CreateUnitFromCard(actor.Side, card);
                    if (unit == null)
                        return;

                    actor.Board.Add(unit);

                    // 2) "소환 시 발동" 이펙트가 있다면 실행
                    if (card.SummonEffects != null && card.SummonEffects.Count > 0)
                    {
                        EffectRunner.RunEffects(
                            _battleContext,
                            actor,
                            opponent,
                            card,
                            card.SummonEffects,
                            explicitTargetUnit: null /* 필요 시 타겟 전달 */);
                    }
                    break;
                }

                case CardConstants.Type.Spell:
                {
                    // 스펠은 필드에 남지 않고, 이펙트만 실행
                    if (card.SpellEffects != null && card.SpellEffects.Count > 0)
                    {
                        // TODO: TargetType 에 따라 타겟 선택 로직 추가
                        EffectRunner.RunEffects(
                            _battleContext,
                            actor,
                            opponent,
                            card,
                            card.SpellEffects,
                            explicitTargetUnit: null);
                    }
                    break;
                }

                default:
                {
                    // 기타 타입(장비/영속물 등)은 추후 확장
                    break;
                }
            }
        }

        private void ExecuteAttackUnit(
            TcgBattleSideState actor,
            TcgBattleSideState opponent,
            TcgBattleCommand cmd)
        {
            var attacker = cmd.Attacker;
            var target   = cmd.TargetUnit;

            if (attacker == null || target == null)
                return;

            if (!actor.Board.Contains(attacker))
                return;

            if (!opponent.Board.Contains(target))
                return;

            if (!attacker.CanAttack)
                return;

            // 양쪽에 데미지 적용
            target.ModifyAttack(-attacker.Attack);
            attacker.ModifyAttack(-target.Attack);

            attacker.CanAttack = false;

            // 사망 처리
            if (target.Hp <= 0)
                opponent.Board.Remove(target);

            if (attacker.Hp <= 0)
                actor.Board.Remove(attacker);
        }
        private void ExecuteAttackHero(
            TcgBattleSideState actor,
            TcgBattleSideState opponent,
            TcgBattleCommand cmd)
        {
            var attacker = cmd.Attacker;
            if (attacker == null)
                return;

            if (!actor.Board.Contains(attacker))
                return;

            if (!attacker.CanAttack)
                return;

            opponent.HeroHp -= attacker.Attack;
            attacker.CanAttack = false;

            // TODO: 영웅 HP 0 이하이면 전투 종료 처리
            if (opponent.HeroHp <= 0)
            {
                OnBattleEnd(actor.Side);
            }
        }

        private void OnBattleEnd(ConfigCommonTcg.TcgPlayerSide actorSide)
        {
        }

        private void ExecuteEndTurn(ConfigCommonTcg.TcgPlayerSide side)
        {
            // 턴 주인이 맞는지 검증
            if (_battleContext.ActiveSide != side)
                return;

            // 턴 수 증가/액티브 변경
            if (side == ConfigCommonTcg.TcgPlayerSide.Player)
            {
                _battleContext.ActiveSide = ConfigCommonTcg.TcgPlayerSide.Enemy;
                _isPlayerTurn = false;
            }
            else
            {
                _battleContext.ActiveSide = ConfigCommonTcg.TcgPlayerSide.Player;
                _isPlayerTurn = true;
                _battleContext.TurnNumber++;
            }

            // 새 턴 시작 시 마나/공격 가능 여부 리셋
            var newActor = _battleContext.GetSideState(_battleContext.ActiveSide);
            newActor.MaxMana = Mathf.Min(
                newActor.MaxMana + 1,
                _tcgSettings.countMaxManaInBattle);
            newActor.CurrentMana = newActor.MaxMana;

            foreach (var unit in newActor.Board)
            {
                unit.CanAttack = true;
            }

            // UI 토글 (내 턴에만 조작 가능하도록)
            _uiWindowTcgMyHand.SetInteractable(_battleContext.ActiveSide == ConfigCommonTcg.TcgPlayerSide.Player);

            // AI 턴이면 바로 AI 명령 결정/실행
            if (_battleContext.ActiveSide == ConfigCommonTcg.TcgPlayerSide.Enemy)
            {
                RunAiTurn();
            }
        }
        private void RunAiTurn()
        {
            _commandBuffer.Clear();
            _enemyController.DecideTurnActions(_battleContext, _commandBuffer);
            ExecuteCommands(_commandBuffer);
        }
        /// <summary>
        /// UI에서 "카드 사용" 요청이 들어왔을 때 호출.
        /// </summary>
        public void OnUiRequestPlayCard(ConfigCommonTcg.TcgPlayerSide side, CardRuntime card)
        {
            if (!_isPlayerTurn || side != ConfigCommonTcg.TcgPlayerSide.Player)
                return;

            if (_playerController is TcgHumanController human)
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
            TcgUnitRuntime attacker,
            TcgUnitRuntime target)
        {
            if (!_isPlayerTurn || side != ConfigCommonTcg.TcgPlayerSide.Player)
                return;

            if (_playerController is TcgHumanController human)
            {
                var cmd = TcgBattleCommand.AttackUnit(side, attacker, target);
                human.EnqueueCommand(cmd);
            }
        }

        public void OnUiRequestAttackHero(
            ConfigCommonTcg.TcgPlayerSide side,
            TcgUnitRuntime attacker)
        {
            if (!_isPlayerTurn || side != ConfigCommonTcg.TcgPlayerSide.Player)
                return;

            if (_playerController is TcgHumanController human)
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
            _playerController.DecideTurnActions(_battleContext, _commandBuffer);

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
        /// <summary>
        /// Creature 타입 카드를 기반으로 필드에 소환할 유닛 런타임을 생성합니다.
        /// - 실제 스탯/키워드는 카드 테이블/런타임에서 가져와야 합니다.
        /// </summary>
        private TcgUnitRuntime CreateUnitFromCard(
            ConfigCommonTcg.TcgPlayerSide ownerSide,
            CardRuntime cardRuntime)
        {
            if (cardRuntime == null)
            {
                GcLogger.LogError("[Battle] CreateUnitFromCard: cardRuntime is null.");
                return null;
            }

            // 1) CardRuntime 에서 스탯/키워드 정보 가져오기
            //    (아래는 예시. 실제 필드 이름에 맞게 수정 필요)
            int attack = cardRuntime.Attack; // 예: CardRuntime.Attack
            int hp     = cardRuntime.Health; // 예: CardRuntime.Health

            // 키워드 예시: CardRuntime.Keywords 또는 테이블에서 변환
            List<ConfigCommonTcg.TcgKeyword> keywords = new List<ConfigCommonTcg.TcgKeyword>(4);
            foreach (var kw in cardRuntime.Keywords) // 예: IEnumerable<TcgKeyword>
            {
                keywords.Add(kw);
            }

            // 2) 유닛 런타임 생성
            var unit = new TcgUnitRuntime(
                cardRuntime.Uid,
                ownerSide,
                cardRuntime,
                attack,
                hp,
                keywords);

            // 소환 시점에는 공격 불가 (돌진 키워드가 있으면 예외)
            if (unit.HasKeyword(ConfigCommonTcg.TcgKeyword.Rush))
                unit.CanAttack = true;
            else
                unit.CanAttack = false;

            return unit;
        }
    }
}
