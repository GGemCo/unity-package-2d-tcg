using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 적(Enemy) 진영의 핸드 UI를 담당하는 윈도우입니다.
    /// <see cref="UIWindowTcgHandBase"/>의 공통 로직을 사용하며,
    /// Enemy 전용 UID/아이콘 세팅 핸들러/드래그 드롭 전략을 구체화합니다.
    /// (Enemy 핸드는 기본 정책상 드래그가 비활성화됩니다.)
    /// </summary>
    public class UIWindowTcgHandEnemy : UIWindowTcgHandBase
    {
        /// <summary>
        /// Enemy 핸드 윈도우에 해당하는 고정 Window UID입니다.
        /// </summary>
        protected override UIWindowConstants.WindowUid WindowUid =>
            UIWindowConstants.WindowUid.TcgHandEnemy;

        /// <summary>
        /// Enemy 핸드에서 아이콘을 세팅/갱신하는 핸들러를 생성합니다.
        /// </summary>
        /// <returns>Enemy 핸드용 아이콘 세팅 핸들러.</returns>
        protected override ISetIconHandler CreateSetIconHandler() =>
            new SetIconHandlerEnemy();

        /// <summary>
        /// Enemy 핸드에서 사용할 드래그/드롭 전략을 생성합니다.
        /// </summary>
        /// <returns>Enemy 핸드용 드래그/드롭 전략.</returns>
        protected override IDragDropStrategy CreateDragDropStrategy() =>
            new DragDropStrategyHandEnemy();
    }
}