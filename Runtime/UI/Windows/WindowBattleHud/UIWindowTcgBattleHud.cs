using GGemCo2DCore;
using UnityEngine;
using UnityEngine.UI;

namespace GGemCo2DTcg
{
    public class UIWindowTcgBattleHud : UIWindow
    {
        [Header(UIWindowConstants.TitleHeaderIndividual)]
        public Button buttonBattleExit;

        private UIWindowTcgFieldEnemy _uiWindowTcgFieldEnemy;
        private UIWindowTcgFieldPlayer _uiWindowTcgFieldPlayer;

        private TcgBattleManager _battleManager;
        protected override void Awake()
        {
            base.Awake();
            
            buttonBattleExit?.onClick.AddListener(OnClickBattleExit);
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
            _battleManager.EndBattleForce();
        }

        public void Release()
        {
        }
    }
}