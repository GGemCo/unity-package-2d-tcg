using System;
using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 한 판의 전투 진행(턴 흐름, 커맨드 실행, 트리거 처리, 승패 판정)을 담당하는 도메인 세션입니다.
    /// </summary>
    /// <remarks>
    /// UI/MonoBehaviour/Scene에 의존하지 않도록 설계된 도메인 레이어의 핵심 객체이며,
    /// 외부에서는 커맨드 실행 및 턴 진행 API를 통해 세션을 조작합니다.
    /// </remarks>
    public sealed class TcgBattleSession : IDisposable
    {
        /// <summary>
        /// Ability 실행 시점(Begin/End)을 UI 레이어에 알리기 위한 이벤트입니다.
        /// </summary>
        /// <remarks>
        /// UI는 본 이벤트를 구독하여 <see cref="TcgAbilityConstants.TcgAbilityType"/>별 연출을 재생할 수 있습니다.
        /// 또한, 세션 내부에서는 동일 이벤트를 Step으로 변환해 단일 타임라인에 합류시키는 용도로도 사용합니다.
        /// </remarks>
        public event Action<TcgAbilityPresentationEvent> AbilityPresentation;

        /// <summary>
        /// 전투의 현재 상태(플레이어/적 사이드, 손패/필드, 턴 수 등)를 보유하는 컨텍스트입니다.
        /// </summary>
        public TcgBattleDataMain Context { get; }

        /// <summary>
        /// 현재 턴이 플레이어 턴인지 여부입니다.
        /// </summary>
        public bool IsPlayerTurn => Context.ActiveSide == ConfigCommonTcg.TcgPlayerSide.Player;

        /// <summary>
        /// 전투가 종료되었는지 여부입니다.
        /// </summary>
        public bool IsBattleEnded { get; private set; }

        /// <summary>
        /// 전투 승자입니다(무승부 포함). 외부 노출 정책에 따라 private로 유지됩니다.
        /// </summary>
        private ConfigCommonTcg.TcgPlayerSide Winner { get; set; }

        private readonly Dictionary<ConfigCommonTcg.TcgBattleCommandType, ITcgBattleCommandHandler> _commandHandlers;
        private readonly ITcgPlayerController _playerController;
        private readonly ITcgPlayerController _enemyController;

        /// <summary>
        /// 컨트롤러가 생성한 커맨드를 임시로 담는 버퍼입니다.
        /// </summary>
        private readonly List<TcgBattleCommand> _commandBuffer = new List<TcgBattleCommand>(32);

        /// <summary>
        /// 실행 대기 중인 커맨드 큐입니다(후속 커맨드 포함).
        /// </summary>
        private readonly List<TcgBattleCommand> _executionQueue = new List<TcgBattleCommand>(64);

        private readonly GGemCoTcgSettings _tcgSettings;
        private readonly SystemMessageManager _systemMessageManager;

        // CardDrawn 이벤트 구독 해제용
        private Action<TcgBattleDataCardInHand> _onPlayerCardDrawn;
        private Action<TcgBattleDataCardInHand> _onEnemyCardDrawn;

        // (note) AbilityPresentation 이벤트 발행은 아래 PublishAbilityPresentation()을 통해 일괄 처리합니다.

        /// <summary>
        /// 현재 도메인 로직에서 생성된 <see cref="TcgPresentationStep"/>을 누적할 대상 리스트입니다.
        /// </summary>
        /// <remarks>
        /// 목적:
        /// - 커맨드 실행 중(OnPlay Ability 포함) 발생하는 추가 트리거(OnDraw/OnTurnStart/OnTurnEnd 등)까지
        ///   동일한 타임라인에서 재생될 수 있도록 “현재 누적 대상”을 세션 레벨에서 제공합니다.
        /// - UI 레이어는 커맨드 단위의 PresentationSteps만 재생하더라도,
        ///   세션이 누적한 Ability Step이 함께 포함되면 단일 러너에서 자연스럽게 이어서 재생할 수 있습니다.
        /// </remarks>
        private List<TcgPresentationStep> _presentationStepsSink;

        /// <summary>
        /// 세션이 실행하는 Ability/Trigger에서 생성되는 연출 Step을 지정한 리스트로 누적하도록 설정합니다.
        /// </summary>
        /// <param name="steps">연출 Step을 누적할 대상 리스트입니다.</param>
        /// <returns>스코프 종료 시 이전 누적 대상으로 복원하는 <see cref="IDisposable"/>입니다.</returns>
        /// <remarks>
        /// using 스코프 내에서만 캡처가 유효하며, 스코프 종료 시 자동으로 원복됩니다.
        /// </remarks>
        internal IDisposable BeginPresentationCapture(List<TcgPresentationStep> steps)
        {
            return new PresentationCaptureScope(this, steps);
        }

        /// <summary>
        /// PresentationStep 누적 대상 전환을 스코프로 관리하기 위한 내부 헬퍼입니다.
        /// </summary>
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

        /// <summary>
        /// 전투 세션을 생성하고, 컨텍스트/컨트롤러/트리거 구독을 초기화합니다.
        /// </summary>
        /// <param name="playerSide">플레이어 사이드 상태입니다.</param>
        /// <param name="enemySide">적 사이드 상태입니다.</param>
        /// <param name="commandHandlers">커맨드 타입별 실행 핸들러 테이블입니다.</param>
        /// <param name="playerController">플레이어 입력/행동 결정을 담당하는 컨트롤러입니다.</param>
        /// <param name="enemyController">적(AI) 행동 결정을 담당하는 컨트롤러입니다.</param>
        /// <param name="settings">전투 규칙/수치 설정입니다.</param>
        /// <param name="systemMessageManager">실패 메시지 등 시스템 메시지 표시를 위한 매니저입니다.</param>
        /// <exception cref="ArgumentNullException">필수 의존성이 null이면 발생합니다.</exception>
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

        /// <summary>
        /// 드로우 이벤트를 구독하여 OnDraw 트리거 처리를 세션에서 수행하도록 연결합니다.
        /// </summary>
        private void SubscribeDrawEvents()
        {
            // 플레이어
            _onPlayerCardDrawn = card => OnSideCardDrawn(Context.Player, card);
            Context.Player.CardDrawn += _onPlayerCardDrawn;

            // 적
            _onEnemyCardDrawn = card => OnSideCardDrawn(Context.Enemy, card);
            Context.Enemy.CardDrawn += _onEnemyCardDrawn;
        }

        /// <summary>
        /// 드로우 이벤트 구독을 해제합니다.
        /// </summary>
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

        /// <summary>
        /// 적(AI) 턴의 행동을 결정하고 커맨드를 실행하여 트레이스를 누적합니다.
        /// </summary>
        /// <param name="traces">실행된 커맨드 트레이스를 누적할 리스트입니다(선택).</param>
        public void ExecuteEnemyTurnWithTrace(List<TcgBattleCommandTrace> traces)
        {
            if (IsBattleEnded) return;
            if (IsPlayerTurn) return;

            _commandBuffer.Clear();
            _enemyController.DecideTurnActions(Context, _commandBuffer);
            ExecuteCommandsWithTrace(_commandBuffer, traces);
        }

        /// <summary>
        /// 현재 턴을 종료하고 다음 턴으로 진행합니다.
        /// </summary>
        /// <remarks>
        /// 처리 순서(요약):
        /// 1) 턴 종료 트리거 처리
        /// 2) ActiveSide 전환 및 턴 수 증가(플레이어 턴 시작 기준)
        /// 3) (선택) 공격 가능 상태 초기화
        /// 4) 마나 증가/회복 및 턴 시작 드로우
        /// 5) 턴 시작 트리거 처리
        /// </remarks>
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

            // 3) 턴 수 증가(플레이어 턴 시작 시점에 증가)
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

        /// <summary>
        /// 턴 종료 시점에 발동하는 트리거(OnTurnEnd)를 처리합니다.
        /// </summary>
        private void ResolveEndOfTurnEffects()
        {
            if (IsBattleEnded) return;

            var activeSide = Context.GetSideState(Context.ActiveSide);

            // Permanent/Event 등 턴 종료 트리거 처리
            ResolveTriggersForSide(activeSide, TcgAbilityConstants.TcgAbilityTriggerType.OnTurnEnd, sourceCardInHand: null);
        }

        /// <summary>
        /// 턴 시작 시점에 발동하는 트리거(OnTurnStart)를 처리합니다.
        /// </summary>
        private void ResolveStartOfTurnEffects()
        {
            if (IsBattleEnded) return;

            var activeSide = Context.GetSideState(Context.ActiveSide);

            // Permanent/Event 등 턴 시작 트리거 처리
            ResolveTriggersForSide(activeSide, TcgAbilityConstants.TcgAbilityTriggerType.OnTurnStart, sourceCardInHand: null);
        }

        /// <summary>
        /// 턴 시작 드로우를 수행합니다.
        /// </summary>
        /// <remarks>
        /// <c>DrawOneCard()</c> 내부에서 CardDrawn 이벤트가 발생하며,
        /// 세션이 이를 받아 OnDraw 트리거(<see cref="TcgAbilityConstants.TcgAbilityTriggerType.OnDraw"/>)를 처리합니다.
        /// </remarks>
        private void DrawStartOfTurnCard()
        {
            var side = Context.GetSideState(Context.ActiveSide);
            _ = side.DrawOneCard();
}

        /// <summary>
        /// 턴 종료 후(플레이어 턴 시작 시점) 마나 한도 증가 및 마나 회복을 수행합니다.
        /// </summary>
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

        #region Trigger Loop

        /// <summary>
        /// 특정 Side가 카드를 드로우했을 때(손패에 들어갔을 때) 호출됩니다.
        /// </summary>
        /// <param name="side">카드를 드로우한 사이드 상태입니다.</param>
        /// <param name="drawnCardInHand">손패에 들어온 카드 인스턴스입니다.</param>
        /// <remarks>
        /// Permanent/Event의 <see cref="TcgAbilityConstants.TcgAbilityTriggerType.OnDraw"/> 트리거를 처리합니다.
        /// </remarks>
        private void OnSideCardDrawn(TcgBattleDataSide side, TcgBattleDataCardInHand drawnCardInHand)
        {
            if (IsBattleEnded) return;
            if (side == null) return;
            if (drawnCardInHand == null) return;

            ResolveTriggersForSide(side, TcgAbilityConstants.TcgAbilityTriggerType.OnDraw, drawnCardInHand);
        }

        /// <summary>
        /// 지정된 Side의 Permanent/Event 존을 순회하며 <paramref name="tcgAbilityTriggerType"/>에 해당하는 Ability를 실행합니다.
        /// </summary>
        /// <param name="ownerSide">트리거 소유 사이드입니다.</param>
        /// <param name="tcgAbilityTriggerType">처리할 트리거 타입입니다.</param>
        /// <param name="sourceCardInHand">OnDraw 등에서 트리거의 소스가 되는 카드(없으면 null)입니다.</param>
        /// <remarks>
        /// - Permanent는 만료/제거가 발생할 수 있어 뒤에서부터 순회합니다.
        /// - Event는 발동 후 소비(consume)될 수 있어 뒤에서부터 순회합니다.
        /// - 각 Ability 실행 후 <see cref="TryCheckBattleEnd"/>로 종료 조건을 점검합니다.
        /// </remarks>
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

                    if (p.Definition.TcgAbilityTriggerType != tcgAbilityTriggerType)
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
                    var (targetZone, targetIndex) = ResolveTriggerTarget(ownerSide, opponentSide, p.Definition.TcgAbilityTargetType);
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

                    if (e.Definition.TcgAbilityTriggerType != tcgAbilityTriggerType)
                        continue;

                    var (targetZone, targetIndex) = ResolveTriggerTarget(ownerSide, opponentSide, e.Definition.TcgAbilityTargetType);
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

        /// <summary>
        /// Ability 정의를 실행하고, 실행 과정에서 발생하는 연출 이벤트를 발행합니다.
        /// </summary>
        /// <param name="ability">실행할 Ability 정의입니다.</param>
        /// <param name="side">캐스터(소유) 사이드입니다.</param>
        /// <param name="casterZone">캐스터가 위치한 존입니다.</param>
        /// <param name="casterIndex">캐스터 인덱스입니다(없으면 -1).</param>
        /// <param name="targetZone">대상 존입니다.</param>
        /// <param name="targetIndex">대상 인덱스입니다.</param>
        /// <param name="sourceInstance">Permanent/Event 등 트리거 소스 인스턴스입니다(디버그/추적 용도).</param>
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
            // 과도하게 차단하지 않고 AbilityRunner/Handler의 규칙에 맡깁니다.
            // (단, zone이 None이거나 index가 음수이면 최소한의 기본값을 보정합니다.)
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

        /// <summary>
        /// Ability 연출 이벤트를 발행하고, 필요 시 <see cref="TcgPresentationStep"/>으로 변환해 누적합니다.
        /// </summary>
        /// <param name="ev">발행할 Ability 연출 이벤트입니다.</param>
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
        /// 전투를 강제로 종료합니다(씬 전환 등).
        /// </summary>
        /// <param name="winner">강제 지정할 승자입니다.</param>
        public void ForceEnd(ConfigCommonTcg.TcgPlayerSide winner)
        {
            IsBattleEnded = true;
            Winner = winner;
        }

        /// <summary>
        /// 현재 전투의 종료 조건을 검사하고, 종료 시 승자를 결정합니다.
        /// </summary>
        /// <remarks>
        /// 종료 정책:
        /// 1) 영웅 HP가 0 이하인 경우(동시 0 이하면 무승부)
        /// 2) HP 종료가 아닐 때, 손패/필드/덱이 모두 비면(카드 고갈) 종료
        /// </remarks>
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

        /// <summary>
        /// 단일 커맨드를 실행하고, 실행 결과 트레이스를 <paramref name="traces"/>에 기록합니다.
        /// </summary>
        /// <param name="command">실행할 커맨드입니다.</param>
        /// <param name="traces">실행 트레이스를 누적할 리스트입니다(선택). 전달 시 내부에서 Clear 합니다.</param>
        public void ExecuteCommandWithTrace(in TcgBattleCommand command, List<TcgBattleCommandTrace> traces)
        {
            if (IsBattleEnded) return;
            traces?.Clear();

            _executionQueue.Clear();
            _executionQueue.Add(command);

            ProcessExecutionQueue(traces);
        }

        /// <summary>
        /// 여러 커맨드를 순서대로 실행하고, 실행 결과 트레이스를 <paramref name="traces"/>에 기록합니다.
        /// </summary>
        /// <param name="commands">실행할 커맨드 목록입니다.</param>
        /// <param name="traces">실행 트레이스를 누적할 리스트입니다(선택). 전달 시 내부에서 Clear 합니다.</param>
        private void ExecuteCommandsWithTrace(List<TcgBattleCommand> commands, List<TcgBattleCommandTrace> traces)
        {
            if (IsBattleEnded) return;
            if (commands == null || commands.Count == 0) return;
            traces?.Clear();

            _executionQueue.Clear();
            _executionQueue.AddRange(commands);

            ProcessExecutionQueue(traces);
        }

        /// <summary>
        /// 실행 큐를 소진할 때까지 커맨드를 처리합니다(후속 커맨드 포함).
        /// </summary>
        /// <param name="traces">실행 트레이스를 누적할 리스트입니다(선택).</param>
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

        /// <summary>
        /// 커맨드 실행 결과를 해석하여 메시지 표시 및 후속 커맨드를 처리합니다.
        /// </summary>
        /// <param name="result">커맨드 실행 결과입니다.</param>
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

        /// <summary>
        /// 세션을 종료하며, 이벤트 구독/컨트롤러 자원을 해제합니다.
        /// </summary>
        public void Dispose()
        {
            UnsubscribeDrawEvents();

            _playerController?.Dispose();
            _enemyController?.Dispose();

            Context.ClearOwner();
        }

        /// <summary>
        /// 지정한 Side에 해당하는 상태를 반환합니다.
        /// </summary>
        /// <param name="side">조회할 사이드입니다.</param>
        /// <returns>요청한 사이드의 상태를 반환합니다.</returns>
        public TcgBattleDataSide GetSideState(ConfigCommonTcg.TcgPlayerSide side)
        {
            return Context.GetSideState(side);
        }

        /// <summary>
        /// 지정한 Side의 상대편 상태를 반환합니다.
        /// </summary>
        /// <param name="side">기준 사이드입니다.</param>
        /// <returns>상대편 사이드의 상태를 반환합니다.</returns>
        public TcgBattleDataSide GetOpponentState(ConfigCommonTcg.TcgPlayerSide side)
        {
            return Context.GetOpponentState(side);
        }
    }
}
