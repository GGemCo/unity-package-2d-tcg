using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// TCG 패키지의 런타임 진입점을 담당하는 매니저입니다.
    /// - 싱글톤 인스턴스로 유지되며(필요 시 DontDestroyOnLoad),
    /// - TCG에서 사용하는 하위 매니저들을 생성/초기화하고,
    /// - <see cref="SceneGame"/> 생명주기에 맞춰 정리(Destroy)됩니다.
    /// </summary>
    public class TcgPackageManager : MonoBehaviour
    {
        /// <summary>
        /// 현재 활성화된 TCG 패키지 매니저 인스턴스입니다.
        /// </summary>
        public static TcgPackageManager Instance { get; private set; }

        /// <summary>
        /// TCG 저장 데이터 관리 매니저입니다.
        /// 런타임에 생성되며 인스펙터에는 노출되지 않습니다.
        /// </summary>
        [HideInInspector] public SaveDataManagerTcg saveDataManagerTcg;

        /// <summary>
        /// TCG 전투 흐름을 제어하는 매니저입니다.
        /// </summary>
        public TcgBattleManager battleManager;

        /// <summary>
        /// 초기화 진입점입니다.
        /// 테이블 시스템이 준비되지 않은 경우 초기화를 진행하지 않습니다.
        /// 싱글톤으로 동작하도록 인스턴스를 보장하고, 하위 매니저를 초기화합니다.
        /// </summary>
        private void Awake()
        {
            // 테이블 매니저가 로드되지 않았다면 TCG 패키지를 초기화할 수 없습니다.
            // (의존성: 테이블 데이터가 없으면 설정/규칙/콘텐츠 로딩이 불가능할 수 있음)
            if (TableLoaderManager.Instance == null)
            {
                return;
            }

            // 싱글톤 보장:
            // - 최초 인스턴스는 DontDestroyOnLoad로 유지
            // - 이후 생성된 중복 인스턴스는 즉시 파괴
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

        /// <summary>
        /// TCG에서 사용하는 런타임 하위 매니저(GameObject 기반)를 생성하고 참조를 설정합니다.
        /// </summary>
        private void InitializeManagers()
        {
            // TCG 전용 매니저들을 담는 컨테이너 오브젝트입니다.
            // NOTE: 현재 컨테이너는 DontDestroyOnLoad로 별도 설정하지 않으며,
            //       부모-자식 관계상 본 오브젝트가 파괴되면 함께 정리됩니다.
            GameObject managerContainer = new GameObject("GGemCoManagersTcg");

            // 저장 데이터 매니저 생성
            saveDataManagerTcg = CreateManager<SaveDataManagerTcg>(managerContainer);
        }

        /// <summary>
        /// 지정한 타입의 매니저 컴포넌트를 자식 GameObject로 생성해 반환합니다.
        /// </summary>
        /// <typeparam name="T">생성할 매니저 컴포넌트 타입입니다.</typeparam>
        /// <param name="parent">생성된 오브젝트가 소속될 부모 오브젝트입니다.</param>
        /// <returns>추가된 매니저 컴포넌트 인스턴스입니다.</returns>
        private T CreateManager<T>(GameObject parent) where T : Component
        {
            GameObject obj = new GameObject(typeof(T).Name);
            obj.transform.SetParent(parent.transform);
            return obj.AddComponent<T>();
        }

        /// <summary>
        /// 씬이 시작된 이후 전투 매니저를 생성/초기화하고,
        /// <see cref="SceneGame"/> 파괴 이벤트에 연결하여 정리 타이밍을 맞춥니다.
        /// </summary>
        private void Start()
        {
            // SceneGame이 존재할 때만 파괴 콜백을 연결합니다.
            // SceneGame이 파괴되면(예: 게임 씬 종료) 본 매니저도 함께 정리합니다.
            if (SceneGame.Instance)
                SceneGame.Instance.OnSceneGameDestroyed += OnDestroyBySceneGame;

            // 전투 매니저는 MonoBehaviour가 아닌 순수 클래스(new)로 생성합니다.
            battleManager = new TcgBattleManager();

            // SceneGame의 시스템 메시지 매니저를 주입하여 전투 중 메시지/알림을 표시합니다.
            battleManager.Initialize(this, SceneGame.Instance.systemMessageManager);
        }

        /// <summary>
        /// <see cref="SceneGame"/>이 파괴될 때 호출되며, 패키지 매니저를 함께 종료합니다.
        /// </summary>
        private void OnDestroyBySceneGame()
        {
            Destroy(gameObject);
        }

        /// <summary>
        /// 오브젝트가 파괴될 때 이벤트 구독을 해제하여 누수를 방지합니다.
        /// </summary>
        private void OnDestroy()
        {
            if (SceneGame.Instance)
                SceneGame.Instance.OnSceneGameDestroyed -= OnDestroyBySceneGame;
        }
    }
}
