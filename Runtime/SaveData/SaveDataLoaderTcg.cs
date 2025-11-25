using GGemCo2DCore;
using Newtonsoft.Json;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 세이브 데이터 json 파일 로드
    /// </summary>
    public class SaveDataLoaderTcg : SaveDataLoaderBase
    {
        public static SaveDataLoaderTcg Instance { get; private set; }
        
        private SaveDataContainerTcg _saveDataContainerTcg;
        
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
        protected override string GetSaveFilePath(int slotIndex)
        {
            return saveFileController.GetSaveFilePath(slotIndex, "SaveDataTcg");
        }
        /// <summary>
        /// 바로 해제를 위해 추가
        /// </summary>
        private void OnDestroy()
        {
            _saveDataContainerTcg = null;
        }

        protected override void OnLoaded(string json) 
        {
            _saveDataContainerTcg = JsonConvert.DeserializeObject<SaveDataContainerTcg>(json);
        }
        public SaveDataContainerTcg GetSaveDataContainer()
        {
            return _saveDataContainerTcg;
        }
    }
}