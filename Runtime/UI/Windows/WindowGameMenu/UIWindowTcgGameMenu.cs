using GGemCo2DCore;
using UnityEngine;
using UnityEngine.UI;

namespace GGemCo2DTcg
{
    public class UIWindowTcgGameMenu : UIWindow
    {
        [Header(UIWindowConstants.TitleHeaderIndividual)]
        [Tooltip("콜랙션 보기 버튼")]
        public Button buttonCollection;
        [Tooltip("대결 시작 버튼")]
        public Button buttonBattleStart;

        private UIWindowTcgCardCollection _windowTcgCardCollection;

        protected override void Awake()
        {
            base.Awake();
            buttonCollection?.onClick.AddListener(OnClickCollection);
            buttonBattleStart?.onClick.AddListener(OnClickBattleStart);
        }

        protected override void Start()
        {
            base.Start();
            _windowTcgCardCollection =
                SceneGame.uIWindowManager.GetUIWindowByUid<UIWindowTcgCardCollection>(UIWindowConstants.WindowUid
                    .TcgCardCollection);
        }

        private void OnClickCollection()
        {
            _windowTcgCardCollection.Show(true);
        }
        private void OnClickBattleStart()
        {
            if (TcgPackageManager.Instance.battleManager == null)
            {
                GcLogger.LogError($"{nameof(TcgBattleManager)} 클래스가 생성되지 않았습니다.");
                return;
            }
            TcgPackageManager.Instance.battleManager.StartBattle();
        }
    }
}