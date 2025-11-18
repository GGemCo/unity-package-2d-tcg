using System.Collections.Generic;
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
        public void BuildSlotsAndIcons(UIWindow window, GridLayoutGroup container, int maxCount,
            IconConstants.Type iconType, Vector2 slotSize, Vector2 iconSize, GameObject[] slots, GameObject[] icons)
        {
            if (AddressableLoaderSettings.Instance == null || window.containerIcon == null) return;
            UIWindowCardCollection uIWindowCardCollection = window as UIWindowCardCollection;
            if (uIWindowCardCollection == null) return;
            
            // 카드 프리팹 가져오기
            Dictionary<CardConstants.Type, GameObject> prefabUIElementCard = new  Dictionary<CardConstants.Type, GameObject>();
            foreach (var type in EnumCache<CardConstants.Type>.Values)
            {
                if (type == CardConstants.Type.None) continue;
                var key = $"{ConfigAddressableKeyTcg.Card.UIElement}_{type}";
                GameObject prefab = AddressableLoaderPrefabUIElementCard.Instance.GetPrefabByName(key);
                if (prefab == null) continue;
                prefabUIElementCard.TryAdd(type, prefab);
            }
            
            // tcg_card 데이터 테이블 가져오기
            var datas = uIWindowCardCollection.tableTcgCard.GetDatas();
            uIWindowCardCollection.maxCountIcon = datas.Count;
            if (datas.Count <= 0) return;
            
            GameObject slot = ConfigResources.Slot.Load();
            if (slot == null) return;
            
            int index = 0;
            foreach (var data in datas)
            {
                var info = data.Value;
                if (info is not { uid: > 0 }) continue;
                // GcLogger.Log($"card: {info.uid} / {info.name}");

                var prefabUIElementCardCommon = prefabUIElementCard.GetValueOrDefault(info.type);
                if (prefabUIElementCardCommon == null)
                {
                    GcLogger.LogError($"Type에 맞는 프리팹이 없습니다. type: {info.type}");
                    continue;
                }
                
                GameObject slotObject = Object.Instantiate(slot, uIWindowCardCollection.containerIcon.gameObject.transform);
                UISlot uiSlot = slotObject.GetComponent<UISlot>();
                if (uiSlot == null) continue;
                uiSlot.Initialize(uIWindowCardCollection, uIWindowCardCollection.uid, index, slotSize);
                uIWindowCardCollection.SetPositionUiSlot(uiSlot, index);
                slots[index] = slotObject;

                GameObject icon = Object.Instantiate(prefabUIElementCardCommon, slotObject.transform);
                UIElementCard uiElementCard = icon.GetComponent<UIElementCard>();
                if (uiElementCard == null) continue;
                uiElementCard.Initialize(uIWindowCardCollection, uIWindowCardCollection.uid, index, index, iconSize, slotSize);
                // count, 레벨 1로 초기화
                uiElementCard.ChangeInfoByUid(info.uid, 1, 1);
                
                icons[index] = icon;
                index++;
            }
        }
    }
}