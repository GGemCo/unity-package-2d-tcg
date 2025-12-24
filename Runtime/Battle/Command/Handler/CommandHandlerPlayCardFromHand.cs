using System.Collections.Generic;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 손패에서 카드를 사용(Play)하는 커맨드를 처리합니다.
    /// - 코스트 소모
    /// - 손패 제거
    /// - 카드 타입에 따른 처리(Creature 소환, Spell/Equipment 즉시 실행, Permanent/Event 등록 등)
    /// </summary>
    public sealed class CommandHandlerPlayCardFromHand : ITcgBattleCommandHandler
    {
        public ConfigCommonTcg.TcgBattleCommandType CommandType =>
            ConfigCommonTcg.TcgBattleCommandType.PlayCardFromHand;

        public CommandResult Execute(TcgBattleDataMain context, in TcgBattleCommand cmd)
        {
            if (context == null)
                return CommandResult.Fail("Error_Tcg_InvalidContext");

            var card = cmd.tcgBattleDataCard;
            if (card == null)
                return CommandResult.Fail("Error_Tcg_NoCard");

            var actor = context.GetSideState(cmd.Side);
            var opponent = context.GetOpponentState(cmd.Side);

            // 1) 마나 차감
            if (!actor.TryConsumeMana(card.Cost))
                return CommandResult.Fail("Error_Tcg_NotEnoughMana");

            // 2) 손에서 제거
            if (!actor.Hand.TryRemove(card, out int fromIndex))
                return CommandResult.Fail("Error_Tcg_NoCardInHand");

            var steps = new List<TcgPresentationStep>(6);

            // 3) 카드 타입에 따른 처리
            switch (card.Type)
            {
                case CardConstants.Type.Creature:
                {
                    var unit = TcgBattleDataCardFactory.CreateBattleDataFieldCard(actor.Side, card);
                    if (unit == null)
                        return CommandResult.Fail("Error_Tcg_CreateUnitFailed");

                    int toIndex = actor.Board.Add(unit);

                    steps.Add(new TcgPresentationStep(
                        TcgPresentationStepType.MoveCardHandToBoard,
                        cmd.Side,
                        fromIndex: fromIndex,
                        toIndex: toIndex,
                        attacker: actor,
                        target: null));
                    break;
                }

                case CardConstants.Type.Spell:
                {
                    // 스펠은 즉시 능력 실행 후 소모
                    TryRunOnPlayAbility(context, actor, opponent, card, card.SpellDetail?.abilityUid, steps);

                    steps.Add(new TcgPresentationStep(
                        TcgPresentationStepType.PlaySpellCast,
                        cmd.Side,
                        fromIndex: fromIndex,
                        toIndex: -1,
                        attacker: actor,
                        target: opponent));

                    steps.Add(new TcgPresentationStep(
                        TcgPresentationStepType.MoveCardHandToGrave,
                        cmd.Side,
                        fromIndex: fromIndex,
                        toIndex: -1,
                        attacker: actor,
                        target: null));
                    break;
                }

                case CardConstants.Type.Equipment:
                {
                    // 장비는 현재 패키지에서 "장착 슬롯" UI/데이터가 분리되어 있지 않으므로,
                    // 1) OnPlay 능력 실행(버프/디버프/효과)
                    // 2) 카드 소모(Grave) 처리
                    // 로 최소 기능을 제공합니다.
                    TryRunOnPlayAbility(context, actor, opponent, card, card.EquipmentDetail?.abilityUid, steps);

                    steps.Add(new TcgPresentationStep(
                        TcgPresentationStepType.PlayEffectOnTarget,
                        cmd.Side,
                        fromIndex: fromIndex,
                        toIndex: -1,
                        attacker: actor,
                        target: null));

                    steps.Add(new TcgPresentationStep(
                        TcgPresentationStepType.MoveCardHandToGrave,
                        cmd.Side,
                        fromIndex: fromIndex,
                        toIndex: -1,
                        attacker: actor,
                        target: null));
                    break;
                }

                case CardConstants.Type.Permanent:
                {
                    // Permanent는 지속 영역에 등록 (필드/보드와 별개로 관리)
                    if (card.PermanentDetail != null)
                    {
                        var inst = new TcgBattlePermanentInstance(card, card.PermanentDetail);
                        actor.Permanents.Add(inst);

                        if (card.PermanentDetail.triggerType == TcgAbilityConstants.TriggerType.OnPlay)
                        {
                            TryRunOnPlayAbility(context, actor, opponent, card, card.PermanentDetail.abilityUid, steps);
                        }
                    }

                    // UI 관점에서는 "배치"로 표현(필요 시 별도 StepType 추가 권장)
                    steps.Add(new TcgPresentationStep(
                        TcgPresentationStepType.MoveCardHandToBoard,
                        cmd.Side,
                        fromIndex: fromIndex,
                        toIndex: 0,
                        attacker: actor,
                        target: null));
                    break;
                }

                case CardConstants.Type.Event:
                {
                    if (card.EventDetail != null)
                    {
                        var inst = new TcgBattleEventInstance(card, card.EventDetail);
                        actor.Events.Add(inst);

                        if (card.EventDetail.triggerType == TcgAbilityConstants.TriggerType.OnPlay)
                        {
                            TryRunOnPlayAbility(context, actor, opponent, card, card.EventDetail.abilityUid, steps);

                            if (card.EventDetail.consumeOnTrigger)
                            {
                                actor.Events.Remove(inst);
                                steps.Add(new TcgPresentationStep(
                                    TcgPresentationStepType.MoveCardHandToGrave,
                                    cmd.Side,
                                    fromIndex: fromIndex,
                                    toIndex: -1,
                                    attacker: actor,
                                    target: null));
                            }
                            else
                            {
                                // 소비하지 않는 이벤트는 "등록"만 수행하므로, 연출은 필요 시 확장
                            }
                        }
                    }
                    break;
                }

                default:
                {
                    // 기타 타입은 추후 확장
                    steps.Add(new TcgPresentationStep(
                        TcgPresentationStepType.MoveCardHandToGrave,
                        cmd.Side,
                        fromIndex: fromIndex,
                        toIndex: -1,
                        attacker: actor,
                        target: null));
                    break;
                }
            }

            return steps.Count > 0 ? CommandResult.OkPresentation(steps.ToArray()) : CommandResult.Ok();
        }

        private static void TryRunOnPlayAbility(
            TcgBattleDataMain battleDataMain,
            TcgBattleDataSide caster,
            TcgBattleDataSide opponent,
            TcgBattleDataCard sourceCard,
            int? abilityUid,
            List<TcgPresentationStep> steps)
        {
            if (!abilityUid.HasValue || abilityUid.Value <= 0)
                return;

            // 도메인: 능력 실행 (타겟 규칙은 tcg_ability 정의 기반)
            var list = new List<TcgAbilityData>(1)
            {
                new TcgAbilityData { abilityUid = abilityUid.Value }
            };
            var session = battleDataMain.Owner as TcgBattleSession;
            TcgAbilityRunner.RunAbility(
                battleDataMain,
                caster,
                opponent,
                sourceCard,
                list,
                explicitTargetBattleData: null,
                triggerType: TcgAbilityConstants.TriggerType.OnPlay,
                presentationEvent: session != null ? session.PublishAbilityPresentation : null);

            // 연출(선택): 실제 타겟/값은 AbilityRunner/Handler에서 Step으로 올리는 구조로 확장 가능
            steps?.Add(new TcgPresentationStep(
                TcgPresentationStepType.PlayEffectOnTarget,
                caster.Side,
                fromIndex: -1,
                toIndex: -1,
                attacker: caster,
                target: opponent));
        }
    }
}
