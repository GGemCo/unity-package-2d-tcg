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
        /// <summary>
        /// 플레이어 영웅 아이콘을 플레이어 전용 타입으로 캐스팅해 반환합니다.
        /// </summary>
        /// <returns>플레이어 영웅 아이콘(<see cref="UIIconFieldEnemyHero"/>) 또는 null.</returns>
        protected override UIIconCard GetHeroIcon()
        {
            return iconHero as UIIconFieldEnemyHero;
        }
    }
}