using System.Collections.Generic;
using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 전투 중 플레이어(사람/AI)가 요청하는 단일 액션을 표현하는 데이터 모델.
    /// - 이 클래스 자체는 "어떻게 실행할지" 를 알지 않고,
    ///   오직 "무엇을 할지" 만 표현합니다.
    /// </summary>
    public sealed class TcgBattleCommand
    {
        public ConfigCommonTcg.TcgBattleCommandType CommandType { get; private set; }

        /// <summary>
        /// 이 명령을 요청한 플레이어 측.
        /// </summary>
        public ConfigCommonTcg.TcgPlayerSide Side { get; private set; }

        /// <summary>
        /// 카드 사용 시: 손에서 사용할 카드 런타임 참조.
        /// </summary>
        public TcgBattleDataCardInHand attackerBattleDataCardInHand;

        /// <summary>
        /// 공격 시: 공격자 유닛.
        /// </summary>
        public ConfigCommonTcg.TcgZone attackerZone;
        public TcgBattleDataCardInField attackerBattleDataCardInField;

        /// <summary>
        /// 공격 시: 대상 유닛 (영웅 공격이면 null 로 사용).
        /// </summary>
        public ConfigCommonTcg.TcgZone targetZone;
        public TcgBattleDataCardInField targetBattleDataCardInField;

        /// <summary>
        /// 확장용 추가 데이터 (예: 스펠 턴수, 선택된 옵션 등).
        /// </summary>
        public Dictionary<string, object> extraData;

        private TcgBattleCommand() { }

        public static TcgBattleCommand DrawCardToField(
            ConfigCommonTcg.TcgPlayerSide side,
            ConfigCommonTcg.TcgZone attackerZone,
            ConfigCommonTcg.TcgZone targetZone,
            TcgBattleDataCardInHand attackerBattleDataCardInHand)
        {
            return new TcgBattleCommand
            {
                CommandType = ConfigCommonTcg.TcgBattleCommandType.DrawCardToField,
                Side = side,
                attackerZone = attackerZone,
                attackerBattleDataCardInHand = attackerBattleDataCardInHand,
                targetZone = targetZone
            };
        }
        
        public static TcgBattleCommand AttackUnit(
            ConfigCommonTcg.TcgPlayerSide side,
            ConfigCommonTcg.TcgZone attackerZone,
            TcgBattleDataCardInField attackerBattleDataCardInField,
            ConfigCommonTcg.TcgZone targetZone,
            TcgBattleDataCardInField targetBattleDataCardInField)
        {
            return new TcgBattleCommand
            {
                CommandType = ConfigCommonTcg.TcgBattleCommandType.AttackUnit,
                Side = side,
                attackerZone = attackerZone,
                attackerBattleDataCardInField = attackerBattleDataCardInField,
                targetZone = targetZone,
                targetBattleDataCardInField = targetBattleDataCardInField,
            };
        }
        
        public static TcgBattleCommand UseCardSpell(
            ConfigCommonTcg.TcgPlayerSide side,
            ConfigCommonTcg.TcgZone attackerZone,
            TcgBattleDataCardInHand attackerBattleDataCardInHand,
            ConfigCommonTcg.TcgZone targetZone,
            TcgBattleDataCardInField targetBattleDataCardInField)
        {
            return new TcgBattleCommand
            {
                CommandType = ConfigCommonTcg.TcgBattleCommandType.UseCardSpell,
                Side = side,
                attackerZone = attackerZone,
                attackerBattleDataCardInHand = attackerBattleDataCardInHand,
                targetZone = targetZone,
                targetBattleDataCardInField = targetBattleDataCardInField,
            };
        }
        
        public static TcgBattleCommand UseCardEquipment(
            ConfigCommonTcg.TcgPlayerSide side,
            ConfigCommonTcg.TcgZone attackerZone,
            TcgBattleDataCardInHand attackerBattleDataCardInHand,
            ConfigCommonTcg.TcgZone targetZone,
            TcgBattleDataCardInField targetBattleDataCardInField)
        {
            return new TcgBattleCommand
            {
                CommandType = ConfigCommonTcg.TcgBattleCommandType.UseCardEquipment,
                Side = side,
                attackerZone = attackerZone,
                attackerBattleDataCardInHand = attackerBattleDataCardInHand,
                targetZone = targetZone,
                targetBattleDataCardInField = targetBattleDataCardInField,
            };
        }
        
        /// <summary>
        /// Permanent는 Ability 설정에 따라 zone, index 가 결정 됩니다.
        /// </summary>
        /// <param name="side"></param>
        /// <param name="attackerZone"></param>
        /// <param name="attackerBattleDataCardInHand"></param>
        /// <returns></returns>
        public static TcgBattleCommand UseCardPermanent(
            ConfigCommonTcg.TcgPlayerSide side,
            ConfigCommonTcg.TcgZone attackerZone,
            TcgBattleDataCardInHand attackerBattleDataCardInHand)
        {
            return new TcgBattleCommand
            {
                CommandType = ConfigCommonTcg.TcgBattleCommandType.UseCardPermanent,
                Side = side,
                attackerZone = attackerZone,
                attackerBattleDataCardInHand = attackerBattleDataCardInHand
            };
        }

        public static TcgBattleCommand AttackHero(
            ConfigCommonTcg.TcgPlayerSide side,
            ConfigCommonTcg.TcgZone attackerZone,
            TcgBattleDataCardInField attackerBattleDataCardInField,
            ConfigCommonTcg.TcgZone targetZone,
            TcgBattleDataCardInField targetBattleDataCardInField)
        {
            return new TcgBattleCommand
            {
                CommandType = ConfigCommonTcg.TcgBattleCommandType.AttackHero,
                Side = side,
                attackerZone = attackerZone,
                attackerBattleDataCardInField = attackerBattleDataCardInField,
                targetZone = targetZone,
                targetBattleDataCardInField = targetBattleDataCardInField,
            };
        }

        public static TcgBattleCommand EndTurn(ConfigCommonTcg.TcgPlayerSide side)
        {
            return new TcgBattleCommand
            {
                CommandType = ConfigCommonTcg.TcgBattleCommandType.EndTurn,
                Side = side
            };
        }
        
        public static bool RequiresExplicitTarget(TcgAbilityConstants.TcgAbilityTargetType targetType)
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
        
        public static TcgBattleDataCardInField ResolveExplicitTarget(
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
        
        /// <summary>
        /// Ability의 TargetType에 따라 "자동(랜덤) 타겟"을 선택합니다.
        /// - Permanent의 턴 시작/종료 트리거 등, 명시적 타겟이 없는 상황에서 사용합니다.
        /// - 성능을 위해 후보 리스트를 생성하지 않고, 살아있는 대상(Health > 0)만 대상으로 Reservoir Sampling으로 1개를 선택합니다.
        /// </summary>
        /// <param name="targetType">Ability가 요구하는 타겟 타입</param>
        /// <param name="caster">능력 시전자(아군)</param>
        /// <param name="opponent">상대 진영</param>
        /// <param name="includeHero">
        /// AllyCreature/EnemyCreature/AnyCreature 후보군에 영웅을 포함할지 여부.
        /// (예: Permanent가 영웅도 랜덤 타겟으로 포함해야 하면 true)
        /// </param>
        /// <returns>선택된 타겟(없으면 null)</returns>
        public static TcgBattleDataCardInField ResolveRandomTarget(
            TcgAbilityConstants.TcgAbilityTargetType targetType,
            TcgBattleDataSide caster,
            TcgBattleDataSide opponent,
            bool includeHero = true)
        {
            if (caster == null || opponent == null)
                return null;

            switch (targetType)
            {
                // "자기 자신"은 랜덤이 아니라 항상 본인 영웅으로 해석
                case TcgAbilityConstants.TcgAbilityTargetType.Self:
                    return caster.GetHeroBattleDataCardInFieldByIndex(ConfigCommonTcg.IndexHeroSlot);

                // 영웅 대상은 랜덤이 아니라 해당 진영 영웅으로 고정(요구사항에 따라 여기서도 랜덤화 가능)
                case TcgAbilityConstants.TcgAbilityTargetType.AllyHero:
                    return caster.GetHeroBattleDataCardInFieldByIndex(ConfigCommonTcg.IndexHeroSlot);

                case TcgAbilityConstants.TcgAbilityTargetType.EnemyHero:
                    return opponent.GetHeroBattleDataCardInFieldByIndex(ConfigCommonTcg.IndexHeroSlot);

                // 아군 크리처(옵션: 영웅 포함)
                case TcgAbilityConstants.TcgAbilityTargetType.AllyCreature:
                    return PickRandomAliveFromSide(caster, includeHero);

                // 적군 크리처(옵션: 영웅 포함)
                case TcgAbilityConstants.TcgAbilityTargetType.EnemyCreature:
                    return PickRandomAliveFromSide(opponent, includeHero);

                // 양 진영 중 1개(옵션: 영웅 포함)
                case TcgAbilityConstants.TcgAbilityTargetType.AnyCreature:
                    return PickRandomAliveFromBothSides(caster, opponent, includeHero);

                default:
                    // AllEnemies / AllAllies 등 "다중 대상"은 여기서 단일 타겟을 뽑지 않습니다.
                    return null;
            }
        }
        /// <summary>
        /// 지정 진영의 (필드 + 옵션 영웅) 중 살아있는 대상(Health > 0)을 랜덤으로 1개 선택합니다.
        /// </summary>
        private static TcgBattleDataCardInField PickRandomAliveFromSide(
            TcgBattleDataSide side,
            bool includeHero)
        {
            if (side == null)
                return null;

            // Reservoir Sampling:
            // 살아있는 후보를 순회하며, k번째 후보를 만났을 때 1/k 확률로 교체
            TcgBattleDataCardInField chosen = null;
            int aliveCount = 0;

            var cards = side.Field.Cards;
            for (int i = 0; i < cards.Count; i++)
            {
                var c = cards[i];
                if (c == null || c.Health <= 0)
                    continue;

                aliveCount++;
                // Random.Range(0, aliveCount) == 0 이면 교체 (균등 선택)
                if (Random.Range(0, aliveCount) == 0)
                    chosen = c;
            }

            if (includeHero)
            {
                var hero = side.Field.Hero;
                if (hero != null && hero.Health > 0)
                {
                    aliveCount++;
                    if (Random.Range(0, aliveCount) == 0)
                        chosen = hero;
                }
            }

            return chosen;
        }
        /// <summary>
        /// 양 진영의 (필드 + 옵션 영웅) 중 살아있는 대상(Health > 0)을 랜덤으로 1개 선택합니다.
        /// </summary>
        private static TcgBattleDataCardInField PickRandomAliveFromBothSides(
            TcgBattleDataSide a,
            TcgBattleDataSide b,
            bool includeHero)
        {
            TcgBattleDataCardInField chosen = null;
            int aliveCount = 0;

            // side A
            {
                var cards = a.Field.Cards;
                for (int i = 0; i < cards.Count; i++)
                {
                    var c = cards[i];
                    if (c == null || c.Health <= 0)
                        continue;

                    aliveCount++;
                    if (Random.Range(0, aliveCount) == 0)
                        chosen = c;
                }

                if (includeHero)
                {
                    var hero = a.Field.Hero;
                    if (hero != null && hero.Health > 0)
                    {
                        aliveCount++;
                        if (Random.Range(0, aliveCount) == 0)
                            chosen = hero;
                    }
                }
            }

            // side B
            {
                var cards = b.Field.Cards;
                for (int i = 0; i < cards.Count; i++)
                {
                    var c = cards[i];
                    if (c == null || c.Health <= 0)
                        continue;

                    aliveCount++;
                    if (Random.Range(0, aliveCount) == 0)
                        chosen = c;
                }

                if (includeHero)
                {
                    var hero = b.Field.Hero;
                    if (hero != null && hero.Health > 0)
                    {
                        aliveCount++;
                        if (Random.Range(0, aliveCount) == 0)
                            chosen = hero;
                    }
                }
            }

            return chosen;
        }
    }
}
