namespace GGemCo2DTcg
{
    public class AttackHeroCommandHandler : ITcgBattleCommandHandler
    {
        public ConfigCommonTcg.TcgBattleCommandType CommandType =>
            ConfigCommonTcg.TcgBattleCommandType.AttackHero;
        
        public CommandResult Execute(TcgBattleDataMain context, in TcgBattleCommand cmd)
        {
            var attacker = cmd.Attacker;
            var target   = cmd.targetBattleDataHero;

            if (attacker == null || target == null)
                return CommandResult.Fail("Error_Tcg_NoAttackerOrTarget");

            var actor = context.GetSideState(cmd.Side);
            var opponent = context.GetOpponentState(cmd.Side);
            
            if (!actor.Board.Contains(attacker))
                return CommandResult.Fail("Error_Tcg_NoAttackerOnBoard");

            if (!opponent.ContainsInHero(target))
                return CommandResult.Fail("Error_Tcg_NoTargetOnHero");

            if (!attacker.CanAttack)
            {
                // todo. localization
                // _systemMessageManager.ShowMessageWarning("그 캐릭터는 이미 공격을 마쳤습니다.");
                // _systemMessageManager.ShowMessageWarning("이번 턴에 낸 카드는 곧바로 공격할 수 없습니다.");
                return CommandResult.Fail("Error_Tcg_NoAttackedInThisTurn");
            }

            // GcLogger.Log($"{actor.Side} attack to {opponent.Side}");
            // 양쪽에 데미지 적용
            target.ApplyDamage(attacker.Attack);

            attacker.CanAttack = false;

            // 영웅 사망. UIController 에서 연출이 끝나고 TryCheckBattleEnd 함수로 게임 종료 처리
            if (target.Health <= 0)
            {
            }

            return CommandResult.OkPresentation(new[]
            {
                new TcgPresentationStep(
                    TcgPresentationStepType.AttackHero,
                    cmd.Side,
                    attacker: actor,
                    target: opponent,
                    fromIndex: attacker.Index,
                    toIndex: target.Index,
                    valueA: attacker.Health,
                    valueB: target.Health,
                    valueC: target.Attack,
                    valueD: attacker.Attack
                    )
            });
        }
    }
}