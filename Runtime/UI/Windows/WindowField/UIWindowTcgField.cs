using GGemCo2DCore;

namespace GGemCo2DTcg
{
    public class UIWindowTcgField : UIWindow
    {
        private TcgBattleManager _battleManager;
        
        public override void OnShow(bool show)
        {
            
        }
        public void SetBattleManager(TcgBattleManager battleManager)
        {
            _battleManager = battleManager;
        }

        public void RefreshBoard(
            TcgBattleSideState player,
            TcgBattleSideState enemy)
        {
            // 두 보드 상태를 기반으로 필드 레이아웃 갱신
        }

        // 예: 드래그 드랍 후 공격 명령 요청
        public void OnRequestAttackUnit(
            ConfigCommonTcg.TcgPlayerSide side,
            TcgUnitRuntime attacker,
            TcgUnitRuntime target)
        {
            _battleManager?.OnUiRequestAttackUnit(side, attacker, target);
        }

        public void OnRequestAttackHero(
            ConfigCommonTcg.TcgPlayerSide side,
            TcgUnitRuntime attacker)
        {
            _battleManager?.OnUiRequestAttackHero(side, attacker);
        }
    }
}