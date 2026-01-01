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
        /// <summary>
        /// 플레이어 영웅 아이콘을 플레이어 전용 타입으로 캐스팅해 반환합니다.
        /// </summary>
        /// <returns>플레이어 영웅 아이콘(<see cref="UIIconFieldPlayerHero"/>) 또는 null.</returns>
        protected override UIIconCard GetHeroIcon()
        {
            return iconHero as UIIconFieldPlayerHero;
        }
    }
}