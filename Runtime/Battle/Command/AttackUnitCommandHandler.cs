namespace GGemCo2DTcg
{
    public class AttackUnitCommandHandler : ITcgBattleCommandHandler
    {
        public ConfigCommonTcg.TcgBattleCommandType CommandType =>
            ConfigCommonTcg.TcgBattleCommandType.AttackUnit;
        public void Execute(TcgBattleDataMain context, in TcgBattleCommand cmd)
        {
            var attacker = cmd.Attacker;
            var target   = cmd.targetBattleData;

            if (attacker == null || target == null)
                return;

            var actor = context.GetSideState(cmd.Side);
            var opponent = context.GetOpponentState(cmd.Side);
            
            if (!actor.ContainsOnBoard(attacker))
                return;

            if (!opponent.ContainsOnBoard(target))
                return;

            if (!attacker.CanAttack)
            {
                // todo. localization
                // _systemMessageManager.ShowMessageWarning("그 캐릭터는 이미 공격을 마쳤습니다.");
                return;
            }

            // 양쪽에 데미지 적용
            target.ApplyDamage(attacker.Attack);
            attacker.ApplyDamage(target.Attack);

            attacker.CanAttack = false;

            // 사망 처리
            if (target.hp.Value <= 0)
                opponent.RemoveUnitFromBoard(target);

            if (attacker.hp.Value <= 0)
                actor.RemoveUnitFromBoard(attacker);
        }
    }
}