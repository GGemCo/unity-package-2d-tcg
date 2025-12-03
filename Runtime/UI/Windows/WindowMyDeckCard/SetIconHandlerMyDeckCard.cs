using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 카드 콜랙션 윈도우 - 아이콘 관리
    /// </summary>
    public class SetIconHandlerMyDeckCard : ISetIconHandler
    {
        public void OnSetIcon(UIWindow window, int slotIndex, int iconUid, int iconCount, int iconLevel, bool isLearned)
        {
            // GcLogger.Log("MyDeckCard OnSetIcon");
            UIWindowTcgMyDeckCard uiWindowTcgCardCollection = window as UIWindowTcgMyDeckCard;
            if (uiWindowTcgCardCollection == null) return;
        }
        public void OnDetachIcon(UIWindow window, int slotIndex)
        {
            // GcLogger.Log("MyDeckCard OnDetachIcon");
            var slot = window.GetSlotByIndex(slotIndex);
            slot?.gameObject.SetActive(false);
        }
    }
}