using System.Collections.Generic;
using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    public class SceneLoadingTcg : DefaultScene
    {
        private GameLoaderManager _gameLoaderManager;
        
        private void Awake()
        {
            if (!AddressableLoaderSettings.Instance)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(ConfigDefine.SceneNamePreIntro);
                return;
            }
            
        }

        private void OnEnable()
        {
            // Pre 인트로 씬에서 로딩 시작 전
            // Loading 씬에서 로딩 시작 전
            GameLoaderManager.BeforeLoadStartInLoadingScene += OnBeforeLoadStartInLoadingScene;
        }
        private void OnDisable()
        {
            GameLoaderManager.BeforeLoadStartInLoadingScene -= OnBeforeLoadStartInLoadingScene;
        }
        private void OnBeforeLoadStartInLoadingScene(GameLoaderManager sender, GameLoaderManager.EventArgsBeforeLoadStart e)
        {
            var tableLoader = FindFirstObjectByType<TableLoaderManagerTcg>() ?? new GameObject("TableLoaderManagerTcg").AddComponent<TableLoaderManagerTcg>();
            var targetTables = ConfigAddressableTableTcg.All;
            var tableLoadStep = new TableLoadStep(
                id: "core.table.tcg",
                order: 245,
                localizedKey: LocalizationConstants.Keys.Loading.TextTypeTables(),
                tableLoader: tableLoader,
                tables: targetTables
            );
            sender.Register(tableLoadStep);
            
            var addressableLoaderCard = Object.FindFirstObjectByType<AddressableLoaderCard>() ?? new GameObject("AddressableLoaderCard").AddComponent<AddressableLoaderCard>();
            var cardLoadStep = new AddressableTaskStep(
                id: "core.image.card.art",
                order: 400,
                localizedKey: LocalizationConstants.Keys.Loading.TextTypePrefab(),
                startTask: () => addressableLoaderCard.LoadPrefabsAsync(),
                getProgress: () => addressableLoaderCard.GetPrefabLoadProgress()
            );
            sender.Register(cardLoadStep);
            
            var saveDataLoaderTcg = Object.FindFirstObjectByType<SaveDataLoaderTcg>() ?? new GameObject("SaveDataLoaderTcg").AddComponent<SaveDataLoaderTcg>();
            var saveDataLoadStep = new SaveDataLoadStep(
                "core.savedata.tcg",
                order: 381,
                localizedKey: LocalizationConstants.Keys.Loading.TextTypeSaveData(),
                saveDataLoader: saveDataLoaderTcg
            );
            sender.Register(saveDataLoadStep);
            
            var loc = Object.FindFirstObjectByType<LocalizationManagerTcg>() ?? new GameObject("LocalizationManagerTcg").AddComponent<LocalizationManagerTcg>();
            var localizationLoadStep = new LocalizationLoadStep(
                "core.localization.tcg",
                order: 221,
                localizedKey: LocalizationConstants.Keys.Loading.TextTypeLocalization(),
                localizationManager: loc,
                localeCode: PlayerPrefsManager.LoadLocalizationLocaleCode()
            );
            sender.Register(localizationLoadStep);
            
        }

        private void Start()
        {
        }

    }
}