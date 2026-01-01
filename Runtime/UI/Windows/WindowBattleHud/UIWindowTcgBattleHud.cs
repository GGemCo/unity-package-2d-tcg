using System.Collections;
using GGemCo2DCore;
using UnityEngine;
using UnityEngine.UI;

namespace GGemCo2DTcg
{
    public class UIWindowTcgBattleHud : UIWindow
    {
        [Header(UIWindowConstants.TitleHeaderIndividual)]
        [Tooltip("전투 강제 종료 버튼")]
        public Button buttonBattleExit;
        
        [Tooltip("턴 종료 버튼")]
        public Button buttonTurnOff;
        
        [Header("Turn End Text")]
        [Tooltip("턴이 종료되고, 플레이어 턴이 되었을 때 보여주는 텍스트")]
        public GameObject gameObjectEndTurn;
        public float fadeInDuration = 0.6f;
        public float holdDuration = 1.5f;
        public float fadeOutDuration = 0.6f;

        private TcgBattleManager _battleManager;
        protected override void Awake()
        {
            base.Awake();
            
            buttonBattleExit?.onClick.AddListener(OnClickBattleExit);
            gameObjectEndTurn?.SetActive(false);
            // Player 전용 처리
            buttonTurnOff?.onClick.AddListener(OnClickTurnOff);
        }
        protected void OnDestroy()
        {
            buttonBattleExit?.onClick.RemoveAllListeners();
            buttonTurnOff?.onClick.RemoveAllListeners();
        }

        protected override void Start()
        {
            base.Start();
            _battleManager = TcgPackageManager.Instance.battleManager;
        }

        private void OnClickBattleExit()
        {
            _battleManager?.EndBattleForce();
        }

        public void Release()
        {
        }
        public IEnumerator ShowEndTurnText()
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
    }
}