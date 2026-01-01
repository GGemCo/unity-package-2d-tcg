using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 상대 AI의 카드가 있는 윈도우
    /// </summary>
    public class UIWindowTcgHandEnemy : UIWindowTcgHandBase
    {
        protected override UIWindowConstants.WindowUid WindowUid =>
            UIWindowConstants.WindowUid.TcgHandEnemy;

        protected override ISetIconHandler CreateSetIconHandler() =>
            new SetIconHandlerEnemy();

        protected override IDragDropStrategy CreateDragDropStrategy() =>
            new DragDropStrategyHandEnemy();
    }
}