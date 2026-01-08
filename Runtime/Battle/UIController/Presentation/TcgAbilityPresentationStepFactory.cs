namespace GGemCo2DTcg
{
    /// <summary>
    /// <see cref="TcgAbilityPresentationEvent"/>를 <see cref="TcgPresentationStep"/> 목록으로 변환하는 팩토리입니다.
    /// Ability 기반 연출도 Command 기반 연출과 동일한 타임라인(단일 Runner)에서 실행되도록 통합합니다.
    /// </summary>
    /// <remarks>
    /// 설계 의도:
    /// - "행위"(카드 이동/공격 등)는 커맨드 Step으로, "효과"(피해/회복/버프 등)는 Ability Step으로 표현합니다.
    /// - Ability 연출은 한 이벤트가 여러 효과를 가질 수 있어(예: 공격력/체력 동시 버프) 다중 Step 생성을 지원합니다.
    /// </remarks>
    public static class TcgAbilityPresentationStepFactory
    {
        /// <summary>
        /// Ability 연출 이벤트를 1개 이상의 Presentation Step으로 변환하여 <paramref name="steps"/>에 추가합니다.
        /// </summary>
        /// <param name="ev">Step 생성의 입력이 되는 Ability 연출 이벤트입니다.</param>
        /// <param name="steps">생성된 Step을 누적할 대상 리스트입니다(널이면 실패).</param>
        /// <returns><paramref name="steps"/>에 Step이 1개 이상 추가되면 true를 반환합니다.</returns>
        /// <remarks>
        /// - 본 구현은 이벤트의 End phase에서만 "효과" Step을 생성합니다.
        /// - 대상이 명확한 경우에만 toZone/toIndex가 채워지는 것을 전제로 하며, 이벤트 값에 따라 비어 있을 수 있습니다.
        /// </remarks>
        public static bool TryCreateSteps(
            in TcgAbilityPresentationEvent ev,
            System.Collections.Generic.List<TcgPresentationStep> steps)
        {
            if (steps == null)
                return false;

            // End 시점만 "효과" 스텝으로 변환합니다.
            if (ev.EventPhase != TcgAbilityPresentationEvent.Phase.End)
                return false;

            // 타겟이 명확한 경우에만 ToIndex/ToWindowUid 를 채웁니다.
            var casterSide = ev.CasterSide;
            var casterZone = ev.CasterZone;
            var casterIndex = ev.CasterIndex;
            var targetZone = ev.TargetZone;
            var targetIndex = ev.TargetIndex;
            var ability = ev.Ability;

            var added = 0;

            // AbilityType 기반으로 "효과" 스텝을 선택합니다.
            switch (ability.abilityType)
            {
                case TcgAbilityConstants.TcgAbilityType.Damage:
                {
                    var payload = new TcgAbilityPayloadDamage(ability.paramA);
                    steps.Add(new TcgPresentationStep(
                        TcgPresentationConstants.TcgPresentationStepType.AbilityDamage,
                        side: casterSide,
                        fromZone: casterZone,
                        fromIndex: casterIndex,
                        toZone: targetZone,
                        toIndex: targetIndex,
                        payload: payload));
                    added++;
                    break;
                }

                case TcgAbilityConstants.TcgAbilityType.Heal:
                {
                    var payload = new TcgAbilityPayloadHeal(ability.paramA);
                    steps.Add(new TcgPresentationStep(
                        TcgPresentationConstants.TcgPresentationStepType.HealPopup,
                        side: casterSide,
                        fromZone: casterZone,
                        fromIndex: casterIndex,
                        toZone: targetZone,
                        toIndex: targetIndex,
                        payload: payload));
                    added++;
                    break;
                }

                case TcgAbilityConstants.TcgAbilityType.BuffAttack:
                case TcgAbilityConstants.TcgAbilityType.BuffHealth:
                {
                    var payload = new TcgAbilityPayloadBuff(ability.abilityType, ability.paramA);
                    steps.Add(new TcgPresentationStep(
                        TcgPresentationConstants.TcgPresentationStepType.ApplyBuff,
                        side: casterSide,
                        fromZone: casterZone,
                        fromIndex: casterIndex,
                        toZone: targetZone,
                        toIndex: targetIndex,
                        payload: payload));
                    added++;
                    break;
                }

                case TcgAbilityConstants.TcgAbilityType.BuffAttackHealth:
                {
                    // 방식 1(권장): 기존 ApplyBuff 핸들러를 재사용하기 위해 2개의 스텝으로 분해합니다.
                    if (ability.paramA != 0)
                    {
                        var payloadA = new TcgAbilityPayloadBuff(TcgAbilityConstants.TcgAbilityType.BuffAttack, ability.paramA);
                        steps.Add(new TcgPresentationStep(
                            TcgPresentationConstants.TcgPresentationStepType.ApplyBuff,
                            side: casterSide,
                            fromZone: casterZone,
                            fromIndex: casterIndex,
                            toZone: targetZone,
                            toIndex: targetIndex,
                            payload: payloadA));
                        added++;
                    }

                    if (ability.paramB != 0)
                    {
                        var payloadB = new TcgAbilityPayloadBuff(TcgAbilityConstants.TcgAbilityType.BuffHealth, ability.paramB);
                        steps.Add(new TcgPresentationStep(
                            TcgPresentationConstants.TcgPresentationStepType.ApplyBuff,
                            side: casterSide,
                            fromZone: casterZone,
                            fromIndex: casterIndex,
                            toZone: targetZone,
                            toIndex: targetIndex,
                            payload: payloadB));
                        added++;
                    }
                    break;
                }

                case TcgAbilityConstants.TcgAbilityType.Draw:
                {
                    var payload = new TcgAbilityPayloadDraw(ability.paramA);
                    steps.Add(new TcgPresentationStep(
                        TcgPresentationConstants.TcgPresentationStepType.AbilityDraw,
                        side: casterSide,
                        fromZone: casterZone,
                        fromIndex: casterIndex,
                        toZone: targetZone,
                        toIndex: targetIndex,
                        payload: payload));
                    added++;
                    break;
                }

                case TcgAbilityConstants.TcgAbilityType.GainMana:
                {
                    var payload = new TcgAbilityPayloadGainMana(ability.paramA);
                    steps.Add(new TcgPresentationStep(
                        TcgPresentationConstants.TcgPresentationStepType.AbilityGainMana,
                        side: casterSide,
                        fromZone: casterZone,
                        fromIndex: casterIndex,
                        toZone: targetZone,
                        toIndex: targetIndex,
                        payload: payload));
                    added++;
                    break;
                }

                case TcgAbilityConstants.TcgAbilityType.ExtraAction:
                {
                    var payload = new TcgAbilityPayloadExtraAction(ability.paramA);
                    steps.Add(new TcgPresentationStep(
                        TcgPresentationConstants.TcgPresentationStepType.AbilityExtraAction,
                        side: casterSide,
                        fromZone: casterZone,
                        fromIndex: casterIndex,
                        toZone: targetZone,
                        toIndex: targetIndex,
                        payload: payload));
                    added++;
                    break;
                }

                default:
                    break;
            }

            return added > 0;
        }

        /// <summary>
        /// Ability 연출 이벤트로부터 단일 <see cref="TcgPresentationStep"/>을 생성합니다.
        /// </summary>
        /// <param name="ev">Step 생성의 입력이 되는 Ability 연출 이벤트입니다.</param>
        /// <param name="step">생성된 Step입니다(실패 시 default).</param>
        /// <returns>생성에 성공하면 true를 반환합니다.</returns>
        /// <remarks>
        /// 호환을 위해 제공되는 API로, 다중 Step이 생성되는 경우 첫 번째 Step만 반환합니다.
        /// </remarks>
        public static bool TryCreateStep(
            in TcgAbilityPresentationEvent ev,
            out TcgPresentationStep step)
        {
            step = default;

            // (호환용) 다중 스텝 중 첫 번째만 반환합니다.
            var tmp = new System.Collections.Generic.List<TcgPresentationStep>(2);
            if (!TryCreateSteps(ev, tmp) || tmp.Count == 0)
                return false;

            step = tmp[0];
            return true;
        }
    }
}
