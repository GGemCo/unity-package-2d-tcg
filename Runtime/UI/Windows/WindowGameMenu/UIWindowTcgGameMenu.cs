using GGemCo2DCore;
using UnityEngine;
using UnityEngine.UI;

namespace GGemCo2DTcg
{
    /// <summary>
    /// TCG 게임 메뉴 UI 윈도우입니다.
    /// 콜렉션(카드 목록) 화면을 열거나 전투를 시작하는 진입 버튼들을 제공합니다.
    /// </summary>
    public class UIWindowTcgGameMenu : UIWindow
    {
        [Header(UIWindowConstants.TitleHeaderIndividual)]
        [Tooltip("콜랙션 보기 버튼")]
        public Button buttonCollection;

        [Tooltip("대결 시작 버튼")]
        public Button buttonBattleStart;

        /// <summary>
        /// 콜렉션 UI 윈도우 캐시입니다. Start에서 UIWindowManager를 통해 조회합니다.
        /// </summary>
        private UIWindowTcgCardCollection _windowTcgCardCollection;

        /// <summary>
        /// 윈도우 초기화 시 버튼 클릭 이벤트를 등록합니다.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            buttonCollection?.onClick.AddListener(OnClickCollection);
            buttonBattleStart?.onClick.AddListener(OnClickBattleStart);
        }

        /// <summary>
        /// 시작 시점에 콜렉션 윈도우 참조를 캐싱합니다.
        /// </summary>
        protected override void Start()
        {
            base.Start();
            _windowTcgCardCollection =
                SceneGame.uIWindowManager.GetUIWindowByUid<UIWindowTcgCardCollection>(
                    UIWindowConstants.WindowUid.TcgCardCollection);
        }

        /// <summary>
        /// 콜렉션(카드 목록) 화면을 표시합니다.
        /// </summary>
        private void OnClickCollection()
        {
            _windowTcgCardCollection.Show(true);
        }

        /// <summary>
        /// 전투 시작 버튼 클릭 시 전투 파라미터를 구성해 전투를 시작합니다.
        /// 에디터 환경에서는 테스트 시드가 설정되어 있으면 해당 값을 사용합니다.
        /// </summary>
        private void OnClickBattleStart()
        {
            if (TcgPackageManager.Instance.battleManager == null)
            {
                GcLogger.LogError($"{nameof(TcgBattleManager)} 클래스가 생성되지 않았습니다.");
                return;
            }

            // 기본 전투 파라미터 구성
            TcgBattleMetaData paramsBattle = new TcgBattleMetaData
            {
                playerDeckIndex = TcgPackageManager.Instance.saveDataManagerTcg.PlayerTcg.defaultDeckIndex,
                enemyDeckPresetId = 0,
                initialSeed = 0,
                isPlayerFirst = true
            };

#if UNITY_EDITOR
            // 에디터 테스트용 시드가 유효하면 우선 적용
            if (AddressableLoaderSettingsTcg.Instance.tcgSettings &&
                AddressableLoaderSettingsTcg.Instance.tcgSettings.testSeed > 0)
            {
                paramsBattle.initialSeed = AddressableLoaderSettingsTcg.Instance.tcgSettings.testSeed;
            }
#endif

            TcgPackageManager.Instance.battleManager.StartBattle(paramsBattle);
        }
    }
}
