using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    public class TcgPackageManager : MonoBehaviour
    {
        public static TcgPackageManager Instance { get; private set; }

        public SaveDataManagerTcg saveDataManagerTcg;
        public TcgBattleManager battleManager;
        
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
            InitializeManagers();
        }

        private void InitializeManagers()
        {
            GameObject managerContainer = new GameObject("ManagersTcg");
            saveDataManagerTcg = CreateManager<SaveDataManagerTcg>(managerContainer);

            battleManager = new TcgBattleManager();
            battleManager.Initialize(this);
        }

        private T CreateManager<T>(GameObject parent) where T : Component
        {
            GameObject obj = new GameObject(typeof(T).Name);
            obj.transform.SetParent(parent.transform);
            return obj.AddComponent<T>();
        }
        private void Start()
        {
            if (SceneGame.Instance)
                SceneGame.Instance.OnSceneGameDestroyed += OnDestroyBySceneGame;
            battleManager.InitializeByStart();
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