using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 세이브 데이터 - TCG 플레이어 정보
    /// </summary>
    public class PlayerDataTcg : DefaultData, ISaveData
    {
        // 0 부터 시작이기 때문에, 디폴트는 -1
        public int defaultDeckIndex = -1;

        /// <summary>
        /// 초기화 (저장된 데이터를 불러오거나 새로운 데이터 생성)
        /// </summary>
        public void Initialize(TableLoaderManagerTcg loader, SaveDataContainerTcg saveDataContainer = null)
        {
            // 저장된 데이터가 있을 경우 불러오기
            LoadPlayerData(saveDataContainer);
        }

        /// <summary>
        /// 데이터 저장
        /// </summary>
        private void SavePlayerData()
        {
            SceneGame.Instance.saveDataManager.StartSaveData();
        }

        /// <summary>
        /// 저장된 데이터를 불러와서 적용
        /// </summary>
        private void LoadPlayerData(SaveDataContainerTcg saveDataContainer)
        {
            if (saveDataContainer?.PlayerDataTcg != null)
            {
                defaultDeckIndex = saveDataContainer.PlayerDataTcg.defaultDeckIndex;
            }
        }
        
        protected override int GetMaxSlotCount()
        {
            return 0;
        }

        protected override void SaveDatas()
        {
            TcgPackageManager.Instance.saveDataManagerTcg.StartSaveData();
        }
        public bool SetDefaultDeckIndex(int deckIndex)
        {
            defaultDeckIndex = deckIndex;
            SaveDatas();
            return true;
        }
    }
}
