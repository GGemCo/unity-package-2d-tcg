using GGemCo2DCore;

namespace GGemCo2DTcg
{
    public class UIWindowTcgFieldPlayer : UIWindowTcgFieldBase
    {
        protected override UIWindowConstants.WindowUid WindowUid =>
            UIWindowConstants.WindowUid.TcgFieldPlayer;

        protected override ISetIconHandler CreateSetIconHandler() =>
            new SetIconHandlerFieldPlayer();

        protected override IDragDropStrategy CreateDragDropStrategy() =>
            new DragDropStrategyFieldPlayer();
        
        protected override void BindCardIcon(UIIcon uiIcon, TcgBattleDataFieldCard card)
        {
            var iconPlayer = uiIcon as UIIconFieldPlayer;
            if (!iconPlayer) return;

            iconPlayer.SetBattleDataFieldCard(card);
        }
        public void Release()
        {
        }
    }
}