namespace GGemCo2DTcg
{
    public class AttackHeroCommandHandler : ITcgBattleCommandHandler
    {
        public ConfigCommonTcg.TcgBattleCommandType CommandType =>
            ConfigCommonTcg.TcgBattleCommandType.AttackHero;
        
        public void Execute(TcgBattleDataMain context, in TcgBattleCommand cmd)
        {
            var attacker = cmd.Attacker;
            if (attacker == null)
                return;

            var actor = context.GetSideState(cmd.Side);
            var opponent = context.GetOpponentState(cmd.Side);
            
            if (!actor.ContainsOnBoard(attacker))
                return;

            if (!attacker.CanAttack)
            {
                // todo. localization
                // _systemMessageManager.ShowMessageWarning("그 캐릭터는 이미 공격을 마쳤습니다.");
                return;
            }

            opponent.TakeHeroDamage(attacker.Attack);
            attacker.CanAttack = false;

            // TODO: 영웅 HP 0 이하이면 전투 종료 처리
            if (opponent.HeroHp <= 0)
            {
                OnBattleEnd(actor.Side);
            }
        }

        private void OnBattleEnd(ConfigCommonTcg.TcgPlayerSide actorSide)
        {
        }

    }
}