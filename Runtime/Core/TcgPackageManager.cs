using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    public class TcgPackageManager : MonoBehaviour
    {
        public static TcgPackageManager Instance { get; private set; }
        
        private void Awake()
        {
            // 테이블 매니저가 로드 되지 않았다면 return;
            if (TableLoaderManager.Instance == null)
            {
                return;
            }
            // 싱글톤으로 사용하기.
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            // gameObject.AddComponent<BootstrapperCharacterSpawn>();
            // gameObject.AddComponent<BootstrapperMap>();
            // wetDecaySystem = gameObject.AddComponent<WetDecaySystem>();
            // simulationDirtyTracker = gameObject.AddComponent<SimulationDirtyTracker>();
            //
            // // Core에 저장 기여자 등록
            // simulationSaveContributor = new SimulationSaveContributor(simulationDirtyTracker, this);
        }
        private void Start()
        {
            if (SceneGame.Instance)
                SceneGame.Instance.OnSceneGameDestroyed += OnDestroyBySceneGame;
        }
        private void OnDestroyBySceneGame()
        {
            Destroy(gameObject);
        }
        private void OnDestroy()
        {
            if (SceneGame.Instance) 
                SceneGame.Instance.OnSceneGameDestroyed -= OnDestroyBySceneGame;
        }

    }
}