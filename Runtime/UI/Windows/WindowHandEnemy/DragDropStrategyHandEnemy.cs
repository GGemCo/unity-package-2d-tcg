using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 핸드 적 윈도우 - 아이콘 드래그 앤 드랍 관리
    /// </summary>
    public class DragDropStrategyHandEnemy : DragDropStrategyBase, IDragDropStrategy
    {
        private TcgBattleManager _battleManager;
        public void HandleDragInWindow(UIWindow window, UIIcon droppedUIIcon)
        {
            UIWindowTcgHandEnemy uiWindowTcgHandEnemy = window as UIWindowTcgHandEnemy;
            if (uiWindowTcgHandEnemy == null) return;
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
                // 핸드에서 바로 사용하는 카드
                case UIWindowConstants.WindowUid.TcgHandPlayer:
                    UseCard(droppedWindowUid, droppedUIIcon, window.uid);
                    break;
            }
        }
        public void HandleDragInIcon(UIWindow window, UIIcon droppedUIIcon, UIIcon targetUIIcon)
        {
            UIWindowTcgHandEnemy uiWindowTcgHandEnemy = window as UIWindowTcgHandEnemy;
            if (uiWindowTcgHandEnemy == null) return;
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
                    // 핸드에서 바로 사용하는 카드
                    // spell, equipment, permanent, event 타입
                    case UIWindowConstants.WindowUid.TcgHandPlayer:
                    case UIWindowConstants.WindowUid.TcgFieldPlayer:
                        // 영웅만 공격 가능
                        if (targetIconSlotIndex != ConfigCommonTcg.IndexHeroSlot) return;
                        UseCard(droppedWindowUid, droppedUIIcon, targetWindowUid, targetUIIcon);
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