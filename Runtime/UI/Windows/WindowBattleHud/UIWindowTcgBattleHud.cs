using GGemCo2DCore;
using UnityEngine.UI;

namespace GGemCo2DTcg
{
    public class UIWindowTcgBattleHud : UIWindow
    {
        public Button buttonTurnOff;
        public Button buttonBattleExit;

        private UIWindowTcgField _uiWindowTcgField;
        private TcgBattleManager _battleManager;
        protected override void Awake()
        {
            base.Awake();
            
            buttonTurnOff?.onClick.AddListener(OnClickTurnOff);
            buttonBattleExit?.onClick.AddListener(OnClickBattleExit);
        }
        protected void OnDestroy()
        {
            buttonTurnOff?.onClick.RemoveAllListeners();
            buttonBattleExit?.onClick.RemoveAllListeners();
        }

        protected override void Start()
        {
            base.Start();
            _uiWindowTcgField = SceneGame.uIWindowManager.GetUIWindowByUid<UIWindowTcgField>(UIWindowConstants.WindowUid.TcgField);
            _battleManager = TcgPackageManager.Instance.battleManager;
        }

        private void OnClickBattleExit()
        {
            _uiWindowTcgField?.Show(false);
        }

        private void OnClickTurnOff()
        {
            if (_battleManager == null)
                return;

            _battleManager.OnUiRequestEndTurn(ConfigCommonTcg.TcgPlayerSide.Player);
        }
    }
}