using System.Collections.Generic;

namespace GGemCo2DTcg
{
    public class CommandHandlerAttackUnit : CommandHandlerBase, ITcgBattleCommandHandler
    {
        public ConfigCommonTcg.TcgBattleCommandType CommandType =>
            ConfigCommonTcg.TcgBattleCommandType.AttackUnit;
        
        public CommandResult Execute(TcgBattleDataMain context, in TcgBattleCommand cmd)
        {
            var attackerZone = cmd.attackerZone;
            var attacker = cmd.attackerBattleDataCardInField;
            
            var targetZone = cmd.targetZone;
            var target   = cmd.targetBattleDataCardInField;

            if (attacker == null || target == null)
                return CommandResult.Fail("Error_Tcg_NoAttackerOrTarget");
            
            var attackerIndex = attacker.Index;
            var targetIndex = target.Index;
            
            var actor = context.GetSideState(cmd.Side);
            var opponent = context.GetOpponentState(cmd.Side);
            
            if (!actor.ContainsInField(attacker))
                return CommandResult.Fail("Error_Tcg_NoAttackerOnField");

            if (!opponent.ContainsInField(target))
                return CommandResult.Fail("Error_Tcg_NoTargetOnField");

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

            // 사망 처리
            if (attacker.Health <= 0)
            {
                actor.Field.Remove(attacker);
            }
            if (target.Health <= 0)
            {
                opponent.Field.Remove(target);
            }
            attacker.CanAttack = false;

            var steps = new List<TcgPresentationStep>(6);
            
            // 1) 공통: 캐스팅/투사체 연출
            // - 효과(피해/회복/버프 등)는 Ability 기반 Step에서 처리합니다.
            steps.Add(new TcgPresentationStep(
                TcgPresentationConstants.TcgPresentationStepType.MoveCardToTarget,
                cmd.Side,
                fromZone: attackerZone,
                fromIndex: attackerIndex,
                toZone: targetZone,
                toIndex: targetIndex));

            var payload = new TcgBattleUIControllerPayloadAttackUnit(attacker.Health, target.Attack, target.Health, attacker.Attack);
            steps.Add(new TcgPresentationStep(
                TcgPresentationConstants.TcgPresentationStepType.AttackUnit,
                cmd.Side,
                fromZone: attackerZone,
                fromIndex: attackerIndex,
                toZone: targetZone,
                toIndex: targetIndex,
                payload: payload));
            
            // 사망 처리
            if (attacker.Health <= 0)
            {
                steps.Add(new TcgPresentationStep(
                    TcgPresentationConstants.TcgPresentationStepType.MoveCardToGrave,
                    cmd.Side,
                    fromZone: attackerZone,
                    fromIndex: attackerIndex,
                    toIndex: -1,
                    toZone: ConfigCommonTcg.TcgZone.None));
            }
            else
            {
                steps.Add(new TcgPresentationStep(
                    TcgPresentationConstants.TcgPresentationStepType.MoveCardToBack,
                    cmd.Side,
                    fromZone: attackerZone,
                    fromIndex: attackerIndex,
                    toIndex: -1,
                    toZone: ConfigCommonTcg.TcgZone.None));
            }
            if (target.Health <= 0)
            {
                steps.Add(new TcgPresentationStep(
                    TcgPresentationConstants.TcgPresentationStepType.MoveCardToGrave,
                    cmd.Side,
                    fromZone: targetZone,
                    fromIndex: targetIndex,
                    toIndex: -1,
                    toZone: ConfigCommonTcg.TcgZone.None));
            }
            else
            {
                steps.Add(new TcgPresentationStep(
                    TcgPresentationConstants.TcgPresentationStepType.MoveCardToBack,
                    cmd.Side,
                    fromZone: targetZone,
                    fromIndex: targetIndex,
                    toIndex: -1,
                    toZone: ConfigCommonTcg.TcgZone.None));
            }
            
            return steps.Count > 0 ? CommandResult.OkPresentation(steps.ToArray()) : CommandResult.Ok();
        }
    }
}