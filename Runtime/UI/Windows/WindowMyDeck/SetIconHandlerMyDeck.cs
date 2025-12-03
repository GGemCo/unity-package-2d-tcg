using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 카드 콜랙션 윈도우 - 아이콘 관리
    /// </summary>
    public class SetIconHandlerMyDeck : ISetIconHandler
    {
        public void OnSetIcon(UIWindow window, int slotIndex, int iconUid, int iconCount, int iconLevel, bool isLearned)
        {
            UIWindowTcgMyDeck uiWindowTcgCardCollection = window as UIWindowTcgMyDeck;
            if (uiWindowTcgCardCollection == null) return;
        }
        public void OnDetachIcon(UIWindow window, int slotIndex)
        {
            var slot = window.GetSlotByIndex(slotIndex);
            slot?.gameObject.SetActive(false);
        }
    }
}