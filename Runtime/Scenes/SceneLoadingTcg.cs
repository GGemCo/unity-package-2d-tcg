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
            var targetTables = new List<AddressableAssetInfo> { ConfigAddressableTableTcg.TableTcgCard };
            var tableLoadStep = new TableLoadStep(
                id: "core.table.card",
                order: 245,
                localizedKey: LocalizationConstants.Keys.Loading.TextTypeTables(),
                tableLoader: tableLoader,
                tables: targetTables
            );
            sender.Register(tableLoadStep);
        }

        private void Start()
        {
        }

    }
}