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

        [Header("Ability Presentation")]
        [Tooltip("Ability 연출용 오브젝트(선택). 연결되어 있으면 Ability 실행 후 Fade In→Hold→Fade Out 로 표시합니다.")]
        public GameObject gameObjectAbilityPresentation;

        [Tooltip("Ability 연출에 사용할 텍스트(선택). 연결되면 AbilityType에 맞는 문구를 표시합니다.")]
        public TMP_Text textAbilityPresentation;

        [Tooltip("Ability 연출 Fade In 시간(초). 0 이면 TurnEnd 설정값을 사용합니다.")]
        public float abilityFadeInDuration = 0.25f;

        [Tooltip("Ability 연출 Hold 시간(초). 0 이면 TurnEnd 설정값을 사용합니다.")]
        public float abilityHoldDuration = 0.65f;

        [Tooltip("Ability 연출 Fade Out 시간(초). 0 이면 TurnEnd 설정값을 사용합니다.")]
        public float abilityFadeOutDuration = 0.25f;
        
        private UIWindowTcgFieldEnemy _uiWindowTcgFieldEnemy;
        private UIWindowTcgFieldPlayer _uiWindowTcgFieldPlayer;

        private TcgBattleManager _battleManager;
        protected override void Awake()
        {
            base.Awake();
            
            buttonBattleExit?.onClick.AddListener(OnClickBattleExit);
            
            gameObjectEndTurn?.SetActive(false);
            gameObjectAbilityPresentation?.SetActive(false);
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
            fadeOption.fadeIn.easeType  = Easing.EaseType.EaseOutQuad;
            fadeOption.fadeOut.easeType = Easing.EaseType.EaseInQuad;

            yield return UiFadeSequenceUtility.FadeInHoldFadeOut(this, gameObjectEndTurn,
                fadeInDuration, holdDuration,
                fadeOutDuration, fadeOption, true);
        }

        /// <summary>
        /// Ability 처리 후 재생되는 UI 연출.
        /// - 프리팹에서 <see cref="gameObjectAbilityPresentation"/>이 연결되어 있지 않으면 null을 반환합니다.
        /// - UIController는 null일 경우 기본 대기(짧은 Wait)만 수행합니다.
        /// </summary>
        public IEnumerator ShowAbilityTypePresentation(TcgAbilityPresentationEvent evt)
        {
            if (gameObjectAbilityPresentation == null)
                yield break;

            // 텍스트가 연결되어 있으면 AbilityType에 맞는 문구를 표시
            if (textAbilityPresentation != null)
            {
                textAbilityPresentation.text = GetAbilityPresentationText(evt.AbilityType);
            }

            gameObjectAbilityPresentation.SetActive(true);

            var fadeOption = UiFadeSequenceUtility.FadeSequenceOptions.Default;
            fadeOption.startAlpha = 0f;
            fadeOption.fadeIn.easeType  = Easing.EaseType.EaseOutQuad;
            fadeOption.fadeOut.easeType = Easing.EaseType.EaseInQuad;

            float inDur = abilityFadeInDuration > 0f ? abilityFadeInDuration : fadeInDuration;
            float holdDur = abilityHoldDuration > 0f ? abilityHoldDuration : holdDuration;
            float outDur = abilityFadeOutDuration > 0f ? abilityFadeOutDuration : fadeOutDuration;

            yield return UiFadeSequenceUtility.FadeInHoldFadeOut(this, gameObjectAbilityPresentation,
                inDur, holdDur, outDur, fadeOption, true);
        }

        private static string GetAbilityPresentationText(TcgAbilityConstants.TcgAbilityType type)
        {
            return type switch
            {
                TcgAbilityConstants.TcgAbilityType.Damage => "DAMAGE",
                TcgAbilityConstants.TcgAbilityType.Heal => "HEAL",
                TcgAbilityConstants.TcgAbilityType.Draw => "DRAW",
                TcgAbilityConstants.TcgAbilityType.BuffAttack => "BUFF ATTACK",
                TcgAbilityConstants.TcgAbilityType.BuffHealth => "BUFF HEALTH",
                TcgAbilityConstants.TcgAbilityType.GainMana => "GAIN MANA",
                TcgAbilityConstants.TcgAbilityType.ExtraAction => "EXTRA ACTION",
                _ => type.ToString().ToUpperInvariant()
            };
        }
    }
}