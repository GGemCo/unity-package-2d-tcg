using System.Collections.Generic;
using GGemCo2DCore;
using UnityEngine; // Mathf 사용

namespace GGemCo2DTcg
{
    /// <summary>
    /// TCG 전투 시작을 관리하는 메인 엔트리 클래스.
    /// - 저장된 덱/플레이어 데이터 검증
    /// - 셔플 컨텍스트 생성
    /// - 런타임 덱 생성 및 셔플
    /// - UI 윈도우 초기화
    /// </summary>
    public class TcgBattleManager
    {
        private TcgPackageManager _packageManager;
        private SaveDataManagerTcg _saveDataManagerTcg;

        private UIWindowTcgField _uiWindowTcgField;
        private UIWindowTcgMyHand _uiWindowTcgMyHand;

        /// <summary>
        /// TCG 패키지 매니저로부터 필수 의존성을 주입합니다.
        /// </summary>
        /// <param name="packageManager">TCG 패키지 매니저 인스턴스.</param>
        public void Initialize(TcgPackageManager packageManager)
        {
            _packageManager = packageManager;
            _saveDataManagerTcg = _packageManager?.saveDataManagerTcg;

            if (_packageManager == null)
            {
                GcLogger.LogError($"{nameof(TcgPackageManager)} 가 null 입니다. {nameof(TcgBattleManager)} 초기화 실패.");
            }

            if (_saveDataManagerTcg == null)
            {
                GcLogger.LogError($"{nameof(SaveDataManagerTcg)} 가 null 입니다. 저장 데이터를 사용할 수 없습니다.");
            }
        }

        /// <summary>
        /// 저장된 정보(기본 덱)를 기반으로 전투를 시작합니다.
        /// </summary>
        public void StartBattle()
        {
            // 1. 기본 의존성 및 저장 데이터 검증
            if (!ValidateCoreDependencies())
                return;

            var myDeckData = _saveDataManagerTcg.MyDeck;
            if (!ValidateMyDeckData(myDeckData))
                return;

            var playerDataTcg = _saveDataManagerTcg.PlayerTcg;
            if (!ValidatePlayerData(playerDataTcg))
                return;

            var deck = myDeckData.GetDeckInfoByIndex(playerDataTcg.defaultDeckIndex);
            if (!ValidateDeck(deck, playerDataTcg.defaultDeckIndex))
                return;

            // 2. 셔플 컨텍스트 생성
            var shuffleContext = BuildShuffleContext();

            // 3. 런타임 덱 생성 및 셔플
            List<CardRuntime> runtimeCardList = DeckBuilder.BuildRuntimeDeck(deck.cardList);
            var deckRuntime = new DeckRuntime<CardRuntime>(shuffleContext);
            deckRuntime.SetCards(runtimeCardList);
            deckRuntime.Shuffle();

#if UNITY_EDITOR
            // 에디터에서만 디버그 로그 출력
            LogShuffledDeckForDebug(deckRuntime);
#endif

            // 4. UI 윈도우 준비
            if (!EnsureBattleWindows())
                return;

            // 5. 윈도우에 초기 데이터 전달
            InitializeBattleWindows(deckRuntime);
        }

        #region Validation

        /// <summary>
        /// 필수 필드(패키지 매니저, 세이브 매니저)가 모두 유효한지 검사합니다.
        /// </summary>
        private bool ValidateCoreDependencies()
        {
            if (_packageManager == null)
            {
                GcLogger.LogError($"{nameof(TcgBattleManager)} 가 초기화되지 않았습니다. {nameof(TcgPackageManager)} 가 null 입니다.");
                return false;
            }

            if (_saveDataManagerTcg == null)
            {
                GcLogger.LogError($"{nameof(SaveDataManagerTcg)} 가 null 입니다. 저장 데이터를 사용할 수 없습니다.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// MyDeckData가 존재하고, 최소 1개 이상의 덱이 있는지 검사합니다.
        /// </summary>
        private bool ValidateMyDeckData(MyDeckData myDeckData)
        {
            if (myDeckData == null)
            {
                GcLogger.LogError($"{nameof(MyDeckData)} 클래스가 없습니다.");
                return false;
            }

            if (myDeckData.GetCurrentCount() <= 0)
            {
                GcLogger.LogError("저장된 덱 데이터가 없습니다.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 플레이어 TCG 데이터가 유효한지 검사합니다.
        /// </summary>
        private bool ValidatePlayerData(PlayerDataTcg playerDataTcg)
        {
            if (playerDataTcg == null)
            {
                GcLogger.LogError($"{nameof(PlayerDataTcg)} 클래스가 없습니다.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 기본 덱 정보와 카드 리스트가 유효한지 검사합니다.
        /// </summary>
        private bool ValidateDeck(MyDeckSaveData deck, int deckIndex)
        {
            if (deck == null)
            {
                GcLogger.LogError($"저장된 덱 정보가 없습니다. index: {deckIndex}");
            }
            else if (deck.cardList == null || deck.cardList.Count == 0)
            {
                GcLogger.LogError($"덱 안에 카드 정보가 없습니다. index: {deckIndex}");
            }
            else
            {
                return true;
            }

            return false;
        }

        #endregion

        #region Shuffle

        /// <summary>
        /// 설정값과 테스트 시드를 기반으로 셔플 메타데이터를 생성합니다.
        /// - 테스트 시드가 0보다 크면 고정 시드 사용
        /// - costWeights 설정이 있으면 Weighted 모드, 없으면 PureRandom 모드
        /// </summary>
        private ShuffleMetaData BuildShuffleContext()
        {
            // 1) 시드 설정
            var seedManager = CreateSeedManager();

            // 2) 셔플 모드 및 설정
            var shuffleSettings = AddressableLoaderSettingsTcg.Instance.tcgSettings;
            var shuffleMode = ConfigCommonTcg.ShuffleMode.PureRandom;
            var shuffleConfig = new ShuffleConfig();

            int[] costWeights = shuffleSettings.costWeights;

            // costWeights 에 값이 정의되어 있으면 Weighted 모드로 전환
            if (costWeights == null || costWeights.Length <= 0)
                return new ShuffleMetaData(shuffleMode, seedManager, shuffleConfig);
            
            shuffleMode = ConfigCommonTcg.ShuffleMode.Weighted;

            shuffleConfig.FrontLoadedCount = 10;

            if (shuffleConfig.CostWeights == null || shuffleConfig.CostWeights.Count <= 0)
                return new ShuffleMetaData(shuffleMode, seedManager, shuffleConfig);
            int length = Mathf.Min(costWeights.Length, shuffleConfig.CostWeights.Count);
            for (int i = 0; i < length; i++)
            {
                if (costWeights[i] <= 0) continue;
                shuffleConfig.CostWeights[i] = costWeights[i];
            }

            return new ShuffleMetaData(shuffleMode, seedManager, shuffleConfig);
        }

        /// <summary>
        /// 설정된 테스트 시드가 있으면 고정 시드를 사용하는 SeedManager를 생성합니다.
        /// </summary>
        private SeedManager CreateSeedManager()
        {
            var shuffleSettings = AddressableLoaderSettingsTcg.Instance.tcgSettings;
            int serverSeed = shuffleSettings.testSeed;

            if (serverSeed > 0)
                return new SeedManager(serverSeed);

            return new SeedManager();
        }

#if UNITY_EDITOR
        /// <summary>
        /// 에디터 환경에서 셔플된 덱 구성을 로그로 출력합니다.
        /// </summary>
        private void LogShuffledDeckForDebug(DeckRuntime<CardRuntime> deckRuntime)
        {
            if (deckRuntime == null)
                return;

            deckRuntime.DebugCard();
        }
#endif

        #endregion

        #region UI Windows

        /// <summary>
        /// 전투에 필요한 UI 윈도우(필드, 내 패)를 확보합니다.
        /// 없으면 로그를 남기고 false 를 반환합니다.
        /// </summary>
        private bool EnsureBattleWindows()
        {
            var windowManager = SceneGame.Instance?.uIWindowManager;
            if (windowManager == null)
            {
                GcLogger.LogError("SceneGame 또는 UIWindowManager 가 없습니다. 전투 UI를 열 수 없습니다.");
                return false;
            }

            if (_uiWindowTcgField == null)
            {
                _uiWindowTcgField = windowManager.GetUIWindowByUid<UIWindowTcgField>(UIWindowConstants.WindowUid.TcgField);
            }

            if (_uiWindowTcgField == null)
            {
                GcLogger.LogError($"{nameof(UIWindowTcgField)} 윈도우가 UI 매니저에 없습니다.");
                return false;
            }

            if (_uiWindowTcgMyHand == null)
            {
                _uiWindowTcgMyHand = windowManager.GetUIWindowByUid<UIWindowTcgMyHand>(UIWindowConstants.WindowUid.TcgMyHand);
            }

            if (_uiWindowTcgMyHand == null)
            {
                GcLogger.LogError($"{nameof(UIWindowTcgMyHand)} 윈도우가 UI 매니저에 없습니다.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// UI 윈도우를 표시하고, 초기 패 정보를 전달합니다.
        /// </summary>
        private void InitializeBattleWindows(DeckRuntime<CardRuntime> deckRuntime)
        {
            // 필드 윈도우 표시
            _uiWindowTcgField.Show(true);

            // 내 패 윈도우에 덱 런타임 전달 (첫 패 등 초기화)
            _uiWindowTcgMyHand.SetFirstCard(deckRuntime);
        }

        #endregion
    }
}
