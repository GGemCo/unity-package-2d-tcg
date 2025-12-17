using GGemCo2DCore;

namespace GGemCo2DTcg
{
    public class UIWindowTcgFieldEnemy : UIWindowTcgFieldBase
    {
        protected override UIWindowConstants.WindowUid WindowUid =>
            UIWindowConstants.WindowUid.TcgFieldEnemy;

        protected override ISetIconHandler CreateSetIconHandler() =>
            new SetIconHandlerFieldEnemy();

        protected override IDragDropStrategy CreateDragDropStrategy() =>
            new DragDropStrategyFieldEnemy();
        
        protected override void BindCardIcon(UIIcon uiIcon, TcgBattleDataFieldCard card)
        {
            var iconEnemy = uiIcon as UIIconFieldEnemy;
            if (!iconEnemy) return;

            iconEnemy.SetBattleDataFieldCard(card);
        }
        public void Release()
        {
        }
    }
}