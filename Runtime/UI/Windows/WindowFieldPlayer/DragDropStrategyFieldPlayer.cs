using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 필드 플레이어 윈도우 - 아이콘 드래그 앤 드랍 관리
    /// </summary>
    public class DragDropStrategyFieldPlayer : IDragDropStrategy
    {
        public void HandleDragInWindow(UIWindow window, UIIcon droppedUIIcon)
        {
            UIWindowTcgFieldPlayer uiWindowTcgFieldPlayer = window as UIWindowTcgFieldPlayer;
            if (uiWindowTcgFieldPlayer == null) return;
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
            switch (droppedWindowUid)
            {
                case UIWindowConstants.WindowUid.TcgHandPlayer:
                    var uiWindowTcgHandPlayer = droppedWindow as UIWindowTcgHandPlayer;
                    if (uiWindowTcgHandPlayer == null)
                    {
                        GcLogger.LogError($"{nameof(UIWindowTcgHandPlayer)} 클래스가 없습니다.");
                        return;
                    }
                    UIIconHandPlayer uiIconHandPlayer = droppedUIIcon as UIIconHandPlayer;
                    if (uiIconHandPlayer == null)
                    {
                        GcLogger.LogError($"{nameof(UIIconHandPlayer)} 클래스가 없습니다.");
                        return;
                    }
                    TcgBattleDataCard tcgBattleDataCard = uiIconHandPlayer.GetCardRuntime();
                    uiWindowTcgHandPlayer.OnClickCard(tcgBattleDataCard);
                    break;
            }
        }
        public void HandleDragInIcon(UIWindow window, UIIcon droppedUIIcon, UIIcon targetUIIcon)
        {
            UIWindowTcgFieldPlayer uiWindowTcgFieldPlayer = window as UIWindowTcgFieldPlayer;
            if (uiWindowTcgFieldPlayer == null) return;
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
                    case UIWindowConstants.WindowUid.TcgHandPlayer:
                        var uiWindowTcgHandPlayer = droppedWindow as UIWindowTcgHandPlayer;
                        if (uiWindowTcgHandPlayer == null)
                        {
                            GcLogger.LogError($"{nameof(UIWindowTcgHandPlayer)} 클래스가 없습니다.");
                            return;
                        }
                        UIIconHandPlayer uiIconHandPlayer = droppedUIIcon as UIIconHandPlayer;
                        if (uiIconHandPlayer == null)
                        {
                            GcLogger.LogError($"{nameof(UIIconHandPlayer)} 클래스가 없습니다.");
                            return;
                        }
                        TcgBattleDataCard tcgBattleDataCard = uiIconHandPlayer.GetCardRuntime();
                        uiWindowTcgHandPlayer.OnClickCard(tcgBattleDataCard);
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