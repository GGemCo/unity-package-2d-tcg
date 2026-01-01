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
        public static bool TryCreateStep(
            in TcgAbilityPresentationEvent ev,
            out TcgPresentationStep step)
        {
            // 기본값
            step = default;

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
            
            // AbilityType 기반으로 "효과" 스텝을 선택합니다.
            switch (ability.abilityType)
            {
                case TcgAbilityConstants.TcgAbilityType.Damage:
                    TcgAbilityPayloadDamage payloadDamage = new TcgAbilityPayloadDamage(ability.paramA);
                    
                    step = new TcgPresentationStep(
                        TcgPresentationConstants.TcgPresentationStepType.AbilityDamage,
                        side: casterSide,
                        fromZone: casterZone,
                        fromIndex: casterIndex,
                        toZone: targetZone,
                        toIndex: targetIndex,
                        payload: payloadDamage
                        );
                    return true;

                case TcgAbilityConstants.TcgAbilityType.Heal:
                    TcgAbilityPayloadHeal payloadHeal = new TcgAbilityPayloadHeal(ability.paramA);
                    
                    step = new TcgPresentationStep(
                        TcgPresentationConstants.TcgPresentationStepType.HealPopup,
                        side: casterSide,
                        fromZone: casterZone,
                        fromIndex: casterIndex,
                        toZone: targetZone,
                        toIndex: targetIndex,
                        payload: payloadHeal
                        );
                    return true;

                case TcgAbilityConstants.TcgAbilityType.BuffAttack:
                case TcgAbilityConstants.TcgAbilityType.BuffHealth:
                    TcgAbilityPayloadBuff payloadBuff = new TcgAbilityPayloadBuff(ability.abilityType, ability.paramA);
                    
                    step = new TcgPresentationStep(
                        TcgPresentationConstants.TcgPresentationStepType.ApplyBuff,
                        side: casterSide,
                        fromZone: casterZone,
                        fromIndex: casterIndex,
                        toZone: targetZone,
                        toIndex: targetIndex,
                        payload: payloadBuff
                        );
                    return true;

                case TcgAbilityConstants.TcgAbilityType.Draw:
                case TcgAbilityConstants.TcgAbilityType.GainMana:
                case TcgAbilityConstants.TcgAbilityType.ExtraAction:
                default:
                    // 현재 UI 연출이 정의되지 않은 타입은 스텝을 만들지 않습니다.
                    return false;
            }
        }
    }
}
