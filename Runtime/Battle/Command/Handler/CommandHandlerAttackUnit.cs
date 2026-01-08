using System.Collections.Generic;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 필드의 유닛(크리처)이 상대 유닛을 공격하는 커맨드를 처리합니다.
    /// </summary>
    /// <remarks>
    /// - 공격자는 한 턴에 한 번만 공격할 수 있습니다(<see cref="TcgBattleDataCardInField.CanAttack"/>).
    /// - 유닛 간 전투는 상호 피해를 적용하며, 체력이 0 이하가 되면 필드에서 제거됩니다.
    /// - UI 연출은 <see cref="TcgPresentationStep"/> 목록으로 반환합니다.
    /// </remarks>
    public class CommandHandlerAttackUnit : CommandHandlerBase, ITcgBattleCommandHandler
    {
        /// <summary>
        /// 이 핸들러가 처리하는 커맨드 타입입니다.
        /// </summary>
        public ConfigCommonTcg.TcgBattleCommandType CommandType =>
            ConfigCommonTcg.TcgBattleCommandType.AttackUnit;

        /// <summary>
        /// 유닛 공격 커맨드를 실행하고, 필요한 경우 UI 연출용 <see cref="TcgPresentationStep"/>들을 반환합니다.
        /// </summary>
        /// <param name="context">현재 전투 상태 컨텍스트입니다.</param>
        /// <param name="cmd">실행할 전투 커맨드 데이터입니다.</param>
        /// <returns>
        /// 처리 결과입니다. 연출이 필요한 경우 <see cref="CommandResult.OkPresentation"/> 형태로 Step을 포함할 수 있습니다.
        /// </returns>
        public CommandResult Execute(TcgBattleDataMain context, in TcgBattleCommand cmd)
        {
            var attackerZone = cmd.attackerZone;
            var attacker = cmd.attackerBattleDataCardInField;

            var targetZone = cmd.targetZone;
            var target = cmd.targetBattleDataCardInField;

            if (attacker == null || target == null)
                return CommandResult.Fail("Error_Tcg_NoAttackerOrTarget");

            var attackerIndex = attacker.Index;
            var targetIndex = target.Index;

            var actor = context.GetSideState(cmd.Side);
            var opponent = context.GetOpponentState(cmd.Side);

            // 공격자가 실제로 필드에 존재하는지 검증합니다.
            if (!actor.ContainsInField(attacker))
                return CommandResult.Fail("Error_Tcg_NoAttackerOnField");

            // 대상이 상대 필드에 존재하는지 검증합니다.
            if (!opponent.ContainsInField(target))
                return CommandResult.Fail("Error_Tcg_NoTargetOnField");

            // 턴 내 공격 가능 여부(공격 횟수/상태 이상 등)를 확인합니다.
            if (!attacker.CanAttack)
                return CommandResult.Fail("Error_Tcg_NoAttackedInThisTurn");

            // 도메인 전투 처리: 상호 피해 적용
            target.ApplyDamage(attacker.Attack);
            attacker.ApplyDamage(target.Attack);

            // 도메인 사망 처리: 체력이 0 이하인 유닛은 필드에서 제거합니다.
            if (attacker.Health <= 0)
                actor.Field.Remove(attacker);

            if (target.Health <= 0)
                opponent.Field.Remove(target);

            // 공격 소진 처리(턴 내 재공격 방지)
            attacker.CanAttack = false;

            var steps = new List<TcgPresentationStep>(6);

            // 1) 공통: 공격 시작(이동/캐스팅/투사체) 연출
            // - 실제 효과(피해/회복/버프 등)는 Ability 기반 Step에서 처리하는 정책을 따를 수 있습니다.
            steps.Add(new TcgPresentationStep(
                TcgPresentationConstants.TcgPresentationStepType.MoveCardToTarget,
                cmd.Side,
                fromZone: attackerZone,
                fromIndex: attackerIndex,
                toZone: targetZone,
                toIndex: targetIndex));

            // 공격 결과(현재 체력/입힌 피해량)를 UI로 전달합니다.
            // NOTE: payload 파라미터 의미는 타입 정의에 맞춰 유지됩니다.
            var payload = new TcgBattleUIControllerPayloadAttackUnit(
                attacker.Health,
                target.Attack,
                target.Health,
                attacker.Attack);

            steps.Add(new TcgPresentationStep(
                TcgPresentationConstants.TcgPresentationStepType.AttackUnit,
                cmd.Side,
                fromZone: attackerZone,
                fromIndex: attackerIndex,
                toZone: targetZone,
                toIndex: targetIndex,
                payload: payload));

            // 공격자 연출: 사망 시 무덤, 생존 시 복귀
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

            // 대상 연출: 사망 시 무덤, 생존 시 복귀
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

            return steps.Count > 0
                ? CommandResult.OkPresentation(steps.ToArray())
                : CommandResult.Ok();
        }
    }
}
