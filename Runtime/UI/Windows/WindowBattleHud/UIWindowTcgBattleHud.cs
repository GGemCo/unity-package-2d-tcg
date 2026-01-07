using System.Collections;
using GGemCo2DCore;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace GGemCo2DTcg
{
    public class UIWindowTcgBattleHud : UIWindow
    {
        [Header(UIWindowConstants.TitleHeaderIndividual)]
        [Tooltip("남은 시간")]
        [SerializeField] 
        private LocalizeStringEvent localizeRemainingTime;
        [Tooltip("남은 턴수")]
        [SerializeField] 
        private LocalizeStringEvent localizeRemainingTurns;
        [Tooltip("전투 강제 종료 버튼")]
        public Button buttonBattleExit;
        
        [Tooltip("턴 종료 버튼")]
        public Button buttonTurnOff;
        
        [Header("Turn End Text")]
        [Tooltip("턴이 종료되고, 플레이어 턴이 되었을 때 보여주는 텍스트")]
        public GameObject gameObjectEndTurn;
        public TMP_Text textTurnStartSide;
        public float fadeInDuration = 0.6f;
        public float holdDuration = 1.5f;
        public float fadeOutDuration = 0.6f;

        private TcgBattleManager _battleManager;
        // ------------------------------
        // Turn Timer
        // ------------------------------
        private Coroutine _turnTimerCoroutine;
        private int _turnTimeLimitSeconds;
        private int _remainingSeconds;
        
        // localization
        private string _localizationPlayerTurn;
        private string _localizationEnemyTurn;
        // Arguments 배열을 재사용해서 매번 new를 피합니다.
        private readonly object[] _timeArgs = new object[2];
        private readonly object[] _turnArgs = new object[1];
        
        protected override void Awake()
        {
            base.Awake();
            
            buttonBattleExit?.onClick.AddListener(OnClickBattleExit);
            gameObjectEndTurn?.SetActive(false);
            // Player 전용 처리
            buttonTurnOff?.onClick.AddListener(OnClickTurnOff);
            
            if (localizeRemainingTime != null)
                localizeRemainingTime.StringReference.Arguments = _timeArgs;

            if (localizeRemainingTurns != null)
                localizeRemainingTurns.StringReference.Arguments = _turnArgs;
        }
        protected void OnDestroy()
        {
            buttonBattleExit?.onClick.RemoveAllListeners();
            buttonTurnOff?.onClick.RemoveAllListeners();
            StopTurnTimer();
        }

        protected override void Start()
        {
            base.Start();
            _battleManager = TcgPackageManager.Instance.battleManager;

            _localizationPlayerTurn = LocalizationManager.Instance.GetUIWindowTcgBattleHudByKey("Text_TurnPlayer");
            _localizationEnemyTurn = LocalizationManager.Instance.GetUIWindowTcgBattleHudByKey("Text_TurnEnemy");
        }

        private void OnClickBattleExit()
        {
            _battleManager?.EndBattleForce();
        }

        public void Release()
        {
            StopTurnTimer();
        }
        public IEnumerator ShowEndTurnText(ConfigCommonTcg.TcgPlayerSide side)
        {
            if (gameObjectEndTurn == null) yield break;
            
            gameObjectEndTurn.SetActive(true);
            var fadeOption = UiFadeSequenceUtility.FadeSequenceOptions.Default;
            fadeOption.startAlpha = 0f;
            fadeOption.fadeIn.easeType  = Easing.EaseType.EaseOutQuad;
            fadeOption.fadeOut.easeType = Easing.EaseType.EaseInQuad;
            fadeOption.fadeOut.disableInputWhenInvisible = true;
            fadeOption.fadeOut.updateInteractableOnComplete = true;
            fadeOption.fadeOut.updateBlocksRaycastsOnComplete = true;

            if (textTurnStartSide != null)
                textTurnStartSide.text = side == ConfigCommonTcg.TcgPlayerSide.Player ? _localizationPlayerTurn : _localizationEnemyTurn;

            yield return UiFadeSequenceUtility.FadeInHoldFadeOut(this, gameObjectEndTurn,
                fadeInDuration, holdDuration,
                fadeOutDuration, fadeOption, true);
        }

        /// <summary>
        /// 턴 종료 버튼 클릭 시 전투 매니저에 턴 종료를 요청합니다.
        /// </summary>
        private void OnClickTurnOff()
        {
            _battleManager?.OnUiRequestEndTurn();
        }
        /// <summary>
        /// 턴 제한 시간(초)을 설정하고 타이머를 시작합니다.
        /// - 0 이하이면 타이머를 표시하지 않습니다.
        /// </summary>
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
        /// 현재 타이머를 중지하고 코루틴을 해제합니다.
        /// </summary>
        private void StopTurnTimer()
        {
            if (_turnTimerCoroutine == null) return;
            StopCoroutine(_turnTimerCoroutine);
            _turnTimerCoroutine = null;
        }
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
            _battleManager.OnUiRequestEndTurn();
        }

        private void SetTimeText(int totalSeconds)
        {
            if (localizeRemainingTime == null) return;
            
            if (totalSeconds < 0) totalSeconds = 0;

            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            _timeArgs[0] = minutes; // 테이블에서 {0:00}
            _timeArgs[1] = seconds; // 테이블에서 {1:00}

            localizeRemainingTime.RefreshString();
        }

        private void SetRemainTurnCount(int remainTurns, int maxTurns)
        {
            if (localizeRemainingTurns == null)
                return;

            if (remainTurns < 0 || maxTurns <= 0) remainTurns = 0;
            
            _turnArgs[0] = remainTurns;

            localizeRemainingTurns.RefreshString();
        }

        public void RefreshRemainTurnCount(int currentTurnCount, int maxTurns)
        {
            if (maxTurns <= 0)
            {
                SetRemainTurnCount(0, maxTurns);
                return;
            }

            // 남은 턴 = maxTurns - TurnCount
            var remain = maxTurns - currentTurnCount;
            SetRemainTurnCount(remain, maxTurns);
        }
    }
}