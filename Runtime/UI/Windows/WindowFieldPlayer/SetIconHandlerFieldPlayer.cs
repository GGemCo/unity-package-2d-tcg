using GGemCo2DCore;

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
            var slot = window.GetSlotByIndex(slotIndex);
            slot?.gameObject.SetActive(true);
        }
        public void OnDetachIcon(UIWindow window, int slotIndex)
        {
            // GcLogger.Log("FieldPlayer OnDetachIcon");
            var slot = window.GetSlotByIndex(slotIndex);
            slot?.gameObject.SetActive(false);
        }
    }
}