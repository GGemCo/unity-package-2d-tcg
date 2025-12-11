using GGemCo2DCore;

namespace GGemCo2DTcg
{
    public class UIWindowTcgFieldPlayer : UIWindow
    {
        protected override void Awake()
        {
            uid = UIWindowConstants.WindowUid.TcgFieldPlayer;

            base.Awake();

            IconPoolManager.SetSetIconHandler(new SetIconHandlerFieldPlayer());
            DragDropHandler.SetStrategy(new DragDropStrategyFieldPlayer());
        }
        public override void OnShow(bool show)
        {
            DetachAllIcons();
        }

        public void RefreshBoard(TcgBattleDataSide player)
        {
            DetachAllIcons();
            int i = 0;
            foreach (var tcgBattleDataCard in player.Board)
            {
                var uiIcon = SetIconCount(i, tcgBattleDataCard.Uid, 1);
                if (!uiIcon) continue;
                var uiIconFieldPlayer = uiIcon as UIIconFieldPlayer;
                if (!uiIconFieldPlayer) continue;
                uiIconFieldPlayer.SetBattleDataFieldCard(tcgBattleDataCard);
                i++;
            }
        }
    }
}