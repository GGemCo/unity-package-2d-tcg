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
        /// UI는 이 이벤트를 구독하여 <see cref="TcgAbilityConstants.TcgAbilityType"/>별 연출을 재생할 수 있습니다.
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
        private Action<TcgBattleDataCardInHand> _onPlayerCardDrawn;
        private Action<TcgBattleDataCardInHand> _onEnemyCardDrawn;

        // (note) AbilityPresentation 이벤트 발행은 아래 PublishAbilityPresentation()을 통해 일괄 처리합니다.

        /// <summary>
        /// 현재 도메인 로직에서 생성된 <see cref="TcgPresentationStep"/>을 누적할 대상입니다.
        ///
        /// 목적:
        /// - 커맨드 실행 중(OnPlay Ability 포함) 발생하는 추가 트리거(OnDraw/OnTurnStart/OnTurnEnd 등)까지
        ///   동일한 타임라인에서 재생될 수 있도록, 세션 레벨에서 "현재 누적 대상"을 제공합니다.
        /// </summary>
        private List<TcgPresentationStep> _presentationStepsSink;

        /// <summary>
        /// 세션이 실행하는 Ability/Trigger에서 생성되는 연출 Step을 지정한 리스트로 누적하도록 설정합니다.
        /// using 스코프에서 자동으로 원복됩니다.
        /// </summary>
        internal IDisposable BeginPresentationCapture(List<TcgPresentationStep> steps)
        {
            return new PresentationCaptureScope(this, steps);
        }

        private sealed class PresentationCaptureScope : IDisposable
        {
            private readonly TcgBattleSession _session;
            private readonly List<TcgPresentationStep> _prev;
            private bool _disposed;

            public PresentationCaptureScope(TcgBattleSession session, List<TcgPresentationStep> next)
            {
                _session = session;
                _prev = session._presentationStepsSink;
                session._presentationStepsSink = next;
            }

            public void Dispose()
            {
                if (_disposed) return;
                _disposed = true;
                _session._presentationStepsSink = _prev;
            }
        }

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

            // 2) ActiveSide 전환
            Context.ActiveSide =
                (Context.ActiveSide == ConfigCommonTcg.TcgPlayerSide.Player)
                    ? ConfigCommonTcg.TcgPlayerSide.Enemy
                    : ConfigCommonTcg.TcgPlayerSide.Player;

            // 3) 턴 수 증가
            if (Context.ActiveSide == ConfigCommonTcg.TcgPlayerSide.Player)
                Context.TurnCount++;
            
            // 4) (선택) 공격 가능 상태 초기화
            if (Context.ActiveSide == ConfigCommonTcg.TcgPlayerSide.Player)
            {
                Context.Player.SetFieldCardCanAttack(true);
                Context.Enemy.SetFieldCardCanAttack(true);
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
            ResolveTriggersForSide(activeSide, TcgAbilityConstants.TcgAbilityTriggerType.OnTurnEnd, sourceCardInHand: null);
        }

        private void ResolveStartOfTurnEffects()
        {
            if (IsBattleEnded) return;

            var activeSide = Context.GetSideState(Context.ActiveSide);

            // Permanent/Event 등 턴 시작 트리거 처리
            ResolveTriggersForSide(activeSide, TcgAbilityConstants.TcgAbilityTriggerType.OnTurnStart, sourceCardInHand: null);
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
        private void OnSideCardDrawn(TcgBattleDataSide side, TcgBattleDataCardInHand drawnCardInHand)
        {
            if (IsBattleEnded) return;
            if (side == null) return;
            if (drawnCardInHand == null) return;

            ResolveTriggersForSide(side, TcgAbilityConstants.TcgAbilityTriggerType.OnDraw, drawnCardInHand);
        }

        /// <summary>
        /// 지정된 Side의 Permanent/Event 존을 순회하며 triggerType에 해당하는 Ability를 실행합니다.
        /// </summary>
        private void ResolveTriggersForSide(
            TcgBattleDataSide ownerSide,
            TcgAbilityConstants.TcgAbilityTriggerType tcgAbilityTriggerType,
            TcgBattleDataCardInHand sourceCardInHand)
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
                // 만료 시 제거될 수 있으므로 뒤에서부터 순회합니다.
                for (int i = permanents.Count - 1; i >= 0; i--)
                {
                    var p = permanents[i];
                    if (p == null) continue;

                    // 0) Lifetime tick (턴 트리거 시점)
                    p.Lifetime?.OnTurnTrigger(new TcgPermanentLifetimeContext(Context.TurnCount, tcgAbilityTriggerType, p));
                    if (p.IsExpired)
                    {
                        ownerSide.Permanents.Remove(p);
                        ownerSide.Permanents.NotifyExpired(p);
                        continue;
                    }

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

                    var casterZone = p.AttackerZone;
                    var (targetZone, targetIndex) = ResolveTriggerTarget(ownerSide, opponentSide, p.Definition.tcgAbilityTargetType);
                    if (targetIndex < 0)
                        (targetZone, targetIndex) = ResolveFallbackTarget(ownerSide);

                    RunAbilityById(
                        ability: TcgAbilityBuilder.BuildAbility(p.Definition),
                        side: ownerSide.Side,
                        casterZone: casterZone,
                        casterIndex: -1,
                        targetZone: targetZone,
                        targetIndex: targetIndex,
                        sourceInstance: p);

                    p.LastResolvedTurn = Context.TurnCount;

                    // 1) Lifetime tick (발동 완료)
                    p.Lifetime?.OnAbilityResolved(new TcgPermanentLifetimeContext(Context.TurnCount, tcgAbilityTriggerType, p));
                    if (p.IsExpired)
                    {
                        ownerSide.Permanents.Remove(p);
                        ownerSide.Permanents.NotifyExpired(p);
                    }

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

                    var (targetZone, targetIndex) = ResolveTriggerTarget(ownerSide, opponentSide, e.Definition.tcgAbilityTargetType);
                    if (targetIndex < 0)
                        (targetZone, targetIndex) = ResolveFallbackTarget(ownerSide);

                    RunAbilityById(
                        ability: TcgAbilityBuilder.BuildAbility(e.Definition),
                        side: ownerSide.Side,
                        casterZone: ConfigCommonTcg.TcgZone.None,
                        casterIndex: -1,
                        targetZone: targetZone,
                        targetIndex: targetIndex,
                        sourceInstance: e);

                    if (e.Definition.consumeOnTrigger)
                        ownerSide.Events.Remove(e);

                    if (IsBattleEnded) return;
                }
            }

            // -------------------------
            // Local helpers
            // -------------------------
            (ConfigCommonTcg.TcgZone zone, int index) ResolveFallbackTarget(TcgBattleDataSide side)
            {
                var z = (side.Side == ConfigCommonTcg.TcgPlayerSide.Player)
                    ? ConfigCommonTcg.TcgZone.FieldPlayer
                    : ConfigCommonTcg.TcgZone.FieldEnemy;
                return (z, ConfigCommonTcg.IndexHeroSlot);
            }

            (ConfigCommonTcg.TcgZone zone, int index) ResolveTriggerTarget(
                TcgBattleDataSide owner,
                TcgBattleDataSide opponent,
                TcgAbilityConstants.TcgAbilityTargetType targetType)
            {
                var targetSide = owner;
                var zone = (owner.Side == ConfigCommonTcg.TcgPlayerSide.Player)
                    ? ConfigCommonTcg.TcgZone.FieldPlayer
                    : ConfigCommonTcg.TcgZone.FieldEnemy;

                if (targetType == TcgAbilityConstants.TcgAbilityTargetType.AllEnemies
                    || targetType == TcgAbilityConstants.TcgAbilityTargetType.EnemyCreature
                    || targetType == TcgAbilityConstants.TcgAbilityTargetType.EnemyHero)
                {
                    targetSide = opponent;
                    zone = (opponent.Side == ConfigCommonTcg.TcgPlayerSide.Player)
                        ? ConfigCommonTcg.TcgZone.FieldPlayer
                        : ConfigCommonTcg.TcgZone.FieldEnemy;
                }

                var target = TcgBattleCommand.ResolveRandomTarget(targetType, owner, opponent, includeHero: true);
                return target != null ? (zone, target.Index) : (zone, -1);
            }
        }

        private void RunAbilityById(
            in TcgAbilityDefinition ability,
            ConfigCommonTcg.TcgPlayerSide side,
            ConfigCommonTcg.TcgZone casterZone,
            int casterIndex,
            ConfigCommonTcg.TcgZone targetZone,
            int targetIndex,
            object sourceInstance)
        {
            if (!ability.IsValid) return;
            if (side == ConfigCommonTcg.TcgPlayerSide.None) return;

            // caster/target 정보가 일부 없는 트리거(예: Event)도 존재할 수 있으므로,
            // 여기서 과도하게 차단하지 않고 AbilityRunner/Handler의 규칙에 맡깁니다.
            // (단, zone이 None이면 Context에서 타겟을 해석할 수 없으므로 최소한의 기본값을 보정합니다.)
            if (targetZone == ConfigCommonTcg.TcgZone.None || targetIndex < 0)
            {
                targetZone = (side == ConfigCommonTcg.TcgPlayerSide.Player)
                    ? ConfigCommonTcg.TcgZone.FieldPlayer
                    : ConfigCommonTcg.TcgZone.FieldEnemy;
                targetIndex = ConfigCommonTcg.IndexHeroSlot;
            }

            var list = new List<TcgAbilityData>(1)
            {
                new TcgAbilityData { ability = ability }
            };
            TcgAbilityRunner.RunAbility(
                Context,
                side,
                casterZone,
                casterIndex,
                targetZone,
                targetIndex,
                list,
                tcgAbilityTriggerType: ability.tcgAbilityTriggerType,
                presentationEvent: PublishAbilityPresentation);

            TryCheckBattleEnd();
        }

        internal void PublishAbilityPresentation(TcgAbilityPresentationEvent ev)
        {
            // 1) 외부 구독자(디버그/로그/별도 UI)
            AbilityPresentation?.Invoke(ev);

            // 2) 단일 타임라인 합류용 PresentationStep 누적
            if (_presentationStepsSink == null)
                return;

            TcgAbilityPresentationStepFactory.TryCreateSteps(ev, _presentationStepsSink);
        }

        #endregion

        #region Battle End

        /// <summary>
        /// 강제로 전투를 종료합니다. (씬 전환 등)
        /// </summary>
        public void ForceEnd(ConfigCommonTcg.TcgPlayerSide winner)
        {
            IsBattleEnded = true;
            Winner = winner;
        }

        public void TryCheckBattleEnd()
        {
            if (IsBattleEnded) return;

            var playerHp = Context.Player.Field.Hero.Health;
            var enemyHp = Context.Enemy.Field.Hero.Health;

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
            int playerHandCount = Context.Player.Hand.Count;
            int enemyHandCount = Context.Enemy.Hand.Count;
            int playerFieldCount = Context.Player.Field.Count;
            int enemyFieldCount = Context.Enemy.Field.Count;
            int playerDeckCount = Context.Player.TcgBattleDataDeck.Count;
            int enemyDeckCount = Context.Enemy.TcgBattleDataDeck.Count;

            bool playerEmpty = (playerHandCount <= 0 && playerFieldCount <= 0 && playerDeckCount <= 0);
            bool enemyEmpty = (enemyHandCount <= 0 && enemyFieldCount <= 0 && enemyDeckCount <= 0);

            if (playerEmpty || enemyEmpty)
            {
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
                return;
            }
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

        /// <summary>
        /// 특정 Side 에 해당하는 상태를 반환합니다.
        /// </summary>
        public TcgBattleDataSide GetSideState(ConfigCommonTcg.TcgPlayerSide side)
        {
            return Context.GetSideState(side);
        }
        public TcgBattleDataSide GetOpponentState(ConfigCommonTcg.TcgPlayerSide side)
        {
            return Context.GetOpponentState(side);
        }
    }
}
