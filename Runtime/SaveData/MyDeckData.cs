using System.Collections.Generic;
using System.Linq;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    public class MyDeckSaveData
    {
        public int index;
        public readonly string deckName;
        public readonly Dictionary<int, int> cardList;

        public MyDeckSaveData(int index, string deckName, Dictionary<int, int> cardList)
        {
            this.index = index;
            this.deckName = deckName;
            this.cardList = cardList;
        }
    }
    public class MyDeckData : DefaultData, ISaveData
    {
        // public 으로 해야 json 으로 저장된다.
        public Dictionary<int, MyDeckSaveData> myDeckSaveData = new Dictionary<int, MyDeckSaveData>();
        
        private int _currentIndex;
        private TableTcgCard _tableTcgCard;
        private int _maxDeckCardCount;
        
        public void Initialize(TableLoaderManagerTcg loader, SaveDataContainerTcg saveDataContainer = null)
        {
            myDeckSaveData.Clear();
            if (saveDataContainer?.MyDeckData != null)
            {
                myDeckSaveData = new Dictionary<int, MyDeckSaveData>(saveDataContainer.MyDeckData.myDeckSaveData);
            }
            _tableTcgCard = loader.TableTcgCard;
            _maxDeckCardCount = AddressableLoaderSettingsTcg.Instance.tcgSettings.maxDeckCardCount;
        }
        protected override void SaveDatas()
        {
            TcgPackageManager.Instance.saveDataManagerTcg.StartSaveData();
        }

        public int GetCurrentCount()
        {
            return myDeckSaveData.Count;
        }

        protected override int GetMaxSlotCount()
        {
            return SceneGame.Instance.uIWindowManager
                .GetUIWindowByUid<UIWindowMyDeck>(UIWindowConstants.WindowUid.TcgMyDeck)?.maxCountIcon ?? 0;
        }

        public int AddNewDeck(string deckName)
        {
            if (GetCurrentCount() >= MaxSlotCount)
            {
                // todo 정리 필요. localization
                SceneGame.Instance.systemMessageManager.ShowMessageWarning($"최대 생성할 수 있는 덱의 개수는 {MaxSlotCount}개 입니다.");
                return -1;
            }

            var newIndex = myDeckSaveData.Count;
            var data = new MyDeckSaveData(newIndex, deckName, new Dictionary<int, int>());
            myDeckSaveData.TryAdd(newIndex, data);
            SaveDatas();
            return newIndex;
        }

        public Dictionary<int, MyDeckSaveData> GetAllData()
        {
            return myDeckSaveData;
        }

        public MyDeckSaveData GetDeckInfoByIndex(int index)
        {
            var data = myDeckSaveData.GetValueOrDefault(index);
            if (data != null) return data;
            
            GcLogger.LogError($"저장된 덱 정보가 없습니다. index: {index}");
            return null;
        }

        public bool AddCardToDeck(int index, int cardUid)
        {
            var deckSaveData = GetDeckInfoByIndex(index);
            if (deckSaveData == null) return false;
            var info = _tableTcgCard.GetDataByUid(cardUid);
            if (info == null) return false;
            int count = deckSaveData.cardList.GetValueOrDefault(cardUid, -1);
            
            // 최대 넣을 수 있는 개수 체크
            // 같은 카드가 없을 때만 체크
            if (count == -1 && deckSaveData.cardList.Count + 1 > _maxDeckCardCount)
            {
                // todo 정리 필요. localization
                SceneGame.Instance.systemMessageManager.ShowMessageWarning($"덱에 넣을 수 있는 최대 카드 개수는 {_maxDeckCardCount}개 입니다.");
                return false;
            }
            
            // 추가된 카드가 아닐 경우 
            if (count == -1)
            {
                deckSaveData.cardList.TryAdd(cardUid, 1);
            }
            // 같은 카드중복 체크. 최대 넣을 수 있는 개수 체크 
            else
            {
                if (count + 1 > info.maxCopiesPerDeck)
                {
                    // todo 정리 필요. localization
                    SceneGame.Instance.systemMessageManager.ShowMessageWarning($"해당 카드는 최대 {info.maxCopiesPerDeck}개 까지만 추가할 수 있습니다.");
                    return false;
                }
                deckSaveData.cardList[cardUid] = count + 1;
            }
            
            SaveDatas();
            return true;
        }
        /// <summary>
        /// 카드 덱에서 특정 카드 빼기
        /// </summary>
        /// <param name="deckIndex"></param>
        /// <param name="cardUid"></param>
        /// <returns></returns>
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

        public bool RemoveDeck(int index)
        {
            var data = GetDeckInfoByIndex(index);
            if (data == null)
            {
                GcLogger.LogError($"덱 정보가 없습니다. deck Index: {index}");
                return false;
            }
            var result= myDeckSaveData.Remove(index);
            // 재정렬
            if (result)
            {
                // key 기준 오름차순으로 정렬하면서 0부터 다시 인덱스 부여
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
    }
}