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
        
        public void Release()
        {
        }
    }
}