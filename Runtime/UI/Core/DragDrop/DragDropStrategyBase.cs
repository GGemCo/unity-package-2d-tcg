using GGemCo2DCore;

namespace GGemCo2DTcg
{
    public class DragDropStrategyBase
    {
        private TcgBattleManager _battleManager;

        private bool IsWindowHandPlayer(UIWindowConstants.WindowUid windowUid)
        {
            return windowUid == UIWindowConstants.WindowUid.TcgHandPlayer;
        }
        private bool IsWindowFieldPlayer(UIWindowConstants.WindowUid windowUid)
        {
            return windowUid == UIWindowConstants.WindowUid.TcgFieldPlayer;
        }
        private bool IsWindowHandEnemy(UIWindowConstants.WindowUid windowUid)
        {
            return windowUid == UIWindowConstants.WindowUid.TcgHandEnemy;
        }
        private bool IsWindowFieldEnemy(UIWindowConstants.WindowUid windowUid)
        {
            return windowUid == UIWindowConstants.WindowUid.TcgFieldEnemy;
        }

        protected void UseCard(UIWindowConstants.WindowUid droppedUIWindowUid, UIIcon droppedUIIcon, UIWindowConstants.WindowUid targetUIWindowUid, UIIcon targetUIIcon = null)
        {
            UIIconCard iconCard = droppedUIIcon as UIIconCard;
            if (GcLogger.IsNull(iconCard, nameof(droppedUIIcon))) return;
            var attackerIndex = droppedUIIcon.index;

            ConfigCommonTcg.TcgZone attackerZone = ConfigCommonTcg.GetZoneFromWindowUid(droppedUIWindowUid);
            ConfigCommonTcg.TcgZone targetZone = ConfigCommonTcg.GetZoneFromWindowUid(targetUIWindowUid);
            var targetIndex = targetUIIcon != null ? targetUIIcon.index : -1;
            _battleManager ??= TcgPackageManager.Instance.battleManager;

            if (iconCard.IsSpell)
            {
                _battleManager?.UseCardSpell(ConfigCommonTcg.TcgPlayerSide.Player, attackerIndex, targetZone, targetIndex);
            }
            else if (iconCard.IsEquipment)
            {
                _battleManager?.UseCardEquipment(ConfigCommonTcg.TcgPlayerSide.Player, attackerIndex, targetZone, targetIndex);
            }
            else if (iconCard.IsPermanent)
            {
                _battleManager?.UseCardPermanent(ConfigCommonTcg.TcgPlayerSide.Player, attackerIndex);
            }
            else if (iconCard.IsCreature)
            {
                if (IsWindowHandPlayer(droppedUIWindowUid) && IsWindowFieldPlayer(targetUIWindowUid))
                    _battleManager?.DrawCardToField(attackerIndex);
                else if (IsWindowFieldPlayer(droppedUIWindowUid) && IsWindowFieldEnemy(targetUIWindowUid))
                {
                    if (targetIndex == ConfigCommonTcg.IndexHeroSlot)
                    {
                        _battleManager?.AttackHero(ConfigCommonTcg.TcgPlayerSide.Player, attackerZone, attackerIndex, targetZone, targetIndex);   
                    }
                    else
                    {
                        _battleManager?.AttackUnit(ConfigCommonTcg.TcgPlayerSide.Player, attackerZone, attackerIndex, targetZone, targetIndex);    
                    }
                }
                    
            }
        }
    }
}