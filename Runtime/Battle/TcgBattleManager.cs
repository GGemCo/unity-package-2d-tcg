using System;
using System.Collections.Generic;
using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 대결하기 메인 파사드 클래스.
    /// - 패키지/세이브/테이블 의존성 보유
    /// - TcgBattleDeckService, TcgBattleSession, TcgBattleUiCoordinator 를 조합하여 전투를 시작/종료/제어
    /// - UI → 전투 도메인으로의 브리지 역할
    /// </summary>
    public class TcgBattleManager
    {
        // 패키지/세이브/설정
        private TcgPackageManager _packageManager;
        private SaveDataManagerTcg _saveDataManagerTcg;
        private SystemMessageManager _systemMessageManager;
        private GGemCoTcgSettings _tcgSettings;
        private TableTcgCard _tableTcgCard;

        // 서비스들
        private TcgBattleDeckController _deckController;
        private TcgBattleUiController _uiController;
        private TcgBattleSession _session;

        // 커맨드 연출 Trace 버퍼 (GC 최소화)
        private readonly List<TcgBattleCommandTrace> _traceBuffer = new List<TcgBattleCommandTrace>(64);

        // 커맨드 핸들러
        private readonly Dictionary<ConfigCommonTcg.TcgBattleCommandType, ITcgBattleCommandHandler> _commandHandlers
            = new Dictionary<ConfigCommonTcg.TcgBattleCommandType, ITcgBattleCommandHandler>(16);

        public bool IsBattleRunning => _session != null && !_session.IsBattleEnded;

        public void Initialize(TcgPackageManager packageManager, SystemMessageManager systemMessageManager)
        {
            _packageManager = packageManager ?? throw new ArgumentNullException(nameof(packageManager));
            _saveDataManagerTcg = _packageManager.saveDataManagerTcg;
            _systemMessageManager = systemMessageManager;
            _tcgSettings = AddressableLoaderSettingsTcg.Instance.tcgSettings;
            _tableTcgCard = TableLoaderManagerTcg.Instance.TableTcgCard;

            _deckController = new TcgBattleDeckController(_saveDataManagerTcg, _tcgSettings, _tableTcgCard);
            _uiController = new TcgBattleUiController();

            InitializeDefaultCommandHandlers();
        }

        /// <summary>
        /// 전투를 시작합니다. (씬/메뉴에서 호출)
        /// </summary>
        public void StartBattle(TcgBattleMetaData metaData)
        {
            if (metaData == null)
            {
                throw new ArgumentNullException(nameof(metaData));
            }

            if (_session != null)
            {
                GcLogger.LogWarning($"[{nameof(TcgBattleManager)}] 이미 전투가 진행 중입니다. 기존 전투를 종료합니다.");
                EndBattleForce();
            }

            if (!_uiController.TrySetupWindows())
            {
                GcLogger.LogError($"[{nameof(TcgBattleManager)}] 전투 UI 윈도우 설정에 실패했습니다.");
                return;
            }
            if (metaData.playerDeckIndex < 0)
            {
                // todo. localization
                _systemMessageManager.ShowMessageError("먼저 덱을 생성하고, 카드를 추가해주세요.");
                return;
            }

            // 덱 생성
            int seed = metaData.initialSeed > 0 ? metaData.initialSeed : 0;

            var playerDeck = _deckController.BuildPlayerDeckRuntime(metaData.playerDeckIndex, seed);
            var enemyDeck  = _deckController.BuildEnemyDeckRuntime(metaData.enemyDeckPresetId, seed);

            if (playerDeck == null || enemyDeck == null)
            {
                GcLogger.LogError($"[{nameof(TcgBattleManager)}] 덱 생성에 실패했습니다.");
                return;
            }

            // 각 사이드 상태 생성. 영웅 생성
            var playerSide = new TcgBattleDataSide(ConfigCommonTcg.TcgPlayerSide.Player, playerDeck,
                _tcgSettings.countManaBattleStart, _tcgSettings.countManaBattleStart, _tcgSettings.countMaxManaInBattle);

            var enemySide = new TcgBattleDataSide(ConfigCommonTcg.TcgPlayerSide.Enemy, enemyDeck,
                _tcgSettings.countManaBattleStart, _tcgSettings.countManaBattleStart, _tcgSettings.countMaxManaInBattle);

            // 플레이어/적 컨트롤러 생성
            var playerController = new TcgBattleControllerPlayer(ConfigCommonTcg.TcgPlayerSide.Player);
            var enemyController  = new TcgBattleControllerEnemy(ConfigCommonTcg.TcgPlayerSide.Enemy, ConfigCommonTcg.TcgPlayerKind.AiEasy);

            // 세션 생성
            _session = new TcgBattleSession(
                playerSide,
                enemySide,
                _commandHandlers,
                playerController,
                enemyController,
                _tcgSettings,
                _systemMessageManager);
            
            // 첫 드로우/턴 시작 로직
            DrawCards(playerSide, _tcgSettings.startingHandCardCount);
            DrawCards(enemySide,  _tcgSettings.startingHandCardCount);
            
            // UI 바인딩 및 초기 갱신. 순서 중요. BindBattleManager 에서 Hud 윈도우를 사용하고 한다.
            _uiController.ShowAll(true);
            _uiController.BindBattleManager(this, _session, _tcgSettings);
            _uiController.RefreshAll(_session.Context);
        }

        /// <summary>
        /// 강제로 전투를 종료합니다. (씬 전환 등)
        /// </summary>
        public void EndBattleForce()
        {
            if (_session != null)
            {
                _session.ForceEnd(ConfigCommonTcg.TcgPlayerSide.None);
                _session.Dispose();
                _session = null;
            }

            if (_uiController != null)
            {
                _uiController.ShowAll(false);
                _uiController.Release();
            }
        }

        #region UI → 전투 세션 브리지

        /// <summary>
        /// UI에서 "카드 사용" 요청을 보냈을 때 호출됩니다.
        /// </summary>
        public void DrawCardToField(int indexInHand)
        {
            if (!IsBattleRunning) return;
            if (!_session.IsPlayerTurn) return;
            if (_uiController != null && _uiController.IsInteractionLocked) return;
            if (indexInHand < 0)
            {
                GcLogger.LogError($"indexInHand: {indexInHand}");
                return;
            }

            var actor = _session.Context.GetSideState(ConfigCommonTcg.TcgPlayerSide.Player);
            var battleCard = actor.GetBattleDataCardInHandByIndex(indexInHand);
            if (battleCard == null) return;
            
            var command = TcgBattleCommand.DrawCardToField(ConfigCommonTcg.TcgPlayerSide.Player, ConfigCommonTcg.TcgZone.HandPlayer, ConfigCommonTcg.TcgZone.FieldPlayer, battleCard);
            _session.ExecuteCommandWithTrace(command, _traceBuffer);
            _uiController?.PlayPresentationAndRefresh(_session.Context, _traceBuffer);
        }
        /// <summary>
        /// UI에서 "크리처로 유닛 공격" 요청을 보냈을 때 호출됩니다.
        /// Field 에서 Field 로 공격
        /// </summary>
        public void AttackUnit(ConfigCommonTcg.TcgPlayerSide side, ConfigCommonTcg.TcgZone attackerZone, int attackerIndex, ConfigCommonTcg.TcgZone targetZone, int targetIndex)
        {
            if (!IsBattleRunning) return;
            if (!_session.IsPlayerTurn) return;
            if (_uiController != null && _uiController.IsInteractionLocked) return;
            if (attackerIndex < 0)
            {
                GcLogger.LogError($"attackerIndex: {attackerIndex}");
                return;
            }
            if (targetIndex < 0)
            {
                GcLogger.LogError($"targetIndex: {targetIndex}");
                return;
            }

            var actor = _session.Context.GetSideState(side);
            var opponent = _session.Context.GetOpponentState(side);
            
            var attackerBattleDataCardInField = actor.GetBattleDataCardInFieldByIndex(attackerIndex);
            if (attackerBattleDataCardInField == null) return;
            var targetBattleDataCardInField = opponent.GetBattleDataCardInFieldByIndex(targetIndex);
            if (targetBattleDataCardInField == null) return;

            var command = TcgBattleCommand.AttackUnit(side, attackerZone, attackerBattleDataCardInField, targetZone, targetBattleDataCardInField);
            _session.ExecuteCommandWithTrace(command, _traceBuffer);
            _uiController?.PlayPresentationAndRefresh(_session.Context, _traceBuffer);
        }
        /// <summary>
        /// UI에서 "크리처로 적 영웅 공격" 요청을 보냈을 때 호출됩니다.
        /// 크리처는 피해를 받지 않습니다.
        /// </summary>
        public void AttackHero(ConfigCommonTcg.TcgPlayerSide side, ConfigCommonTcg.TcgZone attackerZone, int attackerIndex, ConfigCommonTcg.TcgZone targetZone, int targetIndex)
        {
            if (!IsBattleRunning) return;
            if (!_session.IsPlayerTurn) return;
            if (_uiController != null && _uiController.IsInteractionLocked) return;
            if (attackerIndex < 0)
            {
                GcLogger.LogError($"attackerIndex: {attackerIndex}");
                return;
            }
            if (targetZone == ConfigCommonTcg.TcgZone.None)
            {
                GcLogger.LogError($"{nameof(targetZone)}이 없습니다.");
                return;
            }

            var actor = _session.Context.GetSideState(side);
            var opponent = _session.Context.GetOpponentState(side);
            
            var attackerBattleDataCardInField = actor.GetBattleDataCardInFieldByIndex(attackerIndex);
            if (attackerBattleDataCardInField == null) return;
            var targetBattleDataCardInField = opponent.GetHeroBattleDataCardInFieldByIndex(targetIndex);
            if (targetBattleDataCardInField == null) return;
            var command = TcgBattleCommand.AttackHero(side, attackerZone, attackerBattleDataCardInField, targetZone, targetBattleDataCardInField);
            _session.ExecuteCommandWithTrace(command, _traceBuffer);
            _uiController?.PlayPresentationAndRefresh(_session.Context, _traceBuffer);
        }
        /// <summary>
        /// UI에서 "Spell 카드 사용" 요청을 보냈을 때 호출됩니다.
        /// </summary>
        public void UseCardSpell(ConfigCommonTcg.TcgPlayerSide side, int attackerIndex, ConfigCommonTcg.TcgZone targetZone, int targetIndex)
        {
            if (!IsBattleRunning) return;
            if (!_session.IsPlayerTurn) return;
            if (_uiController != null && _uiController.IsInteractionLocked) return;
            if (attackerIndex < 0)
            {
                GcLogger.LogError($"attackerIndex: {attackerIndex}");
                return;
            }
            if (targetZone == ConfigCommonTcg.TcgZone.None)
            {
                GcLogger.LogError($"{nameof(targetZone)}이 없습니다.");
                return;
            }
            if (targetIndex < 0)
            {
                GcLogger.LogError($"targetIndex: {targetIndex}");
                return;
            }

            var actor = _session.Context.GetSideState(side);
            var opponent = _session.Context.GetOpponentState(side);
            
            var attackerBattleDataCardInHand = actor.GetBattleDataCardInHandByIndex(attackerIndex);
            if (attackerBattleDataCardInHand == null) return;
            TcgBattleDataCardInField targetBattleDataCardInField = null;
            if (targetZone == ConfigCommonTcg.TcgZone.FieldPlayer)
                targetBattleDataCardInField = actor.GetBattleDataCardInFieldByIndex(targetIndex, true);
            else if (targetZone == ConfigCommonTcg.TcgZone.FieldEnemy)
                targetBattleDataCardInField = opponent.GetBattleDataCardInFieldByIndex(targetIndex, true);
            
            if (targetBattleDataCardInField == null) return;
            
            var command = TcgBattleCommand.UseCardSpell(side, ConfigCommonTcg.TcgZone.HandPlayer,
                attackerBattleDataCardInHand, targetZone, targetBattleDataCardInField);
            _session.ExecuteCommandWithTrace(command, _traceBuffer);
            _uiController?.PlayPresentationAndRefresh(_session.Context, _traceBuffer);
        }

        public void UseCardEquipment(ConfigCommonTcg.TcgPlayerSide side, int attackerIndex, ConfigCommonTcg.TcgZone targetZone, int targetIndex)
        {
            if (!IsBattleRunning) return;
            if (!_session.IsPlayerTurn) return;
            if (_uiController != null && _uiController.IsInteractionLocked) return;
            if (attackerIndex < 0)
            {
                GcLogger.LogError($"attackerIndex: {attackerIndex}");
                return;
            }
            if (targetZone == ConfigCommonTcg.TcgZone.None)
            {
                GcLogger.LogError($"{nameof(targetZone)}이 없습니다.");
                return;
            }
            if (targetIndex < 0)
            {
                GcLogger.LogError($"targetIndex: {targetIndex}");
                return;
            }

            var actor = _session.Context.GetSideState(side);
            var opponent = _session.Context.GetOpponentState(side);
            
            var attackerBattleDataCardInHand = actor.GetBattleDataCardInHandByIndex(attackerIndex);
            if (attackerBattleDataCardInHand == null) return;
            TcgBattleDataCardInField targetBattleDataCardInField = null;
            if (targetZone == ConfigCommonTcg.TcgZone.FieldPlayer)
                targetBattleDataCardInField = actor.GetBattleDataCardInFieldByIndex(targetIndex, true);
            else if (targetZone == ConfigCommonTcg.TcgZone.FieldEnemy)
                targetBattleDataCardInField = opponent.GetBattleDataCardInFieldByIndex(targetIndex, true);
            
            if (targetBattleDataCardInField == null) return;
            
            var command = TcgBattleCommand.UseCardEquipment(side, ConfigCommonTcg.TcgZone.HandPlayer,
                attackerBattleDataCardInHand, targetZone, targetBattleDataCardInField);
            _session.ExecuteCommandWithTrace(command, _traceBuffer);
            _uiController?.PlayPresentationAndRefresh(_session.Context, _traceBuffer);
        }

        public void UseCardPermanent(ConfigCommonTcg.TcgPlayerSide side, int attackerIndex)
        {
            if (!IsBattleRunning) return;
            if (!_session.IsPlayerTurn) return;
            if (_uiController != null && _uiController.IsInteractionLocked) return;
            if (attackerIndex < 0)
            {
                GcLogger.LogError($"attackerIndex: {attackerIndex}");
                return;
            }

            var actor = _session.Context.GetSideState(side);
            
            var attackerBattleDataCardInHand = actor.GetBattleDataCardInHandByIndex(attackerIndex);
            if (attackerBattleDataCardInHand == null) return;
            
            var command = TcgBattleCommand.UseCardPermanent(side, ConfigCommonTcg.TcgZone.HandPlayer, attackerBattleDataCardInHand);
            _session.ExecuteCommandWithTrace(command, _traceBuffer);
            _uiController?.PlayPresentationAndRefresh(_session.Context, _traceBuffer);
        }
        /// <summary>
        /// UI에서 "턴 종료" 버튼을 눌렀을 때 호출됩니다.
        /// </summary>
        public void OnUiRequestEndTurn()
        {
            if (!IsBattleRunning) return;
            if (_uiController != null && _uiController.IsInteractionLocked) return;
            if (_session.IsBattleEnded) return;

            // 1) Player EndTurn을 "커맨드"로 실행해서 trace/연출을 확보
            _traceBuffer.Clear();

            var cmd = TcgBattleCommand.EndTurn(ConfigCommonTcg.TcgPlayerSide.Player);
            _session.ExecuteCommandWithTrace(cmd, _traceBuffer);

            // 2) EndTurn 연출이 끝난 뒤 Enemy 턴을 실행하도록 체이닝
            _uiController?.PlayPresentationAndRefresh(_session.Context, _traceBuffer, onCompleted: () =>
            {
                if (_session == null) return;
                if (_session.IsBattleEnded) return;

                // Enemy 턴 자동 실행
                _traceBuffer.Clear();
                _session.ExecuteEnemyTurnWithTrace(_traceBuffer);
                
                _uiController?.PlayPresentationAndRefresh(_session.Context, _traceBuffer, onCompleted: () =>
                {
                    if (_session == null) return;
                    if (_session.IsBattleEnded) return;

                    // Enemy 턴 종료 + 연출 종료 이후, 턴 제한 체크
                    TryEndBattleByTurnLimitAfterEnemyTurn();
                });
            });
        }
        private void TryEndBattleByTurnLimitAfterEnemyTurn()
        {
            if (_tcgSettings == null) return;

            int maxTurns = _tcgSettings.maxTurns;
            if (maxTurns <= 0) return;

            int remain = maxTurns - _session.Context.TurnCount;
            if (remain > 0) return;

            // 정책: 턴 제한 종료 시 승패 판정(예시: HP 비교, 같으면 Draw)
            int playerHp = _session.Context.Player.Field.Hero.Health;
            int enemyHp  = _session.Context.Enemy.Field.Hero.Health;

            ConfigCommonTcg.TcgPlayerSide winner;
            if (playerHp > enemyHp) winner = ConfigCommonTcg.TcgPlayerSide.Player;
            else if (enemyHp > playerHp) winner = ConfigCommonTcg.TcgPlayerSide.Enemy;
            else winner = ConfigCommonTcg.TcgPlayerSide.Draw;

            _session.ForceEnd(winner);
            OnBattleEnded(winner);
        }

        public void OnBattleEnded(ConfigCommonTcg.TcgPlayerSide winner)
        {
            // TODO: 보상 지급, 결과 화면 표시 등 처리
            GcLogger.Log($"[{nameof(TcgBattleManager)}] 전투 종료. 승리: {winner}");

            _uiController.RefreshAll(_session.Context);
            // 여기서 바로 EndBattleForce() 를 호출할지, 
            // 결과 UI에서 나갈 때까지 세션을 유지할지 정책에 따라 결정
            
            // todo. localization
            var message = "전투가 종료되었습니다.";
            if (winner == ConfigCommonTcg.TcgPlayerSide.Player)
            {
                message += "\n승자는 플레이어 입니다.";
            }
            else if (winner == ConfigCommonTcg.TcgPlayerSide.Enemy)
            {
                message += "\n승자는 AI 입니다.";
            }
            PopupMetadata popupMetadata = new PopupMetadata
            {
                PopupType  = PopupManager.Type.Default,
                Title = "전투 종료",
                Message = message,
                MessageColor = Color.yellow,
                OnConfirm = EndBattleForce,
                IsClosableByClick = false
            };
            SceneGame.Instance.popupManager.ShowPopup(popupMetadata);
            // 안전장치: 어떤 경로든 입력 차단 해제
            _uiController?.ResetInteractionLock();
        }

        #endregion

        #region 커맨드 핸들러 등록

        private void InitializeDefaultCommandHandlers()
        {
            _commandHandlers.Clear();

            // 핸들러 구현체들
            RegisterCommandHandler(new CommandHandlerDrawCardToField());
            RegisterCommandHandler(new CommandHandlerUseCardSpell());
            RegisterCommandHandler(new CommandHandlerUseCardPermanent());
            RegisterCommandHandler(new CommandHandlerUseCardEquipment());
            RegisterCommandHandler(new CommandHandlerUseCardEvent());
            RegisterCommandHandler(new CommandHandlerAttackUnit());
            RegisterCommandHandler(new CommandHandlerAttackHero());
            RegisterCommandHandler(new CommandHandlerEndTurn());
        }

        private void RegisterCommandHandler(ITcgBattleCommandHandler handler)
        {
            if (handler == null) return;

            var type = handler.CommandType;
            if (!_commandHandlers.TryAdd(type, handler))
            {
                GcLogger.LogWarning($"[{nameof(TcgBattleManager)}] 이미 등록된 커맨드 타입입니다: {type}");
                return;
            }
        }

        #endregion
        
        private TcgBattleDataCardInHand DrawOneCard(TcgBattleDataSide battleDataSide)
        {
            var deck = battleDataSide.TcgBattleDataDeck;
            if (deck == null || deck.Count == 0)
            {
                // 추후: 피로 데미지 등 처리 가능
                return null;
            }

            if (!deck.TryDraw(out var card)) return null;
            
            battleDataSide.Hand.TryAdd(card);
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
    }
}
