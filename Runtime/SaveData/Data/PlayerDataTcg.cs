using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// TCG 플레이어의 기본 설정 정보를 저장하는 세이브 데이터 클래스입니다.
    /// </summary>
    /// <remarks>
    /// 현재는 기본 덱 인덱스(defaultDeckIndex)만 관리하며,
    /// 향후 플레이어 관련 추가 설정(예: 선호 덱, 튜토리얼 상태 등)이 확장될 수 있습니다.
    /// </remarks>
    public class PlayerDataTcg : DefaultData, ISaveData
    {
        /// <summary>
        /// 플레이어의 기본 덱 인덱스입니다.
        /// </summary>
        /// <remarks>
        /// 덱 인덱스는 0부터 시작하므로, 미설정 상태를 의미하기 위해 기본값은 -1입니다.
        /// </remarks>
        public int defaultDeckIndex = -1;

        /// <summary>
        /// 플레이어 데이터를 초기화합니다.
        /// </summary>
        /// <param name="loader">TCG 테이블 로더입니다. (현재 클래스에서는 직접 사용하지 않습니다)</param>
        /// <param name="saveDataContainer">
        /// 저장된 데이터 컨테이너입니다.
        /// null 이거나 데이터가 없을 경우 기본값으로 유지됩니다.
        /// </param>
        /// <remarks>
        /// 저장된 데이터가 존재하면 해당 데이터를 불러와 적용합니다.
        /// </remarks>
        public void Initialize(TableLoaderManagerTcg loader, SaveDataContainerTcg saveDataContainer = null)
        {
            LoadPlayerData(saveDataContainer);
        }

        /// <summary>
        /// 플레이어 데이터를 저장 요청합니다.
        /// </summary>
        /// <remarks>
        /// 즉시 저장을 보장하지 않으며, SaveDataManager를 통해 저장 프로세스를 시작합니다.
        /// </remarks>
        private void SavePlayerData()
        {
            SceneGame.Instance.saveDataManager.StartSaveData();
        }

        /// <summary>
        /// 저장된 플레이어 데이터를 불러와 현재 인스턴스에 적용합니다.
        /// </summary>
        /// <param name="saveDataContainer">저장된 세이브 데이터 컨테이너입니다.</param>
        private void LoadPlayerData(SaveDataContainerTcg saveDataContainer)
        {
            if (saveDataContainer?.PlayerDataTcg != null)
            {
                defaultDeckIndex = saveDataContainer.PlayerDataTcg.defaultDeckIndex;
            }
        }

        /// <summary>
        /// 플레이어 데이터는 슬롯 개념을 사용하지 않으므로 최대 슬롯 수는 0을 반환합니다.
        /// </summary>
        /// <returns>항상 0을 반환합니다.</returns>
        protected override int GetMaxSlotCount()
        {
            return 0;
        }

        /// <summary>
        /// DefaultData 인터페이스 요구사항에 따라 저장 처리를 수행합니다.
        /// </summary>
        protected override void SaveDatas()
        {
            TcgPackageManager.Instance.saveDataManagerTcg.StartSaveData();
        }

        /// <summary>
        /// 플레이어의 기본 덱 인덱스를 설정합니다.
        /// </summary>
        /// <param name="deckIndex">설정할 기본 덱 인덱스입니다.</param>
        /// <returns>설정에 성공하면 true를 반환합니다.</returns>
        /// <remarks>
        /// 덱 인덱스의 유효성 검사는 호출 측(MyDeckData 등)에서 수행하는 것을 전제로 합니다.
        /// </remarks>
        public bool SetDefaultDeckIndex(int deckIndex)
        {
            defaultDeckIndex = deckIndex;
            SaveDatas();
            return true;
        }
    }
}
