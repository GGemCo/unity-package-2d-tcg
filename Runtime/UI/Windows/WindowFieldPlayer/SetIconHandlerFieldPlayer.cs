using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 카드 콜랙션 윈도우 - 아이콘 관리
    /// </summary>
    public class SetIconHandlerFieldPlayer : ISetIconHandler
    {
        public void OnSetIcon(UIWindow window, int slotIndex, int iconUid, int iconCount, int iconLevel, bool isLearned)
        {
            // GcLogger.Log("FieldPlayer OnSetIcon");
            UIWindowTcgFieldPlayer uiWindowTcgFieldPlayer = window as UIWindowTcgFieldPlayer;
            if (uiWindowTcgFieldPlayer == null) return;
            // 아이콘 위치 재정렬
            var icon = window.GetIconByIndex(slotIndex);
            if (icon)
            {
                icon.transform.localPosition = Vector3.zero;
            }
            var slot = window.GetSlotByIndex(slotIndex);
            slot?.gameObject.SetActive(true);
        }
        public void OnDetachIcon(UIWindow window, int slotIndex)
        {
            // GcLogger.Log("FieldPlayer OnDetachIcon");
            // 아이콘 위치 재정렬
            var icon = window.GetIconByIndex(slotIndex);
            if (icon)
            {
                icon.transform.localPosition = Vector3.zero;
            }
            var slot = window.GetSlotByIndex(slotIndex);
            slot?.gameObject.SetActive(false);
        }
    }
}