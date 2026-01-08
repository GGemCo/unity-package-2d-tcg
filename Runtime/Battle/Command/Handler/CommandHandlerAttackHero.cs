using System.Collections.Generic;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 필드의 크리처가 상대 영웅을 공격하는 커맨드를 처리합니다.
    /// </summary>
    /// <remarks>
    /// - 공격자는 한 턴에 한 번만 공격할 수 있습니다(<see cref="TcgBattleDataCardInField.CanAttack"/>).
    /// - 영웅을 공격하는 경우, 일반적으로 공격자는 반격 피해를 받지 않습니다(연출/스텝 생성 규칙에 반영).
    /// - 전투 종료 판정(영웅 사망 등)은 UI 연출 종료 후 별도 흐름에서 처리될 수 있습니다.
    /// </remarks>
    public class CommandHandlerAttackHero : ITcgBattleCommandHandler
    {
        /// <summary>
        /// 이 핸들러가 처리하는 커맨드 타입입니다.
        /// </summary>
        public ConfigCommonTcg.TcgBattleCommandType CommandType =>
            ConfigCommonTcg.TcgBattleCommandType.AttackHero;

        /// <summary>
        /// 영웅 공격 커맨드를 실행하고, 필요한 경우 UI 연출용 <see cref="TcgPresentationStep"/>들을 반환합니다.
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

            // 대상이 상대 영웅 슬롯에 존재하는지 검증합니다.
            if (!opponent.ContainsInFieldHero(target))
                return CommandResult.Fail("Error_Tcg_NoTargetOnHero");

            // 턴 내 공격 가능 여부(공격 횟수/상태 이상 등)를 확인합니다.
            if (!attacker.CanAttack)
                return CommandResult.Fail("Error_Tcg_NoAttackedInThisTurn");

            // 영웅 공격: 일반적으로 공격자는 반격 피해를 받지 않음(피해는 대상에게만 적용).
            target.ApplyDamage(attacker.Attack);

            // 공격 소진 처리(턴 내 재공격 방지)
            attacker.CanAttack = false;

            // NOTE: 영웅 사망 등 게임 종료 판정은 UI 연출 완료 후 별도 흐름에서 처리될 수 있습니다.
            if (target.Health <= 0)
            {
            }

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

            // 영웅을 공격할 때 크리처는 피해를 입지 않으므로, 공격자 피해(damageToAttacker)는 0으로 전달합니다.
            var payload = new TcgBattleUIControllerPayloadAttackUnit(
                attacker.Health,
                0,
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

            // 공격 종료 후 원위치(또는 대기 위치)로 복귀하는 연출입니다.
            steps.Add(new TcgPresentationStep(
                TcgPresentationConstants.TcgPresentationStepType.MoveCardToBack,
                cmd.Side,
                fromZone: attackerZone,
                fromIndex: attackerIndex,
                toIndex: -1,
                toZone: ConfigCommonTcg.TcgZone.None));

            return steps.Count > 0
                ? CommandResult.OkPresentation(steps.ToArray())
                : CommandResult.Ok();
        }
    }
}
