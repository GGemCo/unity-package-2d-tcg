using System.Collections.Generic;
using System.Linq;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 유저가 보유한 덱(Deck)의 저장 데이터 모델입니다.
    /// </summary>
    /// <remarks>
    /// 덱 이름, 영웅(히어로) 카드 UID, 덱에 포함된 카드 목록(카드 UID -> 수량)을 보관합니다.
    /// </remarks>
    public class MyDeckSaveData
    {
        /// <summary>
        /// 덱 슬롯 인덱스(0부터 시작)입니다.
        /// </summary>
        public int index;

        /// <summary>
        /// 덱 이름입니다.
        /// </summary>
        public readonly string deckName;

        /// <summary>
        /// 덱에 설정된 영웅(히어로) 카드 UID입니다. 미설정 시 0입니다.
        /// </summary>
        public int heroCardUid;

        /// <summary>
        /// 덱에 포함된 카드 목록입니다. (카드 UID -> 수량)
        /// </summary>
        public readonly Dictionary<int, int> cardList;

        /// <summary>
        /// 덱 저장 데이터를 생성합니다.
        /// </summary>
        /// <param name="index">덱 슬롯 인덱스(0부터 시작)입니다.</param>
        /// <param name="deckName">덱 이름입니다.</param>
        /// <param name="cardList">덱 카드 목록(카드 UID -> 수량)입니다.</param>
        /// <param name="heroCardUid">영웅(히어로) 카드 UID입니다. 미설정 시 0입니다.</param>
        public MyDeckSaveData(int index, string deckName, Dictionary<int, int> cardList, int heroCardUid)
        {
            this.index = index;
            this.deckName = deckName;
            this.cardList = cardList;
            this.heroCardUid = heroCardUid;
        }
    }

    /// <summary>
    /// 유저 덱(Deck) 목록을 관리하고 저장/불러오기 및 덱 편집(추가/삭제/카드 편집)을 제공하는 데이터 클래스입니다.
    /// </summary>
    /// <remarks>
    /// 슬롯 개수 제한, 덱 내 최대 카드 수 제한, 카드별 덱 내 중복 제한 등을 검증하며,
    /// 변경 시 저장 트리거를 호출합니다.
    /// </remarks>
    public class MyDeckData : DefaultData, ISaveData
    {
        /// <summary>
        /// 유저 덱 저장 데이터 목록입니다. (덱 인덱스 -> 덱 데이터)
        /// </summary>
        /// <remarks>
        /// public 이어야 JSON 직렬화로 저장됩니다.
        /// </remarks>
        public Dictionary<int, MyDeckSaveData> myDeckSaveData = new Dictionary<int, MyDeckSaveData>();

        private int _currentIndex;
        private TableTcgCard _tableTcgCard;
        private int _maxDeckCardCount;

        /// <summary>
        /// 덱 데이터 시스템을 초기화하고, 저장 데이터가 있으면 이를 로드합니다.
        /// </summary>
        /// <param name="loader">카드 테이블 등 TCG 테이블 로더입니다.</param>
        /// <param name="saveDataContainer">저장 데이터 컨테이너(선택)입니다. null 이면 신규 상태로 초기화됩니다.</param>
        public void Initialize(TableLoaderManagerTcg loader, SaveDataContainerTcg saveDataContainer = null)
        {
            myDeckSaveData.Clear();

            // 저장 데이터가 있으면 복사하여 사용
            if (saveDataContainer?.MyDeckData != null)
            {
                myDeckSaveData = new Dictionary<int, MyDeckSaveData>(saveDataContainer.MyDeckData.myDeckSaveData);
            }

            _tableTcgCard = loader.TableTcgCard;
            _maxDeckCardCount = AddressableLoaderSettingsTcg.Instance.tcgSettings.maxDeckCardCount;
        }

        /// <summary>
        /// 변경 사항 저장을 요청합니다.
        /// </summary>
        protected override void SaveDatas()
        {
            TcgPackageManager.Instance.saveDataManagerTcg.StartSaveData();
        }

        /// <summary>
        /// 현재 보유한 덱의 개수를 반환합니다.
        /// </summary>
        /// <returns>현재 덱 개수입니다.</returns>
        public int GetCurrentCount()
        {
            return myDeckSaveData.Count;
        }

        /// <summary>
        /// 생성 가능한 덱 슬롯의 최대 개수를 UI 설정에서 조회합니다.
        /// </summary>
        /// <returns>최대 덱 슬롯 개수입니다. UI가 없으면 0입니다.</returns>
        protected override int GetMaxSlotCount()
        {
            return SceneGame.Instance.uIWindowManager
                .GetUIWindowByUid<UIWindowTcgMyDeck>(UIWindowConstants.WindowUid.TcgMyDeck)?.maxCountIcon ?? 0;
        }

        /// <summary>
        /// 새 덱을 생성하고 목록에 추가합니다.
        /// </summary>
        /// <param name="deckName">생성할 덱 이름입니다.</param>
        /// <returns>생성된 덱의 인덱스입니다. 생성 실패 시 -1을 반환합니다.</returns>
        public int AddNewDeck(string deckName)
        {
            if (GetCurrentCount() >= MaxSlotCount)
            {
                SceneGame.Instance.systemMessageManager.ShowMessageWarning("Tcg_System_MaxDeckCount", MaxSlotCount);
                return -1;
            }

            var newIndex = myDeckSaveData.Count;
            var data = new MyDeckSaveData(newIndex, deckName, new Dictionary<int, int>(), 0);
            myDeckSaveData.TryAdd(newIndex, data);

            SaveDatas();
            return newIndex;
        }

        /// <summary>
        /// 모든 덱 데이터를 반환합니다.
        /// </summary>
        /// <returns>덱 인덱스 -> 덱 데이터 딕셔너리입니다.</returns>
        public Dictionary<int, MyDeckSaveData> GetAllData()
        {
            return myDeckSaveData;
        }

        /// <summary>
        /// 덱 인덱스로 덱 정보를 조회합니다.
        /// </summary>
        /// <param name="index">조회할 덱 인덱스입니다.</param>
        /// <returns>덱 데이터입니다. 존재하지 않으면 null입니다.</returns>
        public MyDeckSaveData GetDeckInfoByIndex(int index)
        {
            var data = myDeckSaveData.GetValueOrDefault(index);
            if (data != null) return data;

            GcLogger.LogError($"저장된 덱 정보가 없습니다. index: {index}");
            return null;
        }

        /// <summary>
        /// 덱에 카드를 1장 추가합니다.
        /// </summary>
        /// <param name="index">대상 덱 인덱스입니다.</param>
        /// <param name="cardUid">추가할 카드 UID입니다.</param>
        /// <returns>추가에 성공하면 true, 실패하면 false입니다.</returns>
        /// <remarks>
        /// - 덱 내 총 카드 수 제한(_maxDeckCardCount)을 초과할 수 없습니다.
        /// - 카드별 덱 내 중복 제한(info.maxCopiesPerDeck)을 초과할 수 없습니다.
        /// </remarks>
        public bool AddCardToDeck(int index, int cardUid)
        {
            var deckSaveData = GetDeckInfoByIndex(index);
            if (deckSaveData == null) return false;

            var info = _tableTcgCard.GetDataByUid(cardUid);
            if (info == null) return false;

            int count = deckSaveData.cardList.GetValueOrDefault(cardUid, -1);

            // 최대 넣을 수 있는 개수 체크: "새 카드" 추가일 때만 덱 총 카드 수를 증가시키므로 그 경우에만 검사
            if (count == -1 && deckSaveData.cardList.Count + 1 > _maxDeckCardCount)
            {
                SceneGame.Instance.systemMessageManager.ShowMessageWarning("Tcg_System_MaxCardCountInDeck", _maxDeckCardCount);
                return false;
            }

            // 신규 카드 추가
            if (count == -1)
            {
                deckSaveData.cardList.TryAdd(cardUid, 1);
            }
            // 기존 카드 수량 증가(중복 제한 검사)
            else
            {
                if (count + 1 > info.maxCopiesPerDeck)
                {
                    SceneGame.Instance.systemMessageManager.ShowMessageWarning("Tcg_System_MaxCardCopies", info.maxCopiesPerDeck);
                    return false;
                }

                deckSaveData.cardList[cardUid] = count + 1;
            }

            SaveDatas();
            return true;
        }

        /// <summary>
        /// 덱의 영웅(히어로) 카드를 설정합니다.
        /// </summary>
        /// <param name="index">대상 덱 인덱스입니다.</param>
        /// <param name="cardUid">설정할 영웅 카드 UID입니다.</param>
        /// <returns>설정에 성공하면 true, 실패하면 false입니다.</returns>
        public bool AddHeroCardToDeck(int index, int cardUid)
        {
            var deckSaveData = GetDeckInfoByIndex(index);
            if (deckSaveData == null) return false;

            var info = _tableTcgCard.GetDataByUid(cardUid);
            if (info == null) return false;

            deckSaveData.heroCardUid = cardUid;
            SaveDatas();
            return true;
        }

        /// <summary>
        /// 덱에서 특정 카드를 1장 제거하고, 제거 후 남은 수량을 반환합니다.
        /// </summary>
        /// <param name="deckIndex">대상 덱 인덱스입니다.</param>
        /// <param name="cardUid">제거할 카드 UID입니다.</param>
        /// <returns>
        /// 제거 후 남은 수량입니다.
        /// 카드가 없거나 덱이 없으면 -1을 반환합니다.
        /// </returns>
        public int RemoveCardToDeck(int deckIndex, int cardUid)
        {
            var cardList = GetDeckInfoByIndex(deckIndex)?.cardList;
            if (cardList == null) return -1;

            int count = cardList.GetValueOrDefault(cardUid, -1);
            if (count == -1) return -1;

            count--;
            if (count <= 0)
            {
                cardList.Remove(cardUid);
            }
            else
            {
                cardList[cardUid] = count;
            }

            SaveDatas();
            return count;
        }

        /// <summary>
        /// 덱을 삭제하고, 남은 덱들의 인덱스를 0부터 연속되도록 재정렬합니다.
        /// </summary>
        /// <param name="index">삭제할 덱 인덱스입니다.</param>
        /// <returns>삭제에 성공하면 true, 실패하면 false입니다.</returns>
        public bool RemoveDeck(int index)
        {
            var data = GetDeckInfoByIndex(index);
            if (data == null)
            {
                GcLogger.LogError($"덱 정보가 없습니다. deck Index: {index}");
                return false;
            }

            var result = myDeckSaveData.Remove(index);

            // 삭제 성공 시 key 기준 오름차순으로 정렬하면서 0부터 다시 인덱스 부여
            if (result)
            {
                var reordered = new Dictionary<int, MyDeckSaveData>();
                int newIndex = 0;

                foreach (var kv in myDeckSaveData.OrderBy(kv => kv.Key))
                {
                    kv.Value.index = newIndex;
                    reordered[newIndex] = kv.Value;
                    newIndex++;
                }

                myDeckSaveData = reordered;
                SaveDatas();
            }

            return result;
        }

        /// <summary>
        /// 덱 인덱스로 영웅(히어로) 카드 UID를 조회합니다.
        /// </summary>
        /// <param name="deckIndex">대상 덱 인덱스입니다.</param>
        /// <returns>영웅 카드 UID입니다. 덱이 없거나 미설정이면 0입니다.</returns>
        public int GetHeroCardUidByDeckIndex(int deckIndex)
        {
            return GetDeckInfoByIndex(deckIndex)?.heroCardUid ?? 0;
        }
    }
}
