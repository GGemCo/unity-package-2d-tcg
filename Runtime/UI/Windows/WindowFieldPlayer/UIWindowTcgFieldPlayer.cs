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

    }
}