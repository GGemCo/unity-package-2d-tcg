namespace GGemCo2DTcg
{
    /// <summary>
    /// <see cref="TcgAbilityPresentationEvent"/>를 <see cref="TcgPresentationStep"/>으로 변환하는 팩토리입니다.
    /// 
    /// 목적:
    /// - Ability 기반 연출도 Command 기반 연출과 동일한 타임라인(단일 Runner)에서 실행되도록 통합합니다.
    /// - "행위"(카드 이동/공격 등)는 커맨드 Step으로,
    ///   "효과"(피해/회복/버프 등)는 Ability Step으로 표현합니다.
    /// </summary>
    public static class TcgAbilityPresentationStepFactory
    {
        /// <summary>
        /// Ability 연출은 단일 스텝으로 끝나지 않을 수 있으므로,
        /// (예: 공격/체력 동시 버프) 다중 스텝 생성을 지원합니다.
        /// </summary>
        /// <returns>steps에 1개 이상 추가되었으면 true입니다.</returns>
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
