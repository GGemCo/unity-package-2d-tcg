using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 필드 플레이어 윈도우 - 아이콘 드래그 앤 드랍 관리
    /// </summary>
    public class DragDropStrategyFieldPlayer : IDragDropStrategy
    {
        private TcgBattleManager _battleManager;
        
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
                    UseCard(droppedUIIcon);
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
                        UseCard(droppedUIIcon);
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

        private void UseCard(UIIcon droppedUIIcon)
        {
            // 0은 영웅으로 사용하고 있기 때문에 -1 해준다
            var newIndex = droppedUIIcon.index - 1;
            _battleManager ??= TcgPackageManager.Instance.battleManager;
            _battleManager?.OnUiRequestPlayCard(ConfigCommonTcg.TcgPlayerSide.Player, newIndex);
        }
    }
}