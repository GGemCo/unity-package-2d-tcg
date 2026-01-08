using System;
using System.Collections.Generic;
using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// “대결하기” 전투의 시작/종료/진행을 총괄하는 메인 파사드(Facade) 클래스입니다.
    /// </summary>
    /// <remarks>
    /// 역할:
    /// - 패키지/세이브/테이블/설정 의존성을 보유하고 초기화합니다.
    /// - 덱 생성(<see cref="TcgBattleDeckController"/>), 전투 세션(<see cref="TcgBattleSession"/>),
    ///   전투 UI(<see cref="TcgBattleUiController"/>)를 조합하여 전투 흐름을 제어합니다.
    /// - UI 입력을 전투 도메인 커맨드로 변환하여 세션에 전달하는 브리지 역할을 수행합니다.
    /// </remarks>
    public class TcgBattleManager
    {
        // 패키지/세이브/설정
        private TcgPackageManager _packageManager;
        private SaveDataManagerTcg _saveDataManagerTcg;
        private SystemMessageManager _systemMessageManager;
        private GGemCoTcgSettings _tcgSettings;
        private GGemCoTcgUICutsceneSettings _uiCutsceneSettings;
        private TableTcgCard _tableTcgCard;

        // 서비스들
        private TcgBattleDeckController _deckController;
        private TcgBattleUiController _uiController;
        private TcgBattleSession _session;

        /// <summary>
        /// 커맨드 실행 결과를 담는 Trace 버퍼입니다(GC 최소화를 위해 재사용).
        /// </summary>
        private readonly List<TcgBattleCommandTrace> _traceBuffer = new List<TcgBattleCommandTrace>(64);

        /// <summary>
        /// 커맨드 타입 → 커맨드 핸들러 매핑 테이블입니다.
        /// </summary>
        private readonly Dictionary<ConfigCommonTcg.TcgBattleCommandType, ITcgBattleCommandHandler> _commandHandlers
            = new Dictionary<ConfigCommonTcg.TcgBattleCommandType, ITcgBattleCommandHandler>(16);

        /// <summary>
        /// 전투가 진행 중인지 여부입니다(세션이 존재하고 전투가 종료되지 않은 상태).
        /// </summary>
        public bool IsBattleRunning => _session != null && !_session.IsBattleEnded;

        // localization
        private readonly Dictionary<ConfigCommonTcg.TcgPlayerSide, string> _messageForWinner =
            new Dictionary<ConfigCommonTcg.TcgPlayerSide, string>(2);

        /// <summary>
        /// 전투 매니저를 초기화하고, 필요한 의존성과 기본 커맨드 핸들러를 준비합니다.
        /// </summary>
        /// <param name="packageManager">TCG 패키지/세이브/테이블 접근을 제공하는 패키지 매니저입니다.</param>
        /// <param name="systemMessageManager">시스템 메시지(UI 알림) 표시 매니저입니다.</param>
        /// <exception cref="ArgumentNullException"><paramref name="packageManager"/>가 null이면 발생합니다.</exception>
        public void Initialize(TcgPackageManager packageManager, SystemMessageManager systemMessageManager)
        {
            _packageManager = packageManager ?? throw new ArgumentNullException(nameof(packageManager));
            _saveDataManagerTcg = _packageManager.saveDataManagerTcg;
            _systemMessageManager = systemMessageManager;

            _tcgSettings = AddressableLoaderSettingsTcg.Instance.tcgSettings;
            _uiCutsceneSettings = AddressableLoaderSettingsTcg.Instance.uiCutsceneSettings;
            _tableTcgCard = TableLoaderManagerTcg.Instance.TableTcgCard;

            _deckController = new TcgBattleDeckController(_saveDataManagerTcg, _tcgSettings, _tableTcgCard);
            _uiController = new TcgBattleUiController();

            InitializeDefaultCommandHandlers();

            var message = LocalizationManager.Instance.GetSystemByKey("System_Tcg_BattleEnded");
            _messageForWinner.Add(
                ConfigCommonTcg.TcgPlayerSide.Player,
                $"{message}\n{LocalizationManager.Instance.GetSystemByKey("System_Tcg_WinnerPlayer")}");
            _messageForWinner.Add(
                ConfigCommonTcg.TcgPlayerSide.Enemy,
                $"{message}\n{LocalizationManager.Instance.GetSystemByKey("System_Tcg_WinnerAi")}");
        }

        /// <summary>
        /// 전투를 시작합니다(씬/메뉴에서 호출).
        /// </summary>
        /// <param name="metaData">전투 시작에 필요한 메타데이터(덱 인덱스/프리셋/시드 등)입니다.</param>
        /// <exception cref="ArgumentNullException"><paramref name="metaData"/>가 null이면 발생합니다.</exception>
        /// <remarks>
        /// 처리 순서:
        /// - 기존 전투가 있으면 강제 종료
        /// - UI 윈도우 확보
        /// - 덱 런타임 생성(플레이어/적) 및 세션 생성
        /// - 시작 드로우 수행 후 UI를 바인딩하고 초기 상태로 갱신
        /// </remarks>
        public void StartBattle(TcgBattleMetaData metaData)
        {
            if (metaData == null)
                throw new ArgumentNullException(nameof(metaData));

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
                _systemMessageManager.ShowMessageError("System_Tcg_CreateDeckFirst");
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

            // 각 사이드 상태 생성(영웅 포함)
            var playerSide = new TcgBattleDataSide(
                ConfigCommonTcg.TcgPlayerSide.Player,
                playerDeck,
                _tcgSettings.countManaBattleStart,
                _tcgSettings.countManaBattleStart,
                _tcgSettings.countMaxManaInBattle);

            var enemySide = new TcgBattleDataSide(
                ConfigCommonTcg.TcgPlayerSide.Enemy,
                enemyDeck,
                _tcgSettings.countManaBattleStart,
                _tcgSettings.countManaBattleStart,
                _tcgSettings.countMaxManaInBattle);

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

            // UI 바인딩 및 초기 갱신(순서 중요: BindBattleManager 내부에서 HUD 윈도우를 사용)
            _uiController.ShowAll(true);
            _uiController.BindBattleManager(this, _session, _tcgSettings, _uiCutsceneSettings);
            _uiController.RefreshAll(_session.Context);
        }

        /// <summary>
        /// 씬 전환 등 외부 요인으로 전투를 즉시 종료해야 할 때 강제로 전투를 종료합니다.
        /// </summary>
        /// <remarks>
        /// 세션을 종료/정리하고, UI를 숨긴 뒤 컨트롤러가 보유한 참조를 해제합니다.
        /// </remarks>
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
        /// UI에서 “카드 사용(손패 → 필드 배치)” 요청을 보냈을 때 호출됩니다.
        /// </summary>
        /// <param name="indexInHand">손패 내 카드 인덱스입니다.</param>
        /// <remarks>
        /// 플레이어 턴이 아니거나 입력이 잠긴 상태라면 요청을 무시합니다.
        /// </remarks>
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

            var command = TcgBattleCommand.DrawCardToField(
                ConfigCommonTcg.TcgPlayerSide.Player,
                ConfigCommonTcg.TcgZone.HandPlayer,
                ConfigCommonTcg.TcgZone.FieldPlayer,
                battleCard);

            _session.ExecuteCommandWithTrace(command, _traceBuffer);
            _uiController?.PlayPresentationAndRefresh(_session.Context, _traceBuffer);
        }

        /// <summary>
        /// UI에서 “크리처로 유닛 공격” 요청을 보냈을 때 호출됩니다(필드 → 필드 공격).
        /// </summary>
        /// <param name="side">공격 주체 사이드입니다.</param>
        /// <param name="attackerZone">공격자 존입니다.</param>
        /// <param name="attackerIndex">공격자 필드 인덱스입니다.</param>
        /// <param name="targetZone">타겟 존입니다.</param>
        /// <param name="targetIndex">타겟 필드 인덱스입니다.</param>
        public void AttackUnit(
            ConfigCommonTcg.TcgPlayerSide side,
            ConfigCommonTcg.TcgZone attackerZone,
            int attackerIndex,
            ConfigCommonTcg.TcgZone targetZone,
            int targetIndex)
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

            var command = TcgBattleCommand.AttackUnit(
                side,
                attackerZone,
                attackerBattleDataCardInField,
                targetZone,
                targetBattleDataCardInField);

            _session.ExecuteCommandWithTrace(command, _traceBuffer);
            _uiController?.PlayPresentationAndRefresh(_session.Context, _traceBuffer);
        }

        /// <summary>
        /// UI에서 “크리처로 적 영웅 공격” 요청을 보냈을 때 호출됩니다.
        /// </summary>
        /// <remarks>
        /// 현재 규칙상 공격 크리처는 피해를 받지 않습니다.
        /// </remarks>
        /// <param name="side">공격 주체 사이드입니다.</param>
        /// <param name="attackerZone">공격자 존입니다.</param>
        /// <param name="attackerIndex">공격자 필드 인덱스입니다.</param>
        /// <param name="targetZone">타겟 존(영웅이 속한 존)입니다.</param>
        /// <param name="targetIndex">타겟(영웅) 인덱스입니다.</param>
        public void AttackHero(
            ConfigCommonTcg.TcgPlayerSide side,
            ConfigCommonTcg.TcgZone attackerZone,
            int attackerIndex,
            ConfigCommonTcg.TcgZone targetZone,
            int targetIndex)
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

            var command = TcgBattleCommand.AttackHero(
                side,
                attackerZone,
                attackerBattleDataCardInField,
                targetZone,
                targetBattleDataCardInField);

            _session.ExecuteCommandWithTrace(command, _traceBuffer);
            _uiController?.PlayPresentationAndRefresh(_session.Context, _traceBuffer);
        }

        /// <summary>
        /// UI에서 “스펠(Spell) 카드 사용” 요청을 보냈을 때 호출됩니다.
        /// </summary>
        /// <param name="side">사용자 사이드입니다.</param>
        /// <param name="attackerIndex">손패 내 스펠 카드 인덱스입니다.</param>
        /// <param name="targetZone">대상 존입니다.</param>
        /// <param name="targetIndex">대상 인덱스입니다.</param>
        public void UseCardSpell(
            ConfigCommonTcg.TcgPlayerSide side,
            int attackerIndex,
            ConfigCommonTcg.TcgZone targetZone,
            int targetIndex)
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

            var command = TcgBattleCommand.UseCardSpell(
                side,
                ConfigCommonTcg.TcgZone.HandPlayer,
                attackerBattleDataCardInHand,
                targetZone,
                targetBattleDataCardInField);

            _session.ExecuteCommandWithTrace(command, _traceBuffer);
            _uiController?.PlayPresentationAndRefresh(_session.Context, _traceBuffer);
        }

        /// <summary>
        /// UI에서 “장비(Equipment) 카드 사용” 요청을 보냈을 때 호출됩니다.
        /// </summary>
        /// <param name="side">사용자 사이드입니다.</param>
        /// <param name="attackerIndex">손패 내 장비 카드 인덱스입니다.</param>
        /// <param name="targetZone">대상 존입니다.</param>
        /// <param name="targetIndex">대상 인덱스입니다.</param>
        public void UseCardEquipment(
            ConfigCommonTcg.TcgPlayerSide side,
            int attackerIndex,
            ConfigCommonTcg.TcgZone targetZone,
            int targetIndex)
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

            var command = TcgBattleCommand.UseCardEquipment(
                side,
                ConfigCommonTcg.TcgZone.HandPlayer,
                attackerBattleDataCardInHand,
                targetZone,
                targetBattleDataCardInField);

            _session.ExecuteCommandWithTrace(command, _traceBuffer);
            _uiController?.PlayPresentationAndRefresh(_session.Context, _traceBuffer);
        }

        /// <summary>
        /// UI에서 “퍼머넌트(Permanent) 카드 사용” 요청을 보냈을 때 호출됩니다(대상 지정 없음).
        /// </summary>
        /// <param name="side">사용자 사이드입니다.</param>
        /// <param name="attackerIndex">손패 내 퍼머넌트 카드 인덱스입니다.</param>
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

            var command = TcgBattleCommand.UseCardPermanent(
                side,
                ConfigCommonTcg.TcgZone.HandPlayer,
                attackerBattleDataCardInHand);

            _session.ExecuteCommandWithTrace(command, _traceBuffer);
            _uiController?.PlayPresentationAndRefresh(_session.Context, _traceBuffer);
        }

        /// <summary>
        /// UI에서 “턴 종료” 버튼을 눌렀을 때 호출됩니다.
        /// </summary>
        /// <remarks>
        /// EndTurn 커맨드 연출 종료 후 Enemy 턴을 자동 실행하며,
        /// Enemy 턴 연출 종료 뒤 턴 제한 정책에 따라 전투 종료를 판단할 수 있습니다.
        /// </remarks>
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

        /// <summary>
        /// Enemy 턴까지 끝난 시점에 턴 제한 정책을 적용하여 전투 종료 여부를 판단합니다.
        /// </summary>
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

        /// <summary>
        /// 전투 종료 시점에 호출되어 결과 처리와 UI 정리를 수행합니다.
        /// </summary>
        /// <param name="winner">승리한 사이드(무승부 포함)입니다.</param>
        /// <remarks>
        /// 현재 구현은 결과 팝업을 표시하고, 확인 시 <see cref="EndBattleForce"/>로 정리합니다.
        /// </remarks>
        public void OnBattleEnded(ConfigCommonTcg.TcgPlayerSide winner)
        {
            // TODO: 보상 지급, 결과 화면 표시 등 처리
            GcLogger.Log($"[{nameof(TcgBattleManager)}] 전투 종료. 승리: {winner}");

            _uiController.RefreshAll(_session.Context);
            // 여기서 바로 EndBattleForce() 를 호출할지,
            // 결과 UI에서 나갈 때까지 세션을 유지할지 정책에 따라 결정

            PopupMetadata popupMetadata = new PopupMetadata
            {
                PopupType  = PopupManager.Type.Default,
                Title = "System_Tcg_BattleEndTitle",
                Message = _messageForWinner.GetValueOrDefault(winner, ""),
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

        /// <summary>
        /// 기본 커맨드 핸들러들을 등록합니다.
        /// </summary>
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

        /// <summary>
        /// 지정한 커맨드 핸들러를 등록합니다.
        /// </summary>
        /// <param name="handler">등록할 핸들러입니다.</param>
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

        /// <summary>
        /// 덱에서 카드 1장을 뽑아 손패에 추가합니다.
        /// </summary>
        /// <param name="battleDataSide">카드를 뽑을 사이드 상태입니다.</param>
        /// <returns>뽑은 카드이며, 덱이 비었거나 실패하면 null입니다.</returns>
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

        /// <summary>
        /// 덱에서 지정한 수만큼 카드를 뽑아 손패에 추가합니다.
        /// </summary>
        /// <param name="battleDataSide">카드를 뽑을 사이드 상태입니다.</param>
        /// <param name="count">뽑을 카드 수입니다.</param>
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
