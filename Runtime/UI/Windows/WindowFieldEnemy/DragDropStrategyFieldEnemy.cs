using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 필드 적 윈도우 - 아이콘 드래그 앤 드랍 관리
    /// </summary>
    public class DragDropStrategyFieldEnemy : IDragDropStrategy
    {
        public void HandleDragInWindow(UIWindow window, UIIcon droppedUIIcon)
        {
            UIWindowTcgFieldEnemy uiWindowTcgFieldEnemy = window as UIWindowTcgFieldEnemy;
            if (uiWindowTcgFieldEnemy == null) return;
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
        }
        public void HandleDragInIcon(UIWindow window, UIIcon droppedUIIcon, UIIcon targetUIIcon)
        {
            UIWindowTcgFieldEnemy uiWindowTcgFieldEnemy = window as UIWindowTcgFieldEnemy;
            if (uiWindowTcgFieldEnemy == null) return;
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
                    case UIWindowConstants.WindowUid.TcgFieldPlayer:
                        var uiWindowTcgFieldPlayer = droppedWindow as UIWindowTcgFieldPlayer;
                        if (uiWindowTcgFieldPlayer == null)
                        {
                            GcLogger.LogError($"{nameof(UIWindowTcgFieldPlayer)} 클래스가 없습니다.");
                            return;
                        }
                        UIIconFieldPlayer uiIconFieldPlayer = droppedUIIcon as UIIconFieldPlayer;
                        if (uiIconFieldPlayer == null)
                        {
                            GcLogger.LogError($"드랍하는 아이콘에 {nameof(UIIconFieldPlayer)} 클래스가 없습니다.");
                            return;
                        }
                        UIIconFieldEnemy uiIconFieldEnemy = targetUIIcon as UIIconFieldEnemy;
                        if (uiIconFieldEnemy == null)
                        {
                            GcLogger.LogError($"타겟 아이콘에 {nameof(UIIconFieldEnemy)} 클래스가 없습니다.");
                            return;
                        }
                        TcgBattleDataFieldCard battleDataCardPlayer = uiIconFieldPlayer.GetBattleDataFieldCard();
                        TcgBattleDataFieldCard battleDataCardEnemy = uiIconFieldEnemy.GetBattleDataFieldCard();
                        // uiWindowTcgFieldPlayer.OnRequestAttackUnit(battleDataCardPlayer, battleDataCardEnemy);
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