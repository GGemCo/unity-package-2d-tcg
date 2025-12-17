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
        
        public void Release()
        {
        }
    }
}