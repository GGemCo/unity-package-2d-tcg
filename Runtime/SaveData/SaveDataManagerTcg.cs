using System.IO;
using GGemCo2DCore;
using Newtonsoft.Json;

namespace GGemCo2DTcg
{
    /// <summary>
    /// TCG에서 저장/로드할 데이터를 한 묶음으로 담는 컨테이너 클래스입니다.
    /// </summary>
    /// <remarks>
    /// Save/Load 시 이 컨테이너 단위로 JSON 직렬화/역직렬화를 수행합니다.
    /// </remarks>
    public class SaveDataContainerTcg
    {
        /// <summary>
        /// 덱(Deck) 관련 저장 데이터입니다.
        /// </summary>
        public MyDeckData MyDeckData;

        /// <summary>
        /// 플레이어(기본 덱 설정 등) 관련 저장 데이터입니다.
        /// </summary>
        public PlayerDataTcg PlayerDataTcg;
    }

    /// <summary>
    /// TCG 세이브 데이터의 메인 매니저입니다.
    /// </summary>
    /// <remarks>
    /// - 로더로부터 데이터를 읽어 각 데이터 클래스를 초기화합니다.
    /// - 현재 선택된 슬롯에 데이터를 저장하고, 썸네일 및 슬롯 메타 파일을 갱신합니다.
    /// </remarks>
    public class SaveDataManagerTcg : SaveDataManagerBase
    {
        /// <summary>
        /// 덱(Deck) 데이터 관리자입니다.
        /// </summary>
        public MyDeckData MyDeck { get; private set; }

        /// <summary>
        /// 플레이어 TCG 데이터 관리자입니다.
        /// </summary>
        public PlayerDataTcg PlayerTcg { get; private set; }

        private PlayerData _playerData;

        /// <summary>
        /// 로드된 세이브 데이터(있다면)를 기반으로 각 데이터 클래스를 생성 및 초기화합니다.
        /// </summary>
        /// <remarks>
        /// 로드 데이터가 없으면 각 데이터 클래스는 기본값 상태로 초기화됩니다.
        /// </remarks>
        protected override void InitializeData()
        {
            // 로드한 세이브 데이터 가져오기
            SaveDataContainerTcg saveDataContainer = SaveDataLoaderTcg.Instance.GetSaveDataContainer();

            // 각 데이터 클래스 생성
            MyDeck = new MyDeckData();
            PlayerTcg = new PlayerDataTcg();

            // 로드 데이터 적용/초기화 실행
            MyDeck.Initialize(TableLoaderManagerTcg.Instance, saveDataContainer);
            PlayerTcg.Initialize(TableLoaderManagerTcg.Instance, saveDataContainer);
        }

        /// <summary>
        /// Unity Start 단계에서 베이스 초기화 이후 필요한 참조를 확보합니다.
        /// </summary>
        protected override void Start()
        {
            base.Start();
            _playerData = SceneGame.Instance.saveDataManager.Player;
        }

        /// <summary>
        /// 현재 데이터를 선택된 세이브 슬롯에 저장하고, 슬롯 메타 정보를 갱신합니다.
        /// </summary>
        /// <returns>저장에 성공하면 true, 실패하면 false입니다.</returns>
        /// <remarks>
        /// - <see cref="SaveDataContainerTcg"/>로 묶어 JSON으로 직렬화 후 파일로 기록합니다.
        /// - 썸네일 설정이 활성화된 경우 썸네일 캡처 코루틴을 시작합니다.
        /// - 메타 파일에 썸네일 경로/사용 여부/레벨/파일 경로 등을 업데이트합니다.
        /// </remarks>
        /// <exception cref="IOException">파일 쓰기 중 I/O 오류가 발생할 수 있습니다.</exception>
        public override bool SaveData()
        {
            if (!base.SaveData()) return false;

            string filePath = saveFileController.GetSaveFilePath(currentSaveSlot, "SaveDataTcg");
            string thumbnailPath = thumbnailController.GetThumbnailPath(currentSaveSlot);

            var saveData = new SaveDataContainerTcg
            {
                PlayerDataTcg = PlayerTcg,
                MyDeckData = MyDeck,
            };

            string json = JsonConvert.SerializeObject(saveData);
            File.WriteAllText(filePath, json);

            // 썸네일 캡처 후 저장(설정된 폭이 0보다 클 때만)
            if (thumbnailWidth > 0)
            {
                StartCoroutine(thumbnailController.CaptureThumbnail(currentSaveSlot));
            }

            // 메타파일 업데이트
            slotMetaDatController.UpdateSlot(currentSaveSlot, thumbnailPath, true, _playerData.CurrentLevel, filePath);
            return true;
        }
    }
}
