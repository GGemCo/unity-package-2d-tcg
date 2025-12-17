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

        // Enemy 의 SetMana 는 텍스트만 있으면 되므로
        // 베이스 구현 그대로 사용 (override 불필요)
    }
}