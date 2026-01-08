using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 드래그&드롭 기반의 카드 사용/공격 입력을 처리하기 위한 공통 전략(Strategy) 베이스 클래스입니다.
    /// 파생 클래스는 UI 이벤트(드롭/드래그 종료 등)에서 이 클래스의 유틸리티 메서드를 호출하여
    /// 카드 타입 및 드롭 위치에 따른 전투 커맨드를 실행합니다.
    /// </summary>
    public class DragDropStrategyBase
    {
        /// <summary>
        /// 전투 로직 진입점인 배틀 매니저입니다.
        /// 최초 사용 시 <see cref="TcgPackageManager"/>로부터 지연 초기화(Lazy initialization)됩니다.
        /// </summary>
        private TcgBattleManager _battleManager;

        /// <summary>
        /// 플레이어 손패(Hand) 윈도우인지 여부를 반환합니다.
        /// </summary>
        /// <param name="windowUid">검사할 UI 윈도우 UID입니다.</param>
        /// <returns>플레이어 손패 윈도우이면 true입니다.</returns>
        private bool IsWindowHandPlayer(UIWindowConstants.WindowUid windowUid)
        {
            return windowUid == UIWindowConstants.WindowUid.TcgHandPlayer;
        }

        /// <summary>
        /// 플레이어 필드(Field) 윈도우인지 여부를 반환합니다.
        /// </summary>
        /// <param name="windowUid">검사할 UI 윈도우 UID입니다.</param>
        /// <returns>플레이어 필드 윈도우이면 true입니다.</returns>
        private bool IsWindowFieldPlayer(UIWindowConstants.WindowUid windowUid)
        {
            return windowUid == UIWindowConstants.WindowUid.TcgFieldPlayer;
        }

        /// <summary>
        /// 적 손패(Hand) 윈도우인지 여부를 반환합니다.
        /// </summary>
        /// <param name="windowUid">검사할 UI 윈도우 UID입니다.</param>
        /// <returns>적 손패 윈도우이면 true입니다.</returns>
        private bool IsWindowHandEnemy(UIWindowConstants.WindowUid windowUid)
        {
            return windowUid == UIWindowConstants.WindowUid.TcgHandEnemy;
        }

        /// <summary>
        /// 적 필드(Field) 윈도우인지 여부를 반환합니다.
        /// </summary>
        /// <param name="windowUid">검사할 UI 윈도우 UID입니다.</param>
        /// <returns>적 필드 윈도우이면 true입니다.</returns>
        private bool IsWindowFieldEnemy(UIWindowConstants.WindowUid windowUid)
        {
            return windowUid == UIWindowConstants.WindowUid.TcgFieldEnemy;
        }

        /// <summary>
        /// 드래그로 집은 카드(<paramref name="droppedUIIcon"/>)를 드롭 대상(<paramref name="targetUIWindowUid"/>/<paramref name="targetUIIcon"/>)에 사용합니다.
        /// 카드 타입(Spell/Equipment/Permanent/Creature) 및 출발/도착 윈도우 조합에 따라
        /// 알맞은 전투 커맨드(사용, 필드 전개, 유닛/영웅 공격)를 호출합니다.
        /// </summary>
        /// <param name="droppedUIWindowUid">드래그 시작(원본) UI 윈도우 UID입니다.</param>
        /// <param name="droppedUIIcon">드래그된 카드 아이콘입니다.</param>
        /// <param name="targetUIWindowUid">드롭 대상 UI 윈도우 UID입니다.</param>
        /// <param name="targetUIIcon">
        /// 드롭 대상의 아이콘입니다(예: 공격 대상 유닛).
        /// 대상이 없는 드롭(예: 빈 슬롯, 필드 자체)에 대해서는 null일 수 있습니다.
        /// </param>
        protected void UseCard(
            UIWindowConstants.WindowUid droppedUIWindowUid,
            UIIcon droppedUIIcon,
            UIWindowConstants.WindowUid targetUIWindowUid,
            UIIcon targetUIIcon = null)
        {
            // 카드 아이콘 타입 검증 (UIIcon -> UIIconCard)
            UIIconCard iconCard = droppedUIIcon as UIIconCard;
            if (GcLogger.IsNull(iconCard, nameof(droppedUIIcon))) return;

            // 공격/사용 주체 인덱스는 드래그된 아이콘 인덱스를 기준으로 합니다.
            var attackerIndex = droppedUIIcon.index;

            // 윈도우 UID를 전투 존(zone)으로 변환합니다.
            ConfigCommonTcg.TcgZone attackerZone = ConfigCommonTcg.GetZoneFromWindowUid(droppedUIWindowUid);
            ConfigCommonTcg.TcgZone targetZone = ConfigCommonTcg.GetZoneFromWindowUid(targetUIWindowUid);

            // 대상이 없는 경우 -1로 처리합니다.
            var targetIndex = targetUIIcon != null ? targetUIIcon.index : -1;

            // 배틀 매니저는 최초 호출 시에만 가져옵니다.
            _battleManager ??= TcgPackageManager.Instance.battleManager;

            // 카드 타입에 따라 서로 다른 사용 규칙/대상 규칙이 적용됩니다.
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
                // Permanent는 대상 없이 사용되는 형태(설계상)로 보이며, target 정보는 사용하지 않습니다.
                _battleManager?.UseCardPermanent(ConfigCommonTcg.TcgPlayerSide.Player, attackerIndex);
            }
            else if (iconCard.IsCreature)
            {
                // 손패 -> 아군 필드 : 유닛 전개(소환)
                if (IsWindowHandPlayer(droppedUIWindowUid) && IsWindowFieldPlayer(targetUIWindowUid))
                {
                    _battleManager?.DrawCardToField(attackerIndex);
                }
                // 아군 필드 -> 적 필드 : 공격
                else if (IsWindowFieldPlayer(droppedUIWindowUid) && IsWindowFieldEnemy(targetUIWindowUid))
                {
                    // 영웅 슬롯 인덱스면 영웅 공격, 그 외는 유닛 공격
                    if (targetIndex == ConfigCommonTcg.IndexHeroSlot)
                    {
                        _battleManager?.AttackHero(
                            ConfigCommonTcg.TcgPlayerSide.Player,
                            attackerZone,
                            attackerIndex,
                            targetZone,
                            targetIndex);
                    }
                    else
                    {
                        _battleManager?.AttackUnit(
                            ConfigCommonTcg.TcgPlayerSide.Player,
                            attackerZone,
                            attackerIndex,
                            targetZone,
                            targetIndex);
                    }
                }
            }
        }
    }
}
