using System.Collections.Generic;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 손패의 퍼머넌트(Permanent) 카드를 사용(Play)하는 커맨드를 처리합니다.
    /// </summary>
    /// <remarks>
    /// 처리 흐름:
    /// <list type="number">
    /// <item><description>마나를 차감합니다.</description></item>
    /// <item><description>손패에서 카드를 제거합니다.</description></item>
    /// <item><description>퍼머넌트 인스턴스를 생성하여 지속 영역(Permanents)에 등록합니다.</description></item>
    /// <item><description>Lifetime 초기화 훅(<c>OnAdded</c>)을 호출합니다.</description></item>
    /// <item><description>퍼머넌트의 트리거가 OnPlay인 경우, Ability를 실행하고 연출 Step을 타임라인에 누적합니다.</description></item>
    /// <item><description>사용한 카드는 소모되어 무덤으로 이동하는 연출을 추가합니다.</description></item>
    /// </list>
    /// </remarks>
    public sealed class CommandHandlerUseCardPermanent : CommandHandlerBase, ITcgBattleCommandHandler
    {
        /// <summary>
        /// 이 핸들러가 처리하는 커맨드 타입입니다.
        /// </summary>
        public ConfigCommonTcg.TcgBattleCommandType CommandType =>
            ConfigCommonTcg.TcgBattleCommandType.UseCardPermanent;

        /// <summary>
        /// 퍼머넌트 카드 사용 커맨드를 실행하고, 필요한 경우 UI 연출용 <see cref="TcgPresentationStep"/>들을 반환합니다.
        /// </summary>
        /// <param name="context">현재 전투 상태 컨텍스트입니다.</param>
        /// <param name="cmd">실행할 전투 커맨드 데이터입니다.</param>
        /// <returns>
        /// 처리 결과입니다. 연출이 필요한 경우 <see cref="CommandResult.OkPresentation"/> 형태로 Step을 포함할 수 있습니다.
        /// </returns>
        /// <remarks>
        /// - Permanent는 필드(크리처)와 별개로 지속 영역에 등록됩니다.
        /// - OnPlay 트리거의 경우, 타겟이 필요한 Ability는 무작위 타겟을 해석할 수 있습니다(정책에 따른 설계).
        /// </remarks>
        public CommandResult Execute(TcgBattleDataMain context, in TcgBattleCommand cmd)
        {
            if (context == null)
                return CommandResult.Fail("Error_Tcg_InvalidContext");

            var attackerZone = cmd.attackerZone;
            var card = cmd.attackerBattleDataCardInHand;

            // NOTE: Permanent 사용은 일반적으로 명시적 타겟이 없거나, Ability 레벨에서 타겟을 해석합니다.
            //       이 구현에서는 Step/Ability 실행 시점에만 타겟(필요 시)을 결정합니다.
            var targetZone = ConfigCommonTcg.TcgZone.None;

            if (card == null)
                return CommandResult.Fail("Error_Tcg_NoCard");

            if (card.PermanentDetail == null)
                return CommandResult.Fail("Error_Tcg_NoPermanentDetail");

            var actor = context.GetSideState(cmd.Side);
            var opponent = context.GetOpponentState(cmd.Side);

            // 1) 마나 차감
            if (!actor.TryConsumeMana(card.Cost))
                return CommandResult.Fail("Error_Tcg_NotEnoughMana");

            // 2) 손에서 제거
            if (!actor.Hand.TryRemove(card, out int fromIndex))
                return CommandResult.Fail("Error_Tcg_NoCardInHand");

            var steps = new List<TcgPresentationStep>(6);

            // 3) Permanent는 지속 영역(Permanents)에 등록합니다(필드/보드와 별개로 관리).
            var inst = new TcgBattlePermanentInstance(card, card.PermanentDetail, attackerZone);
            actor.Permanents.Add(inst);

            // 4) Lifetime 초기화 훅
            inst.Lifetime?.OnAdded(new TcgPermanentLifetimeContext(
                context.TurnCount,
                TcgAbilityConstants.TcgAbilityTriggerType.None,
                inst));

            // 5) OnPlay 트리거라면 즉시 Ability를 실행합니다.
            if (card.PermanentDetail.TcgAbilityTriggerType == TcgAbilityConstants.TcgAbilityTriggerType.OnPlay)
            {
                var ability = TcgAbilityBuilder.BuildAbility(card.PermanentDetail);

                // 타겟이 필요한 Ability는 무작위 타겟을 선택합니다(히어로 포함 가능).
                // NOTE: "무작위 타겟" 정책은 게임 디자인에 따라 변경될 수 있으므로, 규칙 변경 시 이 지점을 우선 확인합니다.
                TcgBattleDataCardInField explicitTarget = null;
                if (TcgBattleCommand.RequiresExplicitTarget(ability.tcgAbilityTargetType))
                {
                    explicitTarget = TcgBattleCommand.ResolveRandomTarget(
                        ability.tcgAbilityTargetType,
                        actor,
                        opponent,
                        includeHero: true);

                    if (explicitTarget == null)
                        return CommandResult.Fail("Error_Tcg_TargetRequired");
                }

                // 도메인: OnPlay Ability 실행 + (발행된 이벤트를) Step으로 변환하여 동일 타임라인에 누적
                TcgAbilityRunner.TryRunOnPlayAbility(
                    context,
                    cmd.Side,
                    attackerZone,
                    fromIndex,
                    targetZone,
                    explicitTarget?.Index ?? -1,
                    ability,
                    steps);
            }

            // 6) 사용한 카드는 소모되어 무덤으로 이동합니다.
            steps.Add(new TcgPresentationStep(
                TcgPresentationConstants.TcgPresentationStepType.MoveCardToGrave,
                cmd.Side,
                fromZone: attackerZone,
                fromIndex: fromIndex,
                toIndex: -1,
                toZone: ConfigCommonTcg.TcgZone.None));

            return steps.Count > 0
                ? CommandResult.OkPresentation(steps.ToArray())
                : CommandResult.Ok();
        }
    }
}
