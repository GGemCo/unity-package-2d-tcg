using System;
using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 한 번의 전투 진행(턴, 커맨드 실행, 승패 판정, 트리거 처리)을 담당하는 도메인 클래스.
    /// UI/MonoBehaviour/Scene 에 의존하지 않도록 설계합니다.
    /// </summary>
    public sealed class TcgBattleSession : IDisposable
    {
        /// <summary>
        /// Ability 실행 시점(Begin/End)을 UI 레이어에 알리기 위한 이벤트.
        /// UI는 이 이벤트를 구독하여 <see cref="TcgAbilityType"/>별 연출을 재생할 수 있습니다.
        /// </summary>
        public event Action<TcgAbilityPresentationEvent> AbilityPresentation;

        public TcgBattleDataMain Context { get; }

        public bool IsPlayerTurn => Context.ActiveSide == ConfigCommonTcg.TcgPlayerSide.Player;

        public bool IsBattleEnded { get; private set; }

        private ConfigCommonTcg.TcgPlayerSide Winner { get; set; }

        private readonly Dictionary<ConfigCommonTcg.TcgBattleCommandType, ITcgBattleCommandHandler> _commandHandlers;
        private readonly ITcgPlayerController _playerController;
        private readonly ITcgPlayerController _enemyController;

        private readonly List<TcgBattleCommand> _commandBuffer = new List<TcgBattleCommand>(32);
        private readonly List<TcgBattleCommand> _executionQueue = new List<TcgBattleCommand>(64);

        private readonly GGemCoTcgSettings _tcgSettings;
        private readonly SystemMessageManager _systemMessageManager;

        // CardDrawn 이벤트 구독 해제용
        private Action<TcgBattleDataCard> _onPlayerCardDrawn;
        private Action<TcgBattleDataCard> _onEnemyCardDrawn;

        // (note) AbilityPresentation 이벤트 발행은 아래 PublishAbilityPresentation()을 통해 일괄 처리합니다.

        public TcgBattleSession(
            TcgBattleDataSide playerSide,
            TcgBattleDataSide enemySide,
            Dictionary<ConfigCommonTcg.TcgBattleCommandType, ITcgBattleCommandHandler> commandHandlers,
            ITcgPlayerController playerController,
            ITcgPlayerController enemyController,
            GGemCoTcgSettings settings,
            SystemMessageManager systemMessageManager)
        {
            _commandHandlers = commandHandlers ?? throw new ArgumentNullException(nameof(commandHandlers));
            _playerController = playerController ?? throw new ArgumentNullException(nameof(playerController));
            _enemyController = enemyController ?? throw new ArgumentNullException(nameof(enemyController));
            _tcgSettings = settings ?? throw new ArgumentNullException(nameof(settings));
            _systemMessageManager = systemMessageManager ?? throw new ArgumentNullException(nameof(systemMessageManager));

            Context = new TcgBattleDataMain(this, playerSide, enemySide, systemMessageManager)
            {
                ActiveSide = ConfigCommonTcg.TcgPlayerSide.Player
            };

            IsBattleEnded = false;
            Winner = ConfigCommonTcg.TcgPlayerSide.None;

            // 컨트롤러 초기화
            _playerController.Initialize(Context);
            _enemyController.Initialize(Context);

            // 드로우 트리거 구독
            SubscribeDrawEvents();
        }

        private void SubscribeDrawEvents()
        {
            // 플레이어
            _onPlayerCardDrawn = card => OnSideCardDrawn(Context.Player, card);
            Context.Player.CardDrawn += _onPlayerCardDrawn;

            // 적
            _onEnemyCardDrawn = card => OnSideCardDrawn(Context.Enemy, card);
            Context.Enemy.CardDrawn += _onEnemyCardDrawn;
        }

        private void UnsubscribeDrawEvents()
        {
            if (Context?.Player != null && _onPlayerCardDrawn != null)
                Context.Player.CardDrawn -= _onPlayerCardDrawn;

            if (Context?.Enemy != null && _onEnemyCardDrawn != null)
                Context.Enemy.CardDrawn -= _onEnemyCardDrawn;

            _onPlayerCardDrawn = null;
            _onEnemyCardDrawn = null;
        }

        #region Turn Flow

        public void ExecuteEnemyTurnWithTrace(List<TcgBattleCommandTrace> traces)
        {
            if (IsBattleEnded) return;
            if (IsPlayerTurn) return;

            _commandBuffer.Clear();
            _enemyController.DecideTurnActions(Context, _commandBuffer);
            ExecuteCommandsWithTrace(_commandBuffer, traces);
        }

        public void EndTurn()
        {
            if (IsBattleEnded) return;

            // 1) End-of-Turn 트리거
            ResolveEndOfTurnEffects();

            // 2) 턴 수 증가
            Context.TurnCount++;

            // 3) ActiveSide 전환
            Context.ActiveSide =
                (Context.ActiveSide == ConfigCommonTcg.TcgPlayerSide.Player)
                    ? ConfigCommonTcg.TcgPlayerSide.Enemy
                    : ConfigCommonTcg.TcgPlayerSide.Player;

            // 4) (선택) 공격 가능 상태 초기화
            if (Context.ActiveSide == ConfigCommonTcg.TcgPlayerSide.Player)
            {
                Context.Player.SetBoardCardCanAttack(true);
                Context.Enemy.SetBoardCardCanAttack(true);
            }

            // 5) 마나 증가/회복 + 턴 시작 드로우
            IncreaseMaxManaByTurnOff();
            DrawStartOfTurnCard();

            // 6) Start-of-Turn 트리거
            ResolveStartOfTurnEffects();
        }

        private void ResolveEndOfTurnEffects()
        {
            if (IsBattleEnded) return;

            var activeSide = Context.GetSideState(Context.ActiveSide);

            // Permanent/Event 등 턴 종료 트리거 처리
            ResolveTriggersForSide(activeSide, TcgAbilityConstants.TcgAbilityTriggerType.OnTurnEnd, sourceCard: null);
        }

        private void ResolveStartOfTurnEffects()
        {
            if (IsBattleEnded) return;

            var activeSide = Context.GetSideState(Context.ActiveSide);

            // Permanent/Event 등 턴 시작 트리거 처리
            ResolveTriggersForSide(activeSide, TcgAbilityConstants.TcgAbilityTriggerType.OnTurnStart, sourceCard: null);
        }

        private void DrawStartOfTurnCard()
        {
            var side = Context.GetSideState(Context.ActiveSide);
            side.DrawOneCard();
            // DrawOneCard() 내부에서 CardDrawn 이벤트가 발생하고,
            // 그 이벤트를 세션이 받아 OnDraw 트리거를 처리합니다.
        }

        private void IncreaseMaxManaByTurnOff()
        {
            if (Context.ActiveSide != ConfigCommonTcg.TcgPlayerSide.Player)
                return;

            var playerSide = Context.GetSideState(ConfigCommonTcg.TcgPlayerSide.Player);
            playerSide.IncreaseMaxMana(_tcgSettings.countManaAfterTurn, _tcgSettings.countMaxManaInBattle);
            playerSide.RestoreManaFull();

            var enemySide = Context.GetSideState(ConfigCommonTcg.TcgPlayerSide.Enemy);
            enemySide.IncreaseMaxMana(_tcgSettings.countManaAfterTurn, _tcgSettings.countMaxManaInBattle);
            enemySide.RestoreManaFull();
        }

        #endregion

        #region Trigger Loop (Requested)

        /// <summary>
        /// 특정 Side가 카드를 드로우했을 때(손패에 들어갔을 때) 호출됩니다.
        /// - Permanent/Event 의 OnDraw 트리거를 처리합니다.
        /// </summary>
        private void OnSideCardDrawn(TcgBattleDataSide side, TcgBattleDataCard drawnCard)
        {
            if (IsBattleEnded) return;
            if (side == null) return;
            if (drawnCard == null) return;

            ResolveTriggersForSide(side, TcgAbilityConstants.TcgAbilityTriggerType.OnDraw, drawnCard);
        }

        /// <summary>
        /// 지정된 Side의 Permanent/Event 존을 순회하며 triggerType에 해당하는 Ability를 실행합니다.
        /// </summary>
        private void ResolveTriggersForSide(
            TcgBattleDataSide ownerSide,
            TcgAbilityConstants.TcgAbilityTriggerType tcgAbilityTriggerType,
            TcgBattleDataCard sourceCard)
        {
            if (IsBattleEnded) return;
            if (ownerSide == null) return;

            var opponentSide =
                ownerSide.Side == ConfigCommonTcg.TcgPlayerSide.Player
                    ? Context.Enemy
                    : Context.Player;

            // -------------------------
            // 1) Permanents
            // -------------------------
            if (ownerSide.Permanents != null)
            {
                var permanents = ownerSide.Permanents.Items;
                for (int i = 0; i < permanents.Count; i++)
                {
                    var p = permanents[i];
                    if (p == null) continue;

                    if (p.Definition.tcgAbilityTriggerType != tcgAbilityTriggerType)
                        continue;

                    // IntervalTurn 규칙 (1이면 매번, 2 이상이면 N턴마다)
                    if (p.Definition.intervalTurn > 1)
                    {
                        // LastResolvedTurn == 0 이면 첫 실행 허용
                        int last = p.LastResolvedTurn;
                        int nextAllowed = (last <= 0) ? Context.TurnCount : last + p.Definition.intervalTurn;
                        if (Context.TurnCount < nextAllowed)
                            continue;
                    }

                    // 스택 한도 정리(옵션)
                    if (p.Definition.maxStacks > 0 && p.Stacks > p.Definition.maxStacks)
                        p.Stacks = p.Definition.maxStacks;

                    RunAbilityById(
                        abilityUid: p.Definition.abilityUid,
                        ownerSide: ownerSide,
                        opponentSide: opponentSide,
                        tcgAbilityTriggerType: tcgAbilityTriggerType,
                        sourceCard: sourceCard,
                        sourceInstance: p);

                    p.LastResolvedTurn = Context.TurnCount;

                    if (IsBattleEnded) return;
                }
            }

            if (IsBattleEnded) return;

            // -------------------------
            // 2) Events (발동 후 제거 가능하므로 뒤에서부터 순회)
            // -------------------------
            if (ownerSide.Events != null)
            {
                var events = ownerSide.Events.Items;
                for (int i = events.Count - 1; i >= 0; i--)
                {
                    var e = events[i];
                    if (e == null) continue;

                    if (e.Definition.tcgAbilityTriggerType != tcgAbilityTriggerType)
                        continue;

                    RunAbilityById(
                        abilityUid: e.Definition.abilityUid,
                        ownerSide: ownerSide,
                        opponentSide: opponentSide,
                        tcgAbilityTriggerType: tcgAbilityTriggerType,
                        sourceCard: sourceCard,
                        sourceInstance: e);

                    if (e.Definition.consumeOnTrigger)
                    {
                        ownerSide.Events.Remove(e);
                    }

                    if (IsBattleEnded) return;
                }
            }
        }

        private void RunAbilityById(
            int abilityUid,
            TcgBattleDataSide ownerSide,
            TcgBattleDataSide opponentSide,
            TcgAbilityConstants.TcgAbilityTriggerType tcgAbilityTriggerType,
            TcgBattleDataCard sourceCard,
            object sourceInstance)
        {
            if (abilityUid <= 0) return;

            // 프로젝트의 AbilityRunner API에 맞게 호출
            TcgAbilityRunner.RunAbility(
                Context,
                ownerSide,
                opponentSide,
                sourceCard,
                new List<TcgAbilityData>(1) { new TcgAbilityData { abilityUid = abilityUid } },
                explicitTargetBattleData: null,
                tcgAbilityTriggerType: tcgAbilityTriggerType,
                presentationEvent: PublishAbilityPresentation);

            // 실행 후 전투 종료 조건 체크(안전)
            TryCheckBattleEnd();
        }

        /// <summary>
        /// <see cref="AbilityPresentation"/> 이벤트를 안전하게 발행합니다.
        /// </summary>
        internal void PublishAbilityPresentation(TcgAbilityPresentationEvent ev)
        {
            AbilityPresentation?.Invoke(ev);
        }

        #endregion

        #region Battle End

        public void ForceEnd(ConfigCommonTcg.TcgPlayerSide winner)
        {
            IsBattleEnded = true;
            Winner = winner;
        }

        public void TryCheckBattleEnd()
        {
            if (IsBattleEnded) return;

            var playerHp = Context.Player.Hero.Hp;
            var enemyHp = Context.Enemy.Hero.Hp;

            // 1) HP 기반 종료
            if (playerHp <= 0 && enemyHp <= 0)
            {
                IsBattleEnded = true;
                Winner = ConfigCommonTcg.TcgPlayerSide.Draw;
                TcgPackageManager.Instance.battleManager.OnBattleEnded(Winner);
                return;
            }
            if (playerHp <= 0)
            {
                IsBattleEnded = true;
                Winner = ConfigCommonTcg.TcgPlayerSide.Enemy;
                TcgPackageManager.Instance.battleManager.OnBattleEnded(Winner);
                return;
            }
            if (enemyHp <= 0)
            {
                IsBattleEnded = true;
                Winner = ConfigCommonTcg.TcgPlayerSide.Player;
                TcgPackageManager.Instance.battleManager.OnBattleEnded(Winner);
                return;
            }

            // 2) 카드 고갈 기반 종료 (HP 종료가 아닐 때만)
            int playerHandCount = Context.Player.Hand.GetCount();
            int enemyHandCount = Context.Enemy.Hand.GetCount();
            int playerBoardCount = Context.Player.Board.GetCount();
            int enemyBoardCount = Context.Enemy.Board.GetCount();
            int playerDeckCount = Context.Player.TcgBattleDataDeck.Count;
            int enemyDeckCount = Context.Enemy.TcgBattleDataDeck.Count;

            bool playerEmpty = (playerHandCount <= 0 && playerBoardCount <= 0 && playerDeckCount <= 0);
            bool enemyEmpty = (enemyHandCount <= 0 && enemyBoardCount <= 0 && enemyDeckCount <= 0);

            if (!playerEmpty && !enemyEmpty) return;

            IsBattleEnded = true;

            if (playerEmpty && !enemyEmpty)
                Winner = ConfigCommonTcg.TcgPlayerSide.Enemy;
            else if (enemyEmpty && !playerEmpty)
                Winner = ConfigCommonTcg.TcgPlayerSide.Player;
            else
            {
                if (playerHp > enemyHp) Winner = ConfigCommonTcg.TcgPlayerSide.Player;
                else if (enemyHp > playerHp) Winner = ConfigCommonTcg.TcgPlayerSide.Enemy;
                else Winner = ConfigCommonTcg.TcgPlayerSide.Draw;
            }

            TcgPackageManager.Instance.battleManager.OnBattleEnded(Winner);
        }

        #endregion

        #region Command Execution

        public void ExecuteCommandWithTrace(in TcgBattleCommand command, List<TcgBattleCommandTrace> traces)
        {
            if (IsBattleEnded) return;
            traces?.Clear();

            _executionQueue.Clear();
            _executionQueue.Add(command);

            ProcessExecutionQueue(traces);
        }

        private void ExecuteCommandsWithTrace(List<TcgBattleCommand> commands, List<TcgBattleCommandTrace> traces)
        {
            if (IsBattleEnded) return;
            if (commands == null || commands.Count == 0) return;
            traces?.Clear();

            _executionQueue.Clear();
            _executionQueue.AddRange(commands);

            ProcessExecutionQueue(traces);
        }

        private void ProcessExecutionQueue(List<TcgBattleCommandTrace> traces)
        {
            while (_executionQueue.Count > 0 && !IsBattleEnded)
            {
                var cmd = _executionQueue[0];
                _executionQueue.RemoveAt(0);

                if (!_commandHandlers.TryGetValue(cmd.CommandType, out var handler))
                {
                    GcLogger.LogError($"[{nameof(TcgBattleSession)}] 핸들러가 등록되지 않은 커맨드 타입: {cmd.CommandType}");
                    continue;
                }

                var result = handler.Execute(Context, in cmd);
                traces?.Add(new TcgBattleCommandTrace(in cmd, result));

                HandleCommandResult(result);

                // 커맨드 실행 후 종료 조건
                TryCheckBattleEnd();
            }
        }

        private void HandleCommandResult(CommandResult result)
        {
            if (result == null) return;

            if (!result.Success)
            {
                if (!string.IsNullOrEmpty(result.MessageKey))
                    _systemMessageManager?.ShowMessageError(result.MessageKey);

                return;
            }

            if (result.HasFollowUps)
                _executionQueue.AddRange(result.FollowUpCommands);
        }

        #endregion

        public void Dispose()
        {
            UnsubscribeDrawEvents();

            _playerController?.Dispose();
            _enemyController?.Dispose();

            Context.ClearOwner();
        }
    }
}
