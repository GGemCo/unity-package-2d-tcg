using GGemCo2DCore;

namespace GGemCo2DTcg
{
    public class UIWindowTcgFieldEnemy : UIWindow
    {
        private TcgBattleManager _battleManager;
        private TcgBattleControllerBase _battleController;
        
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
        public void SetBattleManager(TcgBattleManager battleManager, TcgBattleControllerBase controller)
        {
            _battleManager = battleManager;
            _battleController = controller;
        }

        public void RefreshBoard()
        {
            DetachAllIcons();
            var cards = _battleController.GetBoardCards();
            int i = 0;
            foreach (var battleDataFieldCard in cards)
            {
                var uiIcon = SetIconCount(i, battleDataFieldCard.Uid, 1);
                if (!uiIcon) continue;
                var uiIconFieldEnemy = uiIcon as UIIconFieldEnemy;
                if (!uiIconFieldEnemy) continue;
                uiIconFieldEnemy.SetBattleDataFieldCard(battleDataFieldCard);
                i++;
            }
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