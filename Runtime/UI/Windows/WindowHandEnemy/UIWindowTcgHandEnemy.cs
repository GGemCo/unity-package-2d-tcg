using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 상대 AI의 카드가 있는 윈도우
    /// </summary>
    public class UIWindowTcgHandEnemy : UIWindowTcgHandBase
    {
        private TcgBattleControllerEnemy _battleControllerEnemy;

        protected override void Awake()
        {
            if (TableLoaderManager.Instance == null)
            {
                return;
            }
            if (containerIcon == null)
            {
                GcLogger.LogError($"{nameof(UIWindowTcgHandEnemy)}: containerIcon 이 null 입니다.");
                return;
            }
            
            uid = UIWindowConstants.WindowUid.TcgHandEnemy;

            base.Awake();

            IconPoolManager.SetSetIconHandler(new SetIconHandlerEnemy());
            DragDropHandler.SetStrategy(new DragDropStrategyHandEnemy());
        }

        public override void SetController(TcgBattleControllerBase tcgBattleController)
        {
            base.SetController(tcgBattleController);
            _battleControllerEnemy = tcgBattleController as TcgBattleControllerEnemy;
        }

        public override void SetInteractable(bool b)
        {
            // 버튼/슬롯에 RaycastTarget, 버튼 활성화 등 적용
        }

        public void OnClickCard(TcgBattleDataCard tcgBattleDataCard)
        {
            _battleControllerEnemy?.OnUiRequestPlayCard(tcgBattleDataCard);
        }
        public override void RefreshHand(IReadOnlyList<TcgBattleDataCard> hand)
        {
            base.RefreshHand(hand);
            int i = 0;
            foreach (var cardRuntime in hand)
            {
                var uiIcon = SetIconCount(i, cardRuntime.Uid, 1);
                if (!uiIcon) continue;
                var uiIconHandEnemy = uiIcon as UIIconHandEnemy;
                if (!uiIconHandEnemy) continue;
                uiIconHandEnemy.SetCardRuntime(cardRuntime);
                i++;
            }
        }
    }
}