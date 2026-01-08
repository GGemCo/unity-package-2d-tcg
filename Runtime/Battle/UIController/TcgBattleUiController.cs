using System;
using System.Collections;
using System.Collections.Generic;
using GGemCo2DCore;
using R3;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 전투 세션(<see cref="TcgBattleSession"/>)과 전투 UI 윈도우를 연결하고,
    /// 연출 재생(Presentation) 이후 UI를 갱신/해제하는 코디네이터입니다.
    /// </summary>
    /// <remarks>
    /// 사용 흐름:
    /// <list type="number">
    /// <item><description><see cref="TrySetupWindows"/>로 전투 관련 UI 윈도우 참조를 확보합니다.</description></item>
    /// <item><description><see cref="BindBattleManager"/>로 세션/설정/연출 러너를 구성합니다.</description></item>
    /// <item><description><see cref="PlayPresentationAndRefresh"/>로 트레이스 기반 연출을 순차 재생하고 최종 UI를 동기화합니다.</description></item>
    /// </list>
    /// </remarks>
    public sealed class TcgBattleUiController
    {
        private UIWindowTcgFieldEnemy  _fieldEnemy;
        private UIWindowTcgFieldPlayer _fieldPlayer;
        private UIWindowTcgHandPlayer  _handPlayer;
        private UIWindowTcgHandEnemy   _handEnemy;
        private UIWindowTcgBattleHud   _battleHud;

        /// <summary>
        /// UI 이벤트/바인딩 등에서 사용하는 구독 해제 컨테이너입니다.
        /// </summary>
        private readonly CompositeDisposable _disposables = new();

        private TcgBattleSession _session;
        private Coroutine _presentationCoroutine;

        private TcgPresentationRunner _runner;
        private TcgPresentationContext _ctx;

        // (note) Ability 연출은 CommandResult.PresentationSteps에 포함되어 단일 러너에서 재생됩니다.
        //        (CommandHandlerBase.TryRunOnPlayAbility 참고)

        /// <summary>
        /// 전투 UI 구성 요소(필드/손패/HUD)가 모두 준비되었는지 여부입니다.
        /// </summary>
        public bool IsReady =>
            _fieldEnemy  != null &&
            _fieldPlayer != null &&
            _handPlayer  != null &&
            _handEnemy   != null &&
            _battleHud   != null;

        // ------------------------------
        // Interaction Lock (연출 중 입력 차단)
        // ------------------------------
        private int _interactionLockDepth;

        /// <summary>
        /// 현재 연출 재생으로 인해 사용자 인터랙션이 잠겨 있는지 여부입니다.
        /// </summary>
        public bool IsInteractionLocked => _interactionLockDepth > 0;

        /// <summary>
        /// 현재 커맨드 연출 코루틴이 재생 중인지 여부입니다.
        /// </summary>
        public bool IsPresenting => _presentationCoroutine != null;

        /// <summary>
        /// <see cref="SceneGame"/>의 <see cref="UIWindowManager"/>에서 전투 관련 윈도우를 찾아 캐싱합니다.
        /// </summary>
        /// <returns>필요한 모든 윈도우를 찾았으면 true, 일부라도 누락되면 false입니다.</returns>
        public bool TrySetupWindows()
        {
            var windowManager = SceneGame.Instance?.uIWindowManager;
            if (windowManager == null)
            {
                GcLogger.LogError($"[{nameof(TcgBattleUiController)}] {nameof(UIWindowManager)} 를 찾을 수 없습니다.");
                return false;
            }

            _fieldEnemy  = windowManager.GetUIWindowByUid<UIWindowTcgFieldEnemy>(UIWindowConstants.WindowUid.TcgFieldEnemy);
            _fieldPlayer = windowManager.GetUIWindowByUid<UIWindowTcgFieldPlayer>(UIWindowConstants.WindowUid.TcgFieldPlayer);
            _handPlayer  = windowManager.GetUIWindowByUid<UIWindowTcgHandPlayer>(UIWindowConstants.WindowUid.TcgHandPlayer);
            _handEnemy   = windowManager.GetUIWindowByUid<UIWindowTcgHandEnemy>(UIWindowConstants.WindowUid.TcgHandEnemy);
            _battleHud   = windowManager.GetUIWindowByUid<UIWindowTcgBattleHud>(UIWindowConstants.WindowUid.TcgBattleHud);

            if (!IsReady)
            {
                GcLogger.LogError($"[{nameof(TcgBattleUiController)}] 전투 UI 윈도우 중 일부를 찾을 수 없습니다.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 전투 UI 윈도우(필드/손패/HUD)를 일괄적으로 표시/숨김 처리합니다.
        /// </summary>
        /// <param name="isShow">true면 표시하고, false면 숨깁니다.</param>
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
        /// 전투 매니저/세션을 바인딩하고, 연출 실행을 위한 컨텍스트와 러너를 구성합니다.
        /// </summary>
        /// <param name="manager">전투 로직/명령을 제공하는 매니저입니다.</param>
        /// <param name="session">현재 전투 세션입니다.</param>
        /// <param name="settings">TCG 전반 설정입니다.</param>
        /// <param name="uiCutsceneSettings">UI 컷씬/연출 관련 설정입니다.</param>
        /// <remarks>
        /// 핸들러 등록은 StepType 기준으로 수행되며, 커맨드 결과의 PresentationSteps가 순차 재생됩니다.
        /// </remarks>
        public void BindBattleManager(
            TcgBattleManager manager,
            TcgBattleSession session,
            GGemCoTcgSettings settings,
            GGemCoTcgUICutsceneSettings uiCutsceneSettings)
        {
            if (!IsReady || manager == null || session == null) return;

            _session = session;

            // 연출 핸들러들이 참조할 UI/세션/설정 묶음 컨텍스트
            _ctx = new TcgPresentationContext(
                _session,
                _fieldEnemy,
                _fieldPlayer,
                _handPlayer,
                _handEnemy,
                _battleHud,
                settings,
                uiCutsceneSettings);

            // 커맨드 결과의 PresentationSteps를 타입별 핸들러로 순차 실행
            _runner = new TcgPresentationRunner(new ITcgPresentationHandler[]
            {
                new HandlerMoveCardToField(),
                new HandlerMoveCardToGrave(),
                new HandlerMoveCardToBack(),
                new HandlerAttackUnit(),
                new HandlerAttackHero(),
                new HandlerEndTurn(),
                new HandlerMoveCardToTarget(),
                new HandlerAbilityDamage(),
                new HandlerAbilityHeal(),
                new HandlerAbilityBuff(),
                new HandlerAbilityDraw(),
                new HandlerAbilityGainMana(),
                new HandlerAbilityExtraAction(),
            });

            // 전투 시작 시점의 턴 타이머 시작
            _battleHud?.StartTurnTimer(settings != null ? settings.turnTimeLimitSeconds : 0);
        }

        /// <summary>
        /// 커맨드 트레이스 기반 연출을 재생한 뒤, 최종 상태로 UI를 동기화합니다.
        /// </summary>
        /// <param name="context">UI 갱신에 사용할 현재 전투 데이터입니다.</param>
        /// <param name="traces">실행된 커맨드 트레이스 목록입니다.</param>
        /// <param name="onCompleted">연출 종료 및 최종 UI 갱신 이후 호출되는 콜백입니다.</param>
        /// <remarks>
        /// - 연출이 없거나(트레이스에 Presentation이 없거나) 연출 구성요소가 준비되지 않은 경우,
        ///   코루틴을 실행하지 않고 즉시 <see cref="RefreshAll"/>을 수행합니다.
        /// - 기존 연출 코루틴이 실행 중이면 중지하고 새로 시작합니다.
        /// </remarks>
        public void PlayPresentationAndRefresh(
            TcgBattleDataMain context,
            IReadOnlyList<TcgBattleCommandTrace> traces,
            Action onCompleted = null)
        {
            if (!IsReady || context == null)
                return;

            // 연출이 없다면 입력 차단 없이 즉시 최종 UI로 동기화
            if (!HasAnyPresentation(traces))
            {
                RefreshAll(context);
                onCompleted?.Invoke();
                return;
            }

            // 연출 실행에 필요한 구성요소가 없으면, 연출 없이 즉시 최종 UI로 동기화
            if (_battleHud == null || _runner == null || _ctx == null)
            {
                RefreshAll(context);
                onCompleted?.Invoke();
                return;
            }

            // 기존 연출 코루틴이 돌고 있으면 중지 후 새로 시작
            if (_presentationCoroutine != null)
            {
                _battleHud.StopCoroutine(_presentationCoroutine);
                _presentationCoroutine = null;

                // StopCoroutine 경로에서는 finally가 보장되지 않으므로 잠금 상태를 직접 복구
                ResetInteractionLock();
            }

            _presentationCoroutine = _battleHud.StartCoroutine(CoPlayPresentationWithCallback(context, traces, onCompleted));
        }

        /// <summary>
        /// 연출 코루틴 실행 후 완료 콜백을 보장하기 위한 래퍼 코루틴입니다.
        /// </summary>
        /// <param name="context">최종 UI 동기화에 사용할 전투 데이터입니다.</param>
        /// <param name="traces">재생할 커맨드 트레이스 목록입니다.</param>
        /// <param name="onCompleted">연출 종료 및 UI 갱신 이후 호출되는 콜백입니다.</param>
        private IEnumerator CoPlayPresentationWithCallback(
            TcgBattleDataMain context,
            IReadOnlyList<TcgBattleCommandTrace> traces,
            Action onCompleted = null)
        {
            // 기존 CoPlayPresentation 로직을 그대로 재사용
            yield return CoPlayPresentation(context, traces);

            // CoPlayPresentation finally에서 RefreshAll/Unlock까지 끝난 뒤 호출됨
            onCompleted?.Invoke();
        }

        /// <summary>
        /// 커맨드 트레이스의 연출 Step을 실행하고, 전투 종료 조건을 체크하며,
        /// 종료/중단 시 UI를 최종 상태로 갱신합니다.
        /// </summary>
        /// <param name="context">최종 UI 동기화에 사용할 전투 데이터입니다.</param>
        /// <param name="traces">재생할 커맨드 트레이스 목록입니다.</param>
        private IEnumerator CoPlayPresentation(TcgBattleDataMain context, IReadOnlyList<TcgBattleCommandTrace> traces)
        {
            BeginInteractionLock();

            try
            {
                yield return _runner.Run(
                    _ctx,
                    traces,
                    _battleHud,
                    shouldStop: () => _session == null || _session.IsBattleEnded,
                    perStepEnded: () => _session.TryCheckBattleEnd()
                );
            }
            // 정상적으로 코루틴이 실행되거나 yield break; 가 호출되면 finally 가 호출 됩니다.
            // StopCoroutine 으로 종료하면 finally 가 호출되지 않습니다.
            finally
            {
                RefreshAll(context);
                _presentationCoroutine = null;
                EndInteractionLock();
            }
        }

        /// <summary>
        /// 플레이어/적의 손패, 필드, 마나 등 전투 UI를 현재 데이터로 갱신합니다.
        /// </summary>
        /// <param name="context">현재 전투 데이터입니다.</param>
        public void RefreshAll(TcgBattleDataMain context)
        {
            if (!IsReady || context == null) return;

            var player = context.Player;
            var enemy  = context.Enemy;

            _handPlayer.RefreshHand(player);
            _handEnemy.RefreshHand(enemy);
            _fieldPlayer.RefreshField(player);
            _fieldEnemy.RefreshField(enemy);

            _handPlayer.SetMana(player.Mana.Current, player.Mana.Max);
            _handEnemy.SetMana(enemy.Mana.Current, enemy.Mana.Max);

            // 턴 제한 정보(HUD)
            if (_battleHud != null)
            {
                var maxTurns = _ctx?.Settings != null ? _ctx.Settings.maxTurns : 0;
                _battleHud.RefreshRemainTurnCount(context.TurnCount, maxTurns);
            }
        }

        /// <summary>
        /// 연출 재생 동안 사용자 입력을 잠그는 잠금 카운트를 증가시킵니다(중첩 잠금 지원).
        /// </summary>
        private void BeginInteractionLock()
        {
            _interactionLockDepth++;
            if (_interactionLockDepth == 1)
            {
                // _battleHud?.SetInteractionLocked(true);
                SceneGame.Instance.bgBlackForMapLoading.SetActive(true);
            }
        }

        /// <summary>
        /// 사용자 입력 잠금 카운트를 감소시키고, 0이 되면 잠금을 해제합니다.
        /// </summary>
        private void EndInteractionLock()
        {
            if (_interactionLockDepth <= 0)
                return;

            _interactionLockDepth--;
            if (_interactionLockDepth == 0)
            {
                // _battleHud?.SetInteractionLocked(false);
                SceneGame.Instance.bgBlackForMapLoading.SetActive(false);
            }
        }

        /// <summary>
        /// 사용자 입력 잠금을 강제로 해제하고, 잠금 카운트를 0으로 초기화합니다.
        /// </summary>
        /// <remarks>
        /// StopCoroutine 등으로 인해 finally 경로가 실행되지 않았을 때 잠금 상태 복구 용도로 사용합니다.
        /// </remarks>
        public void ResetInteractionLock()
        {
            _interactionLockDepth = 0;
            // _battleHud?.SetInteractionLocked(false);
            SceneGame.Instance.bgBlackForMapLoading.SetActive(false);
        }

        /// <summary>
        /// 트레이스 목록에 연출 Step이 하나라도 포함되어 있는지 확인합니다.
        /// </summary>
        /// <param name="traces">확인할 커맨드 트레이스 목록입니다.</param>
        /// <returns>연출이 하나라도 있으면 true, 없으면 false입니다.</returns>
        private static bool HasAnyPresentation(IReadOnlyList<TcgBattleCommandTrace> traces)
        {
            if (traces == null || traces.Count == 0)
                return false;

            for (int i = 0; i < traces.Count; i++)
            {
                if (traces[i].Result != null && traces[i].Result.HasPresentation)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 씬 전환 등으로 컨트롤러를 더 이상 사용하지 않을 때, 코루틴/구독/윈도우 참조를 해제합니다.
        /// </summary>
        public void Release()
        {
            ResetInteractionLock();

            if (_battleHud != null)
            {
                if (_presentationCoroutine != null)
                    _battleHud.StopCoroutine(_presentationCoroutine);
            }

            _presentationCoroutine = null;

            // 구독 해제(필요 시 Dispose로 완전 해제 가능)
            _disposables.Clear(); // 또는 _disposables.Dispose();

            _fieldEnemy?.Release();
            _fieldPlayer?.Release();
            _handPlayer?.Release();
            _handEnemy?.Release();
            _battleHud?.Release();

            _fieldEnemy = null;
            _fieldPlayer = null;
            _handPlayer = null;
            _handEnemy = null;
            _battleHud = null;

            _session = null;
            _runner = null;
            _ctx = null;
        }
    }
}
