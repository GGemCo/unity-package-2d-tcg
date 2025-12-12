using GGemCo2DCore;

namespace GGemCo2DTcg
{
    public class UIWindowTcgFieldEnemy : UIWindow
    {
        private TcgBattleManager _battleManager;
        
        protected override void Awake()
        {
            uid = UIWindowConstants.WindowUid.TcgFieldEnemy;

            base.Awake();

            IconPoolManager.SetSetIconHandler(new SetIconHandlerFieldEnemy());
            DragDropHandler.SetStrategy(new DragDropStrategyFieldEnemy());
        }
        public override void OnShow(bool show)
        {
            DetachAllIcons();
        }

        public void SetBattleManager(TcgBattleManager battleManager)
        {
            _battleManager = battleManager;
        }

        public void RefreshBoard(TcgBattleDataSide enemy)
        {
            DetachAllIcons();
            int i = 0;
            foreach (var tcgBattleDataCard in enemy.Board)
            {
                var uiIcon = SetIconCount(i, tcgBattleDataCard.Uid, 1);
                if (!uiIcon) continue;
                var uiIconFieldEnemy = uiIcon as UIIconFieldEnemy;
                if (!uiIconFieldEnemy) continue;
                uiIconFieldEnemy.SetBattleDataFieldCard(tcgBattleDataCard);
                i++;
            }
        }

        public void Release()
        {
            
        }
    }
}