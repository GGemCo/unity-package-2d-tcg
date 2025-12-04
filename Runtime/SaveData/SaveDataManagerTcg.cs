using System.IO;
using GGemCo2DCore;
using Newtonsoft.Json;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 저장할 데이터 컨테이너 클래스
    /// </summary>
    public class SaveDataContainerTcg
    {
        public MyDeckData MyDeckData;
        public PlayerDataTcg PlayerDataTcg;
    }
    /// <summary>
    /// 세이브 데이터 메인 매니저
    /// </summary>
    public class SaveDataManagerTcg : SaveDataManagerBase
    {
        public MyDeckData MyDeck { get; private set; }
        public PlayerDataTcg PlayerTcg { get; private set; }

        private PlayerData _playerData;
        
        /// <summary>
        /// 로드한 세이브 데이터로 초기화 하기
        /// </summary>
        protected override void InitializeData()
        {
            // 로드한 세이브 데이터 가져오기 
            SaveDataContainerTcg saveDataContainer = SaveDataLoaderTcg.Instance.GetSaveDataContainer();
            
            // 각 데이터 클래스 초기화
            MyDeck = new MyDeckData();
            PlayerTcg = new PlayerDataTcg();

            // 초기화 실행
            MyDeck.Initialize(TableLoaderManagerTcg.Instance, saveDataContainer);
            PlayerTcg.Initialize(TableLoaderManagerTcg.Instance, saveDataContainer);
        }

        protected override void Start()
        {
            base.Start();
            _playerData = SceneGame.Instance.saveDataManager.Player;
        }

        /// <summary>
        /// 현재 데이터를 선택한 슬롯에 저장 + 메타파일 업데이트
        /// </summary>
        public override bool SaveData()
        {
            if (!base.SaveData()) return false;
            
            string filePath = saveFileController.GetSaveFilePath(currentSaveSlot, "SaveDataTcg");
            string thumbnailPath = thumbnailController.GetThumbnailPath(currentSaveSlot);

            SaveDataContainerTcg saveData = new SaveDataContainerTcg
            {
                PlayerDataTcg = PlayerTcg,
                MyDeckData = MyDeck,
            };

            string json = JsonConvert.SerializeObject(saveData);
            File.WriteAllText(filePath, json);
            // GcLogger.Log($"데이터가 저장되었습니다. 슬롯 {currentSaveSlot}");
            
            // 썸네일 캡처 후 저장
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