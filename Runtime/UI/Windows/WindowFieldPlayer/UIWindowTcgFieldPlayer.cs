using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 플레이어(Player) 진영의 필드 UI를 담당하는 윈도우입니다.
    /// <see cref="UIWindowTcgFieldBase"/>의 공통 로직을 사용하며,
    /// Player 전용 UID/아이콘 처리기/드래그드롭 전략과 영웅 아이콘 타입을 구체화합니다.
    /// </summary>
    public class UIWindowTcgFieldPlayer : UIWindowTcgFieldBase
    {
        /// <summary>
        /// Player 필드 윈도우에 해당하는 고정 Window UID입니다.
        /// </summary>
        protected override UIWindowConstants.WindowUid WindowUid =>
            UIWindowConstants.WindowUid.TcgFieldPlayer;

        /// <summary>
        /// Player 필드에서 아이콘을 세팅/갱신하는 핸들러를 생성합니다.
        /// </summary>
        /// <returns>Player 필드용 아이콘 세팅 핸들러.</returns>
        protected override ISetIconHandler CreateSetIconHandler() =>
            new SetIconHandlerFieldPlayer();

        /// <summary>
        /// Player 필드에서 사용할 드래그/드롭 전략을 생성합니다.
        /// </summary>
        /// <returns>Player 필드용 드래그/드롭 전략.</returns>
        protected override IDragDropStrategy CreateDragDropStrategy() =>
            new DragDropStrategyFieldPlayer();

        /// <summary>
        /// 윈도우가 보유한 리소스/상태를 해제합니다.
        /// (현재 구현은 비어 있으며, 필요 시 이벤트 해제/풀 반환/캐시 초기화 등을 추가합니다.)
        /// </summary>
        public void Release()
        {
        }

        /// <summary>
        /// Player 필드에서 사용할 영웅 아이콘을 Player 전용 타입으로 캐스팅해 반환합니다.
        /// </summary>
        /// <returns>Player 영웅 아이콘(<see cref="UIIconFieldPlayerHero"/>) 또는 캐스팅 실패 시 null.</returns>
        protected override UIIconCard GetHeroIcon()
        {
            return iconHero as UIIconFieldPlayerHero;
        }
    }
}