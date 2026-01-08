using GGemCo2DCore;
using Newtonsoft.Json;

namespace GGemCo2DTcg
{
    /// <summary>
    /// TCG 전용 세이브 데이터 JSON 파일을 로드하는 로더 클래스입니다.
    /// </summary>
    /// <remarks>
    /// SaveDataLoaderBase를 상속하며,
    /// 슬롯 기반 세이브 파일을 JSON 문자열로 로드한 뒤
    /// <see cref="SaveDataContainerTcg"/> 객체로 역직렬화합니다.
    /// </remarks>
    public class SaveDataLoaderTcg : SaveDataLoaderBase
    {
        /// <summary>
        /// SaveDataLoaderTcg 싱글톤 인스턴스입니다.
        /// </summary>
        public static SaveDataLoaderTcg Instance { get; private set; }

        /// <summary>
        /// 로드된 TCG 세이브 데이터 컨테이너입니다.
        /// </summary>
        private SaveDataContainerTcg _saveDataContainerTcg;

        /// <summary>
        /// TCG 세이브 파일 이름(슬롯 경로 생성 시 사용)입니다.
        /// </summary>
        private const string SaveFileName = "SaveDataTcg";

        /// <summary>
        /// Unity Awake 단계에서 싱글톤을 초기화합니다.
        /// </summary>
        /// <remarks>
        /// - 최초 생성된 인스턴스는 유지됩니다.
        /// - 중복 생성된 인스턴스는 즉시 제거됩니다.
        /// </remarks>
        protected override void Awake()
        {
            base.Awake();

            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 슬롯 인덱스를 기준으로 세이브 파일 전체 경로를 반환합니다.
        /// </summary>
        /// <param name="slotIndex">세이브 슬롯 인덱스입니다.</param>
        /// <returns>세이브 파일 경로 문자열입니다.</returns>
        protected override string GetSaveFilePath(int slotIndex)
        {
            return saveFileController.GetSaveFilePath(slotIndex, SaveFileName);
        }

        /// <summary>
        /// 오브젝트가 파괴될 때 로드된 세이브 데이터를 해제합니다.
        /// </summary>
        /// <remarks>
        /// 즉시 참조 해제를 통해 불필요한 메모리 참조를 방지합니다.
        /// </remarks>
        private void OnDestroy()
        {
            _saveDataContainerTcg = null;
        }

        /// <summary>
        /// JSON 문자열 로드 완료 시 호출되어 세이브 데이터를 역직렬화합니다.
        /// </summary>
        /// <param name="json">세이브 파일에서 읽어온 JSON 문자열입니다.</param>
        /// <remarks>
        /// Newtonsoft.Json을 사용하여 <see cref="SaveDataContainerTcg"/>로 변환합니다.
        /// </remarks>
        protected override void OnLoaded(string json)
        {
            _saveDataContainerTcg = JsonConvert.DeserializeObject<SaveDataContainerTcg>(json);
        }

        /// <summary>
        /// 현재 로드되어 있는 TCG 세이브 데이터 컨테이너를 반환합니다.
        /// </summary>
        /// <returns>
        /// 로드된 <see cref="SaveDataContainerTcg"/> 인스턴스입니다.
        /// 아직 로드되지 않았다면 null일 수 있습니다.
        /// </returns>
        public SaveDataContainerTcg GetSaveDataContainer()
        {
            return _saveDataContainerTcg;
        }
    }
}
