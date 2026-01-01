using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    public class CommandHandlerBase
    {
        protected static void TryRunOnPlayAbility(
            TcgBattleDataMain battleDataMain,
            ConfigCommonTcg.TcgPlayerSide casterSide,
            ConfigCommonTcg.TcgZone casterZone,
            int casterIndex,
            ConfigCommonTcg.TcgZone targetZone,
            int targetIndex,
            in TcgAbilityDefinition ability,
            List<TcgPresentationStep> steps)
        {
            if (!ability.IsValid)
                return;

            // 도메인: 능력 실행 (타겟 규칙은 상세 테이블의 Ability 정의 기반)
            var list = new List<TcgAbilityData>(1)
            {
                new TcgAbilityData { ability = ability }
            };
            var session = battleDataMain.Owner as TcgBattleSession;

            // Ability 기반 UI 연출도 커맨드 기반 연출과 동일한 타임라인에서 재생될 수 있도록,
            // AbilityPresentationEvent를 PresentationStep으로 변환하여 steps에 누적합니다.
            void PresentationEventBridge(TcgAbilityPresentationEvent ev)
            {
                // 기존 외부 구독(디버그/로그/별도 UI)도 유지
                session?.PublishAbilityPresentation(ev);

                if (steps == null)
                    return;

                if (!TcgAbilityPresentationStepFactory.TryCreateStep(ev, out var step))
                    return;

                steps.Add(step);
            }

            TcgAbilityRunner.RunAbility(
                battleDataMain,
                casterSide,
                casterZone,
                casterIndex,
                targetZone,
                targetIndex,
                list,
                tcgAbilityTriggerType: TcgAbilityConstants.TcgAbilityTriggerType.OnPlay,
                presentationEvent: session != null ? PresentationEventBridge : null);
        }
        protected static TcgAbilityDefinition BuildOnPlayAbilityDefinition(TcgBattleDataCardInHand cardInHand)
        {
            if (cardInHand == null) return default;

            switch (cardInHand.Type)
            {
                // 크리처 카드일 때는 Damage Ability를 임시로 생성하여 사용한다.
                case CardConstants.Type.Creature:
                    StruckTableTcgCardSpell struckTableTcgCardSpell = new StruckTableTcgCardSpell
                    {
                        uid = 1,
                        abilityType = TcgAbilityConstants.TcgAbilityType.Damage,
                        tcgAbilityTargetType = TcgAbilityConstants.TcgAbilityTargetType.AllEnemies,
                        tcgAbilityTriggerType = TcgAbilityConstants.TcgAbilityTriggerType.OnPlay,
                        paramA = cardInHand.Attack
                    };
                    return TcgAbilityBuilder.BuildAbility(struckTableTcgCardSpell);
                case CardConstants.Type.Spell:
                    return TcgAbilityBuilder.BuildAbility(cardInHand.SpellDetail);
                case CardConstants.Type.Equipment:
                    return TcgAbilityBuilder.BuildAbility(cardInHand.EquipmentDetail);
                case CardConstants.Type.Permanent:
                    return TcgAbilityBuilder.BuildAbility(cardInHand.PermanentDetail);
                case CardConstants.Type.Event:
                    return TcgAbilityBuilder.BuildAbility(cardInHand.EventDetail);
                default:
                    return default;
            }
        }

        protected static bool RequiresExplicitTarget(TcgAbilityConstants.TcgAbilityTargetType targetType)
        {
            // 단일 대상 선택이 필요한 타입만 true
            switch (targetType)
            {
                case TcgAbilityConstants.TcgAbilityTargetType.AllyCreature:
                case TcgAbilityConstants.TcgAbilityTargetType.EnemyCreature:
                case TcgAbilityConstants.TcgAbilityTargetType.AnyCreature:
                case TcgAbilityConstants.TcgAbilityTargetType.EnemyHero:
                case TcgAbilityConstants.TcgAbilityTargetType.AllyHero:
                    return true;
                default:
                    return false;
            }
        }

        protected static TcgBattleDataCardInField ResolveExplicitTarget(
            TcgAbilityConstants.TcgAbilityTargetType targetType,
            TcgBattleDataSide caster,
            TcgBattleDataSide opponent,
            TcgBattleDataCardInField targetBattleDataCardInField,
            ConfigCommonTcg.TcgZone targetZone)
        {
            if (caster == null || opponent == null)
                return null;

            if (targetBattleDataCardInField.Index < 0) return null;
            // 타겟이 명시적으로 필요하지 않은 경우에는 null을 반환합니다.
            // (AbilityHandler 내부에서 타겟 규칙에 따라 처리하거나, 전체 대상/영웅 대상은 암시적으로 처리)
            switch (targetType)
            {
                case TcgAbilityConstants.TcgAbilityTargetType.Self:
                    if (targetBattleDataCardInField.Index != ConfigCommonTcg.IndexHeroSlot) return null;
                    return caster.GetHeroBattleDataCardInFieldByIndex(targetBattleDataCardInField.Index);

                case TcgAbilityConstants.TcgAbilityTargetType.AllyHero:
                    if (caster.Side != targetBattleDataCardInField.OwnerSide) return null;
                    if (targetBattleDataCardInField.Index != ConfigCommonTcg.IndexHeroSlot) return null;
                    
                    return caster.GetHeroBattleDataCardInFieldByIndex(targetBattleDataCardInField.Index);

                case TcgAbilityConstants.TcgAbilityTargetType.EnemyHero:
                    if (caster.Side == targetBattleDataCardInField.OwnerSide) return null;
                    if (targetBattleDataCardInField.Index != ConfigCommonTcg.IndexHeroSlot) return null;
                    
                    return opponent.GetHeroBattleDataCardInFieldByIndex(targetBattleDataCardInField.Index);

                case TcgAbilityConstants.TcgAbilityTargetType.AllyCreature:
                    if (caster.Side != targetBattleDataCardInField.OwnerSide) return null;
                    
                    return caster.GetBattleDataCardInFieldByIndex(targetBattleDataCardInField.Index);

                case TcgAbilityConstants.TcgAbilityTargetType.EnemyCreature:
                    if (caster.Side == targetBattleDataCardInField.OwnerSide) return null;

                    return opponent.GetBattleDataCardInFieldByIndex(targetBattleDataCardInField.Index);

                case TcgAbilityConstants.TcgAbilityTargetType.AnyCreature:
                {
                    // UI/입력 설계에 따라 어느 쪽을 먼저 보는지가 달라질 수 있으므로,
                    // 기본은 상대편 -> 본인 순으로 탐색합니다.
                    return opponent.GetBattleDataCardInFieldByIndex(targetBattleDataCardInField.Index) ?? caster.GetBattleDataCardInFieldByIndex(targetBattleDataCardInField.Index);
                }

                default:
                    return null;
            }
        }
    }
}