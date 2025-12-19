using GGemCo2DCore;

namespace GGemCo2DTcg
{
    public class AttackUnitCommandHandler : ITcgBattleCommandHandler
    {
        public ConfigCommonTcg.TcgBattleCommandType CommandType =>
            ConfigCommonTcg.TcgBattleCommandType.AttackUnit;
        public CommandResult Execute(TcgBattleDataMain context, in TcgBattleCommand cmd)
        {
            var attacker = cmd.Attacker;
            var target   = cmd.targetBattleData;

            if (attacker == null || target == null)
                return CommandResult.Fail("Error_Tcg_NoAttackerOrTarget");

            var actor = context.GetSideState(cmd.Side);
            var opponent = context.GetOpponentState(cmd.Side);
            
            if (!actor.Board.Contains(attacker))
                return CommandResult.Fail("Error_Tcg_NoAttackerOnBoard");

            if (!opponent.Board.Contains(target))
                return CommandResult.Fail("Error_Tcg_NoTargetOnBoard");

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
            attacker.ApplyDamage(target.Attack);

            attacker.CanAttack = false;

            // 사망 처리
            if (target.Hp <= 0)
                opponent.Board.Remove(target);

            if (attacker.Hp <= 0)
                actor.Board.Remove(attacker);
            
            return CommandResult.OkPresentation(new[]
            {
                new TcgPresentationStep(
                    TcgPresentationStepType.AttackUnit,
                    cmd.Side,
                    attacker: actor,
                    target: opponent,
                    fromIndex: attacker.Index,
                    toIndex: target.Index,
                    valueA: attacker.Hp,
                    valueB: target.Hp,
                    valueC: target.Attack,
                    valueD: attacker.Attack
                    )
            });
        }
    }
}