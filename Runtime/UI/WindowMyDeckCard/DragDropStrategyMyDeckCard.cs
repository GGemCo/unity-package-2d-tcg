using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 카드 콜랙션 윈도우 - 아이콘 드래그 앤 드랍 관리
    /// </summary>
    public class DragDropStrategyMyDeckCard : IDragDropStrategy
    {
        public void HandleDragInWindow(UIWindow window, UIIcon droppedUIIcon)
        {
            UIWindowMyDeckCard uiWindowMyDeck = window as UIWindowMyDeckCard;
            if (uiWindowMyDeck == null) return;
            // GcLogger.Log("OnEndDragInIcon");
            UIWindow droppedWindow = droppedUIIcon.window;
            UIWindowConstants.WindowUid droppedWindowUid = droppedUIIcon.windowUid;
            int dropIconSlotIndex = droppedUIIcon.slotIndex;
            int dropIconUid = droppedUIIcon.uid;
            int dropIconCount = droppedUIIcon.GetCount();
            if (dropIconUid <= 0)
            {
                return;
            }
            uiWindowMyDeck.AddCardToDeck(dropIconUid);
        }
        public void HandleDragInIcon(UIWindow window, UIIcon droppedUIIcon, UIIcon targetUIIcon)
        {
            UIWindowMyDeckCard uiWindowMyDeck = window as UIWindowMyDeckCard;
            if (uiWindowMyDeck == null) return;
            // GcLogger.Log("OnEndDragInIcon");
            UIWindow droppedWindow = droppedUIIcon.window;
            UIWindowConstants.WindowUid droppedWindowUid = droppedUIIcon.windowUid;
            int dropIconSlotIndex = droppedUIIcon.slotIndex;
            int dropIconUid = droppedUIIcon.uid;
            int dropIconCount = droppedUIIcon.GetCount();
            if (dropIconUid <= 0)
            {
                return;
            }
            
            // 드래그앤 드랍 한 곳에 아무것도 없을때 
            if (targetUIIcon == null)
            {
                return;
            }
            UIWindow targetWindow = targetUIIcon.window;
            UIWindowConstants.WindowUid targetWindowUid = targetUIIcon.windowUid;
            int targetIconSlotIndex = targetUIIcon.slotIndex;
            int targetIconUid = targetUIIcon.uid;
            int targetIconCount = targetUIIcon.GetCount();

            // 다른 윈도우에서 Collection으로 드래그 앤 드랍 했을 때 
            if (droppedWindowUid != targetWindowUid)
            {
                switch (droppedWindowUid)
                {
                    case UIWindowConstants.WindowUid.TcgCardCollection:
                        // GcLogger.Log("UIWindowMyCardDeck HandleDragInIcon");
                        uiWindowMyDeck.AddCardToDeck(dropIconUid);
                        break;
                }
            }
            else
            {
                if (targetIconSlotIndex < window.maxCountIcon)
                {
                }
            }
        }

        public void HandleDragOut(UIWindow window, Vector3 worldPosition, GameObject droppedIcon, GameObject targetIcon,
            Vector3 originalPosition)
        {
        }
    }
}