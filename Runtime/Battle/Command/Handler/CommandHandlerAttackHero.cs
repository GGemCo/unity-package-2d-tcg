using System.Collections.Generic;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 크리처 타입이 영웅을 공격
    /// </summary>
    public class CommandHandlerAttackHero : ITcgBattleCommandHandler
    {
        public ConfigCommonTcg.TcgBattleCommandType CommandType =>
            ConfigCommonTcg.TcgBattleCommandType.AttackHero;
        
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

            if (!opponent.ContainsInFieldHero(target))
                return CommandResult.Fail("Error_Tcg_NoTargetOnHero");

            if (!attacker.CanAttack)
            {
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
            
            // 영웅을 공격 할 때, 크리처는 데미지를 입지 않는다.
            var payload = new TcgBattleUIControllerPayloadAttackUnit(attacker.Health, 0, target.Health, attacker.Attack);
            steps.Add(new TcgPresentationStep(
                TcgPresentationConstants.TcgPresentationStepType.AttackUnit,
                cmd.Side,
                fromZone: attackerZone,
                fromIndex: attackerIndex,
                toZone: targetZone,
                toIndex: targetIndex,
                payload: payload));
            
            steps.Add(new TcgPresentationStep(
                TcgPresentationConstants.TcgPresentationStepType.MoveCardToBack,
                cmd.Side,
                fromZone: attackerZone,
                fromIndex: attackerIndex,
                toIndex: -1,
                toZone: ConfigCommonTcg.TcgZone.None));
            
            return steps.Count > 0 ? CommandResult.OkPresentation(steps.ToArray()) : CommandResult.Ok();
        }
    }
}