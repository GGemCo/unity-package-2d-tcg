using System.Collections.Generic;
using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// TCG 로딩 씬에서 초기 로딩 스텝(테이블/리소스/세이브/로컬라이징)을 등록하는 씬 클래스입니다.
    /// </summary>
    /// <remarks>
    /// GameLoaderManager의 로딩 시작 직전 이벤트에 구독하여,
    /// 필요한 매니저/로더 오브젝트를 생성(또는 탐색)하고 로딩 스텝을 등록합니다.
    /// </remarks>
    public class SceneLoadingTcg : DefaultScene
    {
        private GameLoaderManager _gameLoaderManager;

        /// <summary>
        /// Addressable 설정이 준비되지 않은 경우 PreIntro 씬으로 되돌립니다.
        /// </summary>
        /// <remarks>
        /// AddressableLoaderSettings 인스턴스가 없는 상태에서 로딩을 진행하면
        /// 이후 로딩 단계에서 실패할 가능성이 높으므로 선제적으로 복구합니다.
        /// </remarks>
        private void Awake()
        {
            if (!AddressableLoaderSettings.Instance)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(ConfigDefine.SceneNamePreIntro);
                return;
            }
        }

        /// <summary>
        /// 오브젝트 활성화 시, 로딩 시작 직전 이벤트에 구독합니다.
        /// </summary>
        private void OnEnable()
        {
            // PreIntro 씬/Loading 씬에서 로딩 시작 직전 훅
            GameLoaderManager.BeforeLoadStartInLoadingScene += OnBeforeLoadStartInLoadingScene;
        }

        /// <summary>
        /// 오브젝트 비활성화 시, 이벤트 구독을 해제합니다.
        /// </summary>
        private void OnDisable()
        {
            GameLoaderManager.BeforeLoadStartInLoadingScene -= OnBeforeLoadStartInLoadingScene;
        }

        /// <summary>
        /// 로딩 시작 직전에 호출되어, TCG에서 필요한 로딩 스텝을 GameLoaderManager에 등록합니다.
        /// </summary>
        /// <param name="sender">로딩을 진행하는 <see cref="GameLoaderManager"/>입니다.</param>
        /// <param name="e">로딩 시작 이벤트 인자입니다.</param>
        /// <remarks>
        /// 등록되는 스텝 예:
        /// - TCG 테이블 로드
        /// - 카드 관련 Addressable 프리팹 로드
        /// - TCG 세이브 데이터 로드
        /// - 로컬라이징 데이터 로드 및 로케일 적용
        /// </remarks>
        private void OnBeforeLoadStartInLoadingScene(GameLoaderManager sender, GameLoaderManager.EventArgsBeforeLoadStart e)
        {
            // 1) 테이블 로더 준비 및 테이블 로딩 스텝 등록
            var tableLoader =
                FindFirstObjectByType<TableLoaderManagerTcg>() ??
                new GameObject("TableLoaderManagerTcg").AddComponent<TableLoaderManagerTcg>();

            var targetTables = ConfigAddressableTableTcg.All;
            var tableLoadStep = new TableLoadStep(
                id: "core.table.tcg",
                order: 245,
                localizedKey: LocalizationConstants.Keys.Loading.TextTypeTables(),
                tableLoader: tableLoader,
                tables: targetTables
            );
            sender.Register(tableLoadStep);

            // 2) 카드 Addressable 로더 준비 및 프리팹 로딩 스텝 등록
            var addressableLoaderCard =
                Object.FindFirstObjectByType<AddressableLoaderCard>() ??
                new GameObject("AddressableLoaderCard").AddComponent<AddressableLoaderCard>();

            var cardLoadStep = new AddressableTaskStep(
                id: "core.image.card.art",
                order: 400,
                localizedKey: LocalizationConstants.Keys.Loading.TextTypePrefab(),
                startTask: () => addressableLoaderCard.LoadPrefabsAsync(),
                getProgress: () => addressableLoaderCard.GetPrefabLoadProgress()
            );
            sender.Register(cardLoadStep);

            // 3) 세이브 데이터 로더 준비 및 세이브 데이터 로딩 스텝 등록
            var saveDataLoaderTcg =
                Object.FindFirstObjectByType<SaveDataLoaderTcg>() ??
                new GameObject("SaveDataLoaderTcg").AddComponent<SaveDataLoaderTcg>();

            var saveDataLoadStep = new SaveDataLoadStep(
                "core.savedata.tcg",
                order: 381,
                localizedKey: LocalizationConstants.Keys.Loading.TextTypeSaveData(),
                saveDataLoader: saveDataLoaderTcg
            );
            sender.Register(saveDataLoadStep);

            // 4) 로컬라이징 매니저 준비 및 로컬라이징 로딩 스텝 등록
            var loc =
                Object.FindFirstObjectByType<LocalizationManagerTcg>() ??
                new GameObject("LocalizationManagerTcg").AddComponent<LocalizationManagerTcg>();

            var localizationLoadStep = new LocalizationLoadStep(
                "core.localization.tcg",
                order: 221,
                localizedKey: LocalizationConstants.Keys.Loading.TextTypeLocalization(),
                localizationManager: loc,
                localeCode: PlayerPrefsManager.LoadLocalizationLocaleCode()
            );
            sender.Register(localizationLoadStep);
        }

        /// <summary>
        /// 씬 시작 시점 훅입니다. (현재는 사용하지 않습니다)
        /// </summary>
        private void Start()
        {
        }
    }
}
