using System.Collections;
using System.Collections.Generic;
using GGemCo2DCore;
using R3;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 전투 세션(<see cref="TcgBattleSession"/>)과 전투 UI 윈도우들을 연결하고,
    /// 연출 재생 후 UI를 갱신/해제하는 코디네이터.
    /// </summary>
    /// <remarks>
    /// - <see cref="TrySetupWindows"/>로 윈도우 참조를 확보한 뒤
    /// - <see cref="BindBattleManager"/>로 세션/매니저와 연출 러너를 결합하고
    /// - <see cref="PlayPresentationAndRefresh"/>로 커맨드 트레이스 기반 연출을 순차 재생합니다.
    /// </remarks>
    public sealed class TcgBattleUiController
    {
        private UIWindowTcgFieldEnemy  _fieldEnemy;
        private UIWindowTcgFieldPlayer _fieldPlayer;
        private UIWindowTcgHandPlayer  _handPlayer;
        private UIWindowTcgHandEnemy   _handEnemy;
        private UIWindowTcgBattleHud   _battleHud;

        /// <summary>
        /// UI 이벤트/바인딩 등에서 사용하는 구독 해제 컨테이너.
        /// </summary>
        private readonly CompositeDisposable _disposables = new();

        private TcgBattleSession _session;
        private Coroutine _presentationCoroutine;

        private TcgPresentationRunner _runner;
        private TcgPresentationContext _ctx;

        /// <summary>
        /// 전투 UI 구성 요소(필드/핸드/HUD)가 모두 준비되었는지 여부.
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

        /// <summary>현재 연출 재생으로 인해 인터렉션이 잠겨 있는지 여부.</summary>
        public bool IsInteractionLocked => _interactionLockDepth > 0;

        /// <summary>현재 커맨드 연출 코루틴이 재생 중인지 여부.</summary>
        public bool IsPresenting => _presentationCoroutine != null;
        
        /// <summary>
        /// SceneGame의 <see cref="UIWindowManager"/>에서 전투 관련 윈도우를 찾아 캐싱합니다.
        /// </summary>
        /// <returns>필요한 모든 윈도우를 찾았으면 true, 일부라도 누락되면 false.</returns>
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
        /// 전투 UI 윈도우 일괄 활성/비활성 처리.
        /// </summary>
        /// <param name="isShow">true면 표시, false면 숨김.</param>
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
        /// <param name="manager">전투 로직/명령을 제공하는 매니저.</param>
        /// <param name="session">현재 전투 세션.</param>
        /// <param name="settings">TCG 설정.</param>
        public void BindBattleManager(TcgBattleManager manager, TcgBattleSession session, GGemCoTcgSettings settings)
        {
            if (!IsReady || manager == null || session == null) return;

            _session = session;
            _handPlayer.SetBattleManager(manager);

            // 연출 핸들러들이 참조할 UI/세션 묶음 컨텍스트
            _ctx = new TcgPresentationContext(_session, _fieldEnemy, _fieldPlayer, _handPlayer, _handEnemy, _battleHud, settings);

            // 커맨드 결과의 PresentationSteps를 타입별 핸들러로 순차 실행
            _runner = new TcgPresentationRunner(new ITcgPresentationHandler[]
            {
                new HandlerDrawCard(),
                new HandlerAttackUnit(),
                new HandlerDeathFadeOut(),
                new HandlerAttackHero(),
                new HandlerEndTurn(),
            });
        }

        /// <summary>
        /// <paramref name="traces"/>의 커맨드 연출을 순차 재생한 뒤, 최종 상태로 모든 UI를 갱신합니다.
        /// </summary>
        /// <param name="context">최종 UI를 구성할 전투 데이터(플레이어/적 상태 포함).</param>
        /// <param name="traces">커맨드 실행 결과(연출 스텝 포함) 목록.</param>
        public void PlayPresentationAndRefresh(TcgBattleDataMain context, IReadOnlyList<TcgBattleCommandTrace> traces)
        {
            if (!IsReady || context == null)
                return;

            // 연출이 없다면 입력 차단 없이 즉시 최종 UI로 동기화
            if (!HasAnyPresentation(traces))
            {
                RefreshAll(context);
                return;
            }
            
            // 연출 실행에 필요한 구성요소가 없으면, 연출 없이 즉시 최종 UI로 동기화
            if (_battleHud == null || _runner == null || _ctx == null)
            {
                RefreshAll(context);
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

            _presentationCoroutine = _battleHud.StartCoroutine(CoPlayPresentation(context, traces));
        }

        /// <summary>
        /// 커맨드 트레이스의 연출 스텝을 실행하고, 전투 종료 조건을 체크하며,
        /// 종료/중단 시 UI를 최종 상태로 갱신합니다.
        /// </summary>
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
                    perStepEnded: () => _ctx.CheckBattleEndAndStop(_battleHud, ref _presentationCoroutine)
                );
            }
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
        /// <param name="context">현재 전투 데이터.</param>
        public void RefreshAll(TcgBattleDataMain context)
        {
            if (!IsReady || context == null) return;
            
            // GcLogger.Log($"{nameof(TcgBattleUiController)} RefreshAll");

            var player = context.Player;
            var enemy  = context.Enemy;

            _handPlayer.RefreshHand(player);
            _handEnemy.RefreshHand(enemy);
            _fieldPlayer.RefreshBoard(player);
            _fieldEnemy.RefreshBoard(enemy);
            // _battleHud.Refresh(context);

            _handPlayer.SetMana(player.Mana.Current, player.Mana.Max);
            _handEnemy.SetMana(enemy.Mana.Current, enemy.Mana.Max);
        }
        private void BeginInteractionLock()
        {
            _interactionLockDepth++;
            if (_interactionLockDepth == 1)
            {
                // _battleHud?.SetInteractionLocked(true);
                SceneGame.Instance.bgBlackForMapLoading.SetActive(true);
            }
        }

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
        private void ResetInteractionLock()
        {
            _interactionLockDepth = 0;
            // _battleHud?.SetInteractionLocked(false);
            SceneGame.Instance.bgBlackForMapLoading.SetActive(false);
        }
        
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
        /// 씬 전환 등으로 컨트롤러를 더 이상 사용하지 않을 때, 모든 참조/구독을 해제합니다.
        /// </summary>
        public void Release()
        {
            ResetInteractionLock();
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
