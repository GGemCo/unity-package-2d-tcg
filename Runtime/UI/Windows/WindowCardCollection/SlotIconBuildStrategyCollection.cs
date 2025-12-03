using GGemCo2DCore;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 플레이어 스킬 윈도우 - 아이콘 생성
    /// </summary>
    public class SlotIconBuildStrategyCollection : ISlotIconBuildStrategy
    {
        private readonly TableTcgCard _tableTcgCard;
        
        public SlotIconBuildStrategyCollection(TableTcgCard tableTcgCard)
        {
            _tableTcgCard = tableTcgCard;
        }

        public void BuildSlotsAndIcons(UIWindow window, GridLayoutGroup container, int maxCount,
            IconConstants.Type iconType, Vector2 slotSize, Vector2 iconSize, GameObject[] slots, GameObject[] icons)
        {
            if (AddressableLoaderSettings.Instance == null || window.containerIcon == null) return;
            UIWindowTcgCardCollection uIWindowTcgCardCollection = window as UIWindowTcgCardCollection;
            if (uIWindowTcgCardCollection == null) return;
            if (uIWindowTcgCardCollection.iconPrefab == null)
            {
                GcLogger.LogError($"카드 아이콘 프리팹이 없습니다.");
                return;
            }
            
            // tcg_card 데이터 테이블 가져오기
            var datas = _tableTcgCard.GetDatas();
            uIWindowTcgCardCollection.maxCountIcon = datas.Count;
            if (datas.Count <= 0) return;
            
            GameObject slot = ConfigResources.Slot.Load();
            if (slot == null) return;
            
            int index = 0;
            foreach (var data in datas)
            {
                var info = data.Value;
                if (info is not { uid: > 0 }) continue;
                // GcLogger.Log($"card: {info.uid} / {info.name}");
                
                GameObject slotObject = Object.Instantiate(slot, uIWindowTcgCardCollection.containerIcon.gameObject.transform);
                UISlot uiSlot = slotObject.GetComponent<UISlot>();
                if (uiSlot == null) continue;
                uiSlot.Initialize(uIWindowTcgCardCollection, uIWindowTcgCardCollection.uid, index, slotSize);
                uIWindowTcgCardCollection.SetPositionUiSlot(uiSlot, index);
                slots[index] = slotObject;

                GameObject icon = Object.Instantiate(uIWindowTcgCardCollection.iconPrefab, slotObject.transform);
                UIIconCard uiIconCard = icon.GetComponent<UIIconCard>();
                if (uiIconCard == null) continue;
                uiIconCard.Initialize(uIWindowTcgCardCollection, uIWindowTcgCardCollection.uid, index, index, iconSize, slotSize);
                // count, 레벨 1로 초기화
                uiIconCard.ChangeInfoByUid(info.uid, 1, 1);
                
                icons[index] = icon;
                index++;
            }
        }
    }
}