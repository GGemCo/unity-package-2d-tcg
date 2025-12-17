using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    public abstract class UIWindowTcgFieldBase : UIWindow
    {
        [Header(UIWindowConstants.TitleHeaderIndividual)]
        public float timeToMove = 0.2f;
        public float timeToFadeOut = 0.6f;
        
        // 각 Side 별 UID (Player/Enemy 가 다름)
        protected abstract UIWindowConstants.WindowUid WindowUid { get; }
        
        protected abstract ISetIconHandler CreateSetIconHandler();
        protected abstract IDragDropStrategy CreateDragDropStrategy();
        
        protected override void Awake()
        {
            if (TableLoaderManager.Instance == null)
                return;

            if (containerIcon == null)
            {
                GcLogger.LogError($"{GetType().Name}: containerIcon 이 null 입니다.");
                return;
            }

            uid = WindowUid;

            base.Awake();

            IconPoolManager.SetSetIconHandler(CreateSetIconHandler());
            DragDropHandler.SetStrategy(CreateDragDropStrategy());
        }
        
        public void RefreshBoard(TcgBattleDataSide battleDataSide)
        {
            if (battleDataSide == null)
            {
                GcLogger.LogError($"{GetType().Name}: battleDataSide 가 null 입니다.");
                return;
            }

            DetachAllIcons();

            int i = 0;
            foreach (var card in battleDataSide.Board)
            {
                var uiIcon = SetIconCount(i, card.Uid, 1);
                if (!uiIcon) { i++; continue; }

                card.Index = i;
                var uiIconCard = uiIcon.GetComponent<UIIconCard>();
                if (uiIconCard != null)
                {
                    uiIconCard.UpdateAttack(card.Attack);
                    uiIconCard.UpdateHealth(card.Hp);
                }
                i++;
            }
        }

        public int GetActiveIconCount()
        {
            int count = 0;
            foreach (var slot in slots)
            {
                if (slot == null || !slot.activeSelf) continue;
                count++;
            }
            return count;
        }
    }
}