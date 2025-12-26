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

        // ------------------------------
        // Ability Presentation (AbilityType별 UI 연출)
        // ------------------------------
        private readonly Queue<TcgAbilityPresentationEvent> _abilityFxQueue = new Queue<TcgAbilityPresentationEvent>(16);
        private Coroutine _abilityFxCoroutine;
        private System.Action<TcgAbilityPresentationEvent> _onAbilityPresentation;

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

            // 기존 세션 구독 해제
            if (_session != null && _onAbilityPresentation != null)
                _session.AbilityPresentation -= _onAbilityPresentation;

            _session = session;
            _handPlayer.SetBattleManager(manager);

            // Ability 실행 이벤트 구독(AbilityType별 UI 연출용)
            _onAbilityPresentation = OnAbilityPresentation;
            _session.AbilityPresentation += _onAbilityPresentation;

            // 연출 핸들러들이 참조할 UI/세션 묶음 컨텍스트
            _ctx = new TcgPresentationContext(_session, _fieldEnemy, _fieldPlayer, _handPlayer, _handEnemy, _battleHud, settings);

            // 커맨드 결과의 PresentationSteps를 타입별 핸들러로 순차 실행
            _runner = new TcgPresentationRunner(new ITcgPresentationHandler[]
            {
                new HandlerMoveCardHandToBoard(),
                new HandlerMoveCardHandToGrave(),
                new HandlerAttackUnit(),
                new HandlerDeathFadeOut(),
                new HandlerAttackHero(),
                new HandlerEndTurn(),
            });
        }

        private void OnAbilityPresentation(TcgAbilityPresentationEvent evt)
        {
            // 실제 "효과"가 끝난 뒤 시점에만 UI 연출을 재생하는 것이 일반적으로 자연스럽습니다.
            if (evt.EventPhase != TcgAbilityPresentationEvent.Phase.End)
                return;

            if (_battleHud == null)
                return;

            _abilityFxQueue.Enqueue(evt);
            if (_abilityFxCoroutine == null)
                _abilityFxCoroutine = _battleHud.StartCoroutine(CoPlayAbilityFx());
        }

        private IEnumerator CoPlayAbilityFx()
        {
            while (_battleHud != null && _abilityFxQueue.Count > 0)
            {
                var evt = _abilityFxQueue.Dequeue();

                // 커맨드 연출과 동일한 입력 차단 체계를 재사용(중첩 가능)
                BeginInteractionLock();
                try
                {
                    // HUD에 위임(프리팹에서 연결되어 있으면 실제 연출 실행)
                    if (_battleHud.gameObjectAbilityPresentation != null)
                    {
                        yield return _battleHud.ShowAbilityTypePresentation(evt);
                    }
                    else
                    {
                        yield return GetDefaultAbilityFxWait(evt.AbilityType);
                    }
                }
                finally
                {
                    EndInteractionLock();
                }
            }

            _abilityFxCoroutine = null;
        }

        private static IEnumerator GetDefaultAbilityFxWait(TcgAbilityConstants.TcgAbilityType abilityType)
        {
            // 기본값은 "최소 대기"만 제공(프리팹/연출이 연결되면 HUD에서 실제 연출 재생)
            float seconds = abilityType switch
            {
                TcgAbilityConstants.TcgAbilityType.Damage => 0.25f,
                TcgAbilityConstants.TcgAbilityType.Heal => 0.20f,
                TcgAbilityConstants.TcgAbilityType.Draw => 0.15f,
                TcgAbilityConstants.TcgAbilityType.GainMana => 0.15f,
                TcgAbilityConstants.TcgAbilityType.ExtraAction => 0.20f,
                _ => 0.12f
            };

            yield return new WaitForSeconds(seconds);
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
                    perStepEnded: () => _session.TryCheckBattleEnd()
                );
            }
            // 정상적으로 코루틴이 실행되거나 yield break; 가 호출되면 finally 가 호출 됩니다. StopCoroutine 으로 종료하면 finally 가 호출되지 않습니다. 
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

        public void ResetInteractionLock()
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

            if (_battleHud != null)
            {
                if (_presentationCoroutine != null) _battleHud.StopCoroutine(_presentationCoroutine);
                if (_abilityFxCoroutine != null) _battleHud.StopCoroutine(_abilityFxCoroutine);
            }

            _presentationCoroutine = null;
            _abilityFxCoroutine = null;
            _abilityFxQueue.Clear();

            if (_session != null && _onAbilityPresentation != null)
                _session.AbilityPresentation -= _onAbilityPresentation;
            _onAbilityPresentation = null;
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
