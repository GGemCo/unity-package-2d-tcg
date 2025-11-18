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

        private UIWindowCardCollection _windowCardCollection;

        protected override void Awake()
        {
            base.Awake();
            buttonCollection?.onClick.AddListener(OnClickCollection);
        }

        protected override void Start()
        {
            base.Start();
            _windowCardCollection =
                SceneGame.uIWindowManager.GetUIWindowByUid<UIWindowCardCollection>(UIWindowConstants.WindowUid
                    .TcgCardCollection);
        }

        private void OnClickCollection()
        {
            _windowCardCollection.Show(true);
        }
    }
}