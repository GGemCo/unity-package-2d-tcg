using GGemCo2DCore;

namespace GGemCo2DTcg
{
    public class UIWindowTcgFieldEnemy : UIWindow
    {
        private TcgBattleManager _battleManager;
        
        protected override void Awake()
        {
            uid = UIWindowConstants.WindowUid.TcgFieldEnemy;

            base.Awake();

            IconPoolManager.SetSetIconHandler(new SetIconHandlerFieldEnemy());
            DragDropHandler.SetStrategy(new DragDropStrategyFieldEnemy());
        }
        public override void OnShow(bool show)
        {
            DetachAllIcons();
        }

        public void SetBattleManager(TcgBattleManager battleManager)
        {
            _battleManager = battleManager;
        }

        public void RefreshBoard(
            TcgBattleDataSide player,
            TcgBattleDataSide enemy)
        {
            // 두 보드 상태를 기반으로 필드 레이아웃 갱신
        }

        // 예: 드래그 드랍 후 공격 명령 요청
        public void OnRequestAttackUnit(
            ConfigCommonTcg.TcgPlayerSide side,
            TcgBattleDataFieldCard attacker,
            TcgBattleDataFieldCard target)
        {
            // _battleManager?.OnUiRequestAttackUnit(side, attacker, target);
        }

        public void OnRequestAttackHero(
            ConfigCommonTcg.TcgPlayerSide side,
            TcgBattleDataFieldCard attacker)
        {
            // _battleManager?.OnUiRequestAttackHero(side, attacker);
        }
    }
}