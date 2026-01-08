using System.Collections;
using GGemCo2DCore;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 전투(Battle) HUD UI 윈도우입니다.
    /// - 남은 시간/남은 턴수 로컬라이즈 텍스트를 갱신하고,
    /// - 전투 강제 종료/턴 종료 버튼 입력을 전투 매니저에 전달하며,
    /// - 턴 시작(또는 턴 전환) 안내 텍스트를 페이드 시퀀스로 표시합니다.
    /// </summary>
    public class UIWindowTcgBattleHud : UIWindow
    {
        [Header(UIWindowConstants.TitleHeaderIndividual)]
        [Tooltip("남은 시간(로컬라이즈 문자열 이벤트)")]
        [SerializeField]
        private LocalizeStringEvent localizeRemainingTime;

        [Tooltip("남은 턴수(로컬라이즈 문자열 이벤트)")]
        [SerializeField]
        private LocalizeStringEvent localizeRemainingTurns;

        [Tooltip("전투 강제 종료 버튼")]
        public Button buttonBattleExit;

        [Tooltip("턴 종료 버튼(플레이어 전용)")]
        public Button buttonTurnOff;

        [Header("Turn End Text")]
        [Tooltip("턴이 종료되고 다음 턴이 시작될 때 표시하는 안내 오브젝트")]
        public GameObject gameObjectEndTurn;

        [Tooltip("현재 턴의 주체(Player/Enemy) 텍스트")]
        public TMP_Text textTurnStartSide;

        [Tooltip("페이드 인 시간(초)")]
        public float fadeInDuration = 0.6f;

        [Tooltip("표시 유지 시간(초)")]
        public float holdDuration = 1.5f;

        [Tooltip("페이드 아웃 시간(초)")]
        public float fadeOutDuration = 0.6f;

        /// <summary>
        /// 전투 진행을 제어하는 매니저 참조입니다.
        /// </summary>
        private TcgBattleManager _battleManager;

        // ------------------------------
        // Turn Timer
        // ------------------------------

        /// <summary>
        /// 턴 타이머 코루틴 핸들입니다.
        /// </summary>
        private Coroutine _turnTimerCoroutine;

        /// <summary>
        /// 턴 제한 시간(초)입니다. 0 이하면 타이머를 사용하지 않습니다.
        /// </summary>
        private int _turnTimeLimitSeconds;

        /// <summary>
        /// 현재 남은 시간(초)입니다.
        /// </summary>
        private int _remainingSeconds;

        // localization

        /// <summary>
        /// "플레이어 턴" 표시용 로컬라이즈 문자열입니다.
        /// </summary>
        private string _localizationPlayerTurn;

        /// <summary>
        /// "적 턴" 표시용 로컬라이즈 문자열입니다.
        /// </summary>
        private string _localizationEnemyTurn;

        /// <summary>
        /// LocalizeStringEvent의 Arguments 배열을 재사용하여 GC 할당을 줄입니다.
        /// - 시간: {0}(분), {1}(초)
        /// </summary>
        private readonly object[] _timeArgs = new object[2];

        /// <summary>
        /// LocalizeStringEvent의 Arguments 배열을 재사용하여 GC 할당을 줄입니다.
        /// - 남은 턴: {0}
        /// </summary>
        private readonly object[] _turnArgs = new object[1];

        /// <summary>
        /// 컴포넌트 초기화 시 호출됩니다.
        /// 버튼 리스너를 등록하고, 로컬라이즈 이벤트 Arguments를 주입합니다.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            buttonBattleExit?.onClick.AddListener(OnClickBattleExit);

            // 턴 안내 텍스트는 기본 비활성으로 시작
            gameObjectEndTurn?.SetActive(false);

            // Player 전용 처리
            buttonTurnOff?.onClick.AddListener(OnClickTurnOff);

            // LocalizeStringEvent가 RefreshString() 시 사용할 Arguments 배열을 미리 연결
            if (localizeRemainingTime != null)
                localizeRemainingTime.StringReference.Arguments = _timeArgs;

            if (localizeRemainingTurns != null)
                localizeRemainingTurns.StringReference.Arguments = _turnArgs;
        }

        /// <summary>
        /// 오브젝트 파괴 시 호출됩니다.
        /// 버튼 리스너와 코루틴을 정리하여 참조 누수 및 중복 호출을 방지합니다.
        /// </summary>
        protected void OnDestroy()
        {
            buttonBattleExit?.onClick.RemoveAllListeners();
            buttonTurnOff?.onClick.RemoveAllListeners();
            StopTurnTimer();
        }

        /// <summary>
        /// 첫 프레임 시작 시 호출됩니다.
        /// 전투 매니저 및 턴 안내 텍스트(로컬라이즈 문자열)를 캐싱합니다.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            _battleManager = TcgPackageManager.Instance.battleManager;

            _localizationPlayerTurn =
                LocalizationManager.Instance.GetUIWindowTcgBattleHudByKey("Text_TurnPlayer");
            _localizationEnemyTurn =
                LocalizationManager.Instance.GetUIWindowTcgBattleHudByKey("Text_TurnEnemy");
        }

        /// <summary>
        /// 전투 강제 종료 버튼 클릭 시 호출됩니다.
        /// 전투 매니저에 강제 종료를 요청합니다.
        /// </summary>
        private void OnClickBattleExit()
        {
            _battleManager?.EndBattleForce();
        }

        /// <summary>
        /// 외부에서 HUD 사용 종료 시 호출할 수 있는 정리 메서드입니다.
        /// 현재 실행 중인 턴 타이머를 중지합니다.
        /// </summary>
        public void Release()
        {
            StopTurnTimer();
        }

        /// <summary>
        /// 턴 전환 안내 텍스트를 페이드 인/홀드/페이드 아웃 시퀀스로 표시합니다.
        /// </summary>
        /// <param name="side">현재 턴의 주체(Player/Enemy)</param>
        /// <returns>페이드 시퀀스가 완료될 때까지 대기하는 코루틴 IEnumerator</returns>
        public IEnumerator ShowEndTurnText(ConfigCommonTcg.TcgPlayerSide side)
        {
            if (gameObjectEndTurn == null) yield break;

            gameObjectEndTurn.SetActive(true);

            var fadeOption = UiFadeSequenceUtility.FadeSequenceOptions.Default;
            fadeOption.startAlpha = 0f;
            fadeOption.fadeIn.easeType = Easing.EaseType.EaseOutQuad;
            fadeOption.fadeOut.easeType = Easing.EaseType.EaseInQuad;

            // 페이드 아웃 완료 시 입력/레이캐스트 상태를 정리
            fadeOption.fadeOut.disableInputWhenInvisible = true;
            fadeOption.fadeOut.updateInteractableOnComplete = true;
            fadeOption.fadeOut.updateBlocksRaycastsOnComplete = true;

            if (textTurnStartSide != null)
            {
                textTurnStartSide.text =
                    side == ConfigCommonTcg.TcgPlayerSide.Player
                        ? _localizationPlayerTurn
                        : _localizationEnemyTurn;
            }

            yield return UiFadeSequenceUtility.FadeInHoldFadeOut(
                this,
                gameObjectEndTurn,
                fadeInDuration,
                holdDuration,
                fadeOutDuration,
                fadeOption,
                true);
        }

        /// <summary>
        /// 턴 종료 버튼 클릭 시 호출됩니다.
        /// 전투 매니저에 턴 종료를 요청합니다.
        /// </summary>
        private void OnClickTurnOff()
        {
            _battleManager?.OnUiRequestEndTurn();
        }

        /// <summary>
        /// 턴 제한 시간(초)을 설정하고 타이머를 시작합니다.
        /// 0 이하이면 타이머를 중지하고 0초로 표시합니다.
        /// </summary>
        /// <param name="turnTimeLimitSeconds">턴 제한 시간(초)</param>
        public void StartTurnTimer(int turnTimeLimitSeconds)
        {
            _turnTimeLimitSeconds = turnTimeLimitSeconds;

            if (_turnTimeLimitSeconds <= 0)
            {
                StopTurnTimer();
                SetTimeText(0);
                return;
            }

            _remainingSeconds = _turnTimeLimitSeconds;
            SetTimeText(_remainingSeconds);

            if (_turnTimerCoroutine != null)
            {
                StopCoroutine(_turnTimerCoroutine);
                _turnTimerCoroutine = null;
            }

            _turnTimerCoroutine = StartCoroutine(CoTurnTimer());
        }

        /// <summary>
        /// 현재 실행 중인 턴 타이머 코루틴을 중지하고 핸들을 해제합니다.
        /// </summary>
        private void StopTurnTimer()
        {
            if (_turnTimerCoroutine == null) return;
            StopCoroutine(_turnTimerCoroutine);
            _turnTimerCoroutine = null;
        }

        /// <summary>
        /// 턴 타이머 코루틴입니다.
        /// Time.timeScale의 영향을 받지 않도록 WaitForSecondsRealtime 기준으로 진행합니다.
        /// 시간이 0이 되면 전투 매니저에 턴 종료를 요청합니다.
        /// </summary>
        /// <returns>코루틴 IEnumerator</returns>
        private IEnumerator CoTurnTimer()
        {
            // UI 타이머는 Time.timeScale 영향을 받지 않도록 Realtime 기준으로 진행합니다.
            while (_remainingSeconds > 0)
            {
                yield return new WaitForSecondsRealtime(1f);
                _remainingSeconds--;
                SetTimeText(_remainingSeconds);
            }

            // 0초 표시까지 갱신
            SetTimeText(0);

            _turnTimerCoroutine = null;

            // NOTE: 타이머 만료 시 자동으로 턴 종료를 요청
            _battleManager?.OnUiRequestEndTurn();
        }

        /// <summary>
        /// 남은 시간을 "분/초" 인자로 로컬라이즈 문자열에 주입하고 갱신합니다.
        /// </summary>
        /// <param name="totalSeconds">남은 전체 시간(초)</param>
        private void SetTimeText(int totalSeconds)
        {
            if (localizeRemainingTime == null) return;

            if (totalSeconds < 0) totalSeconds = 0;

            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;

            // 로컬라이즈 테이블에서 {0:00}, {1:00} 형태로 포맷팅된다는 전제
            _timeArgs[0] = minutes;
            _timeArgs[1] = seconds;

            localizeRemainingTime.RefreshString();
        }

        /// <summary>
        /// 남은 턴 수를 로컬라이즈 문자열 인자로 주입하고 갱신합니다.
        /// </summary>
        /// <param name="remainTurns">남은 턴 수</param>
        /// <param name="maxTurns">최대 턴 수(검증 용도)</param>
        private void SetRemainTurnCount(int remainTurns, int maxTurns)
        {
            if (localizeRemainingTurns == null) return;

            if (remainTurns < 0 || maxTurns <= 0) remainTurns = 0;

            _turnArgs[0] = remainTurns;

            localizeRemainingTurns.RefreshString();
        }

        /// <summary>
        /// 현재 턴 카운트와 최대 턴 수를 기반으로 남은 턴 표시를 갱신합니다.
        /// </summary>
        /// <param name="currentTurnCount">현재까지 진행된 턴 수</param>
        /// <param name="maxTurns">최대 턴 수</param>
        public void RefreshRemainTurnCount(int currentTurnCount, int maxTurns)
        {
            if (maxTurns <= 0)
            {
                SetRemainTurnCount(0, maxTurns);
                return;
            }

            // 남은 턴 = maxTurns - currentTurnCount
            var remain = maxTurns - currentTurnCount;
            SetRemainTurnCount(remain, maxTurns);
        }
    }
}
