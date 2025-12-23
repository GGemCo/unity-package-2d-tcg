using System.Collections;
using GGemCo2DCore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GGemCo2DTcg
{
    public class UIWindowTcgBattleHud : UIWindow
    {
        [Header(UIWindowConstants.TitleHeaderIndividual)]
        public Button buttonBattleExit;
        
        [Header("Turn End Text")]
        [Tooltip("턴이 종료되고, 플레이어 턴이 되었을 때 보여주는 텍스트")]
        public GameObject gameObjectEndTurn;
        public float fadeInDuration = 0.6f;
        public float holdDuration = 1.5f;
        public float fadeOutDuration = 0.6f;
        
        private UIWindowTcgFieldEnemy _uiWindowTcgFieldEnemy;
        private UIWindowTcgFieldPlayer _uiWindowTcgFieldPlayer;

        private TcgBattleManager _battleManager;
        protected override void Awake()
        {
            base.Awake();
            
            buttonBattleExit?.onClick.AddListener(OnClickBattleExit);
            
            gameObjectEndTurn?.SetActive(false);
        }
        protected void OnDestroy()
        {
            buttonBattleExit?.onClick.RemoveAllListeners();
        }

        protected override void Start()
        {
            base.Start();
            _uiWindowTcgFieldEnemy = SceneGame.uIWindowManager.GetUIWindowByUid<UIWindowTcgFieldEnemy>(UIWindowConstants.WindowUid.TcgFieldEnemy);
            _uiWindowTcgFieldPlayer = SceneGame.uIWindowManager.GetUIWindowByUid<UIWindowTcgFieldPlayer>(UIWindowConstants.WindowUid.TcgFieldPlayer);
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
            fadeOption.fadeIn.easingFunc  = Easing.EaseOutQuad;
            fadeOption.fadeOut.easingFunc = Easing.EaseInQuad;

            yield return UiFadeSequenceUtility.FadeInHoldFadeOut(this, gameObjectEndTurn,
                fadeInDuration, holdDuration,
                fadeOutDuration, fadeOption, true);
        }
    }
}