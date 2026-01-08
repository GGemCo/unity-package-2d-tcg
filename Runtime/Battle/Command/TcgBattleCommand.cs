using System.Collections.Generic;
using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 전투 중 플레이어(사람/AI)가 요청하는 단일 액션(커맨드)을 표현하는 데이터 모델입니다.
    /// </summary>
    /// <remarks>
    /// 이 타입은 “무엇을 할지(의도/파라미터)”만 담고, “어떻게 실행할지(로직)”는 알지 않습니다.
    /// 실제 실행은 <c>ITcgBattleCommandHandler</c> 구현체(핸들러)에서 담당합니다.
    /// </remarks>
    public sealed class TcgBattleCommand
    {
        /// <summary>
        /// 이 커맨드의 타입입니다.
        /// </summary>
        public ConfigCommonTcg.TcgBattleCommandType CommandType { get; private set; }

        /// <summary>
        /// 이 명령을 요청한 플레이어의 진영(Side)입니다.
        /// </summary>
        public ConfigCommonTcg.TcgPlayerSide Side { get; private set; }

        /// <summary>
        /// 카드 사용 시: 손패에서 사용할 카드 런타임 참조입니다.
        /// </summary>
        public TcgBattleDataCardInHand attackerBattleDataCardInHand;

        /// <summary>
        /// 공격/시전 등의 실행 주체(공격자/시전자)가 위치한 Zone입니다.
        /// </summary>
        public ConfigCommonTcg.TcgZone attackerZone;

        /// <summary>
        /// 공격 시: 공격자 유닛(필드 카드) 참조입니다.
        /// </summary>
        public TcgBattleDataCardInField attackerBattleDataCardInField;

        /// <summary>
        /// 공격/시전의 대상이 위치한 Zone입니다.
        /// </summary>
        public ConfigCommonTcg.TcgZone targetZone;

        /// <summary>
        /// 공격/시전 시: 대상 유닛(필드 카드) 참조입니다.
        /// 영웅 대상 선택이 불가능/불필요한 커맨드에서는 null이 될 수 있습니다.
        /// </summary>
        public TcgBattleDataCardInField targetBattleDataCardInField;

        /// <summary>
        /// 확장용 추가 데이터입니다(예: 선택 옵션, 추가 파라미터 등).
        /// </summary>
        public Dictionary<string, object> extraData;

        /// <summary>
        /// 외부에서 임의 생성하지 못하도록 생성자를 숨깁니다.
        /// </summary>
        private TcgBattleCommand() { }

        /// <summary>
        /// 손패 카드를 필드로 이동(플레이/소환)하는 커맨드를 생성합니다.
        /// </summary>
        /// <param name="side">커맨드를 요청한 진영(Side)입니다.</param>
        /// <param name="attackerZone">카드가 출발하는 Zone(보통 손패)입니다.</param>
        /// <param name="targetZone">카드가 도착하는 Zone(보통 필드)입니다.</param>
        /// <param name="attackerBattleDataCardInHand">사용할 손패 카드 참조입니다.</param>
        /// <returns>생성된 커맨드 인스턴스입니다.</returns>
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

        /// <summary>
        /// 유닛이 유닛을 공격하는 커맨드를 생성합니다.
        /// </summary>
        /// <param name="side">커맨드를 요청한 진영(Side)입니다.</param>
        /// <param name="attackerZone">공격자가 위치한 Zone입니다.</param>
        /// <param name="attackerBattleDataCardInField">공격자 유닛 참조입니다.</param>
        /// <param name="targetZone">대상이 위치한 Zone입니다.</param>
        /// <param name="targetBattleDataCardInField">대상 유닛 참조입니다.</param>
        /// <returns>생성된 커맨드 인스턴스입니다.</returns>
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

        /// <summary>
        /// 스펠(Spell) 카드를 사용하는 커맨드를 생성합니다.
        /// </summary>
        /// <param name="side">커맨드를 요청한 진영(Side)입니다.</param>
        /// <param name="attackerZone">카드가 출발하는 Zone(보통 손패)입니다.</param>
        /// <param name="attackerBattleDataCardInHand">사용할 손패 카드 참조입니다.</param>
        /// <param name="targetZone">대상(명시적 타겟)이 위치한 Zone입니다.</param>
        /// <param name="targetBattleDataCardInField">대상(명시적 타겟) 참조입니다(없으면 null일 수 있음).</param>
        /// <returns>생성된 커맨드 인스턴스입니다.</returns>
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

        /// <summary>
        /// 장비(Equipment) 카드를 사용하는 커맨드를 생성합니다.
        /// </summary>
        /// <param name="side">커맨드를 요청한 진영(Side)입니다.</param>
        /// <param name="attackerZone">카드가 출발하는 Zone(보통 손패)입니다.</param>
        /// <param name="attackerBattleDataCardInHand">사용할 손패 카드 참조입니다.</param>
        /// <param name="targetZone">대상(명시적 타겟)이 위치한 Zone입니다.</param>
        /// <param name="targetBattleDataCardInField">대상(명시적 타겟) 참조입니다(없으면 null일 수 있음).</param>
        /// <returns>생성된 커맨드 인스턴스입니다.</returns>
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
        /// 퍼머넌트(Permanent) 카드를 사용하는 커맨드를 생성합니다.
        /// </summary>
        /// <remarks>
        /// Permanent는 카드/Ability 설정에 따라 실제 타겟/등록 위치 등이 결정될 수 있어,
        /// 커맨드 생성 시점에는 명시적 타겟을 담지 않는 형태를 허용합니다.
        /// </remarks>
        /// <param name="side">커맨드를 요청한 진영(Side)입니다.</param>
        /// <param name="attackerZone">카드가 출발하는 Zone(보통 손패)입니다.</param>
        /// <param name="attackerBattleDataCardInHand">사용할 손패 카드 참조입니다.</param>
        /// <returns>생성된 커맨드 인스턴스입니다.</returns>
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

        /// <summary>
        /// 유닛이 상대 영웅을 공격하는 커맨드를 생성합니다.
        /// </summary>
        /// <param name="side">커맨드를 요청한 진영(Side)입니다.</param>
        /// <param name="attackerZone">공격자가 위치한 Zone입니다.</param>
        /// <param name="attackerBattleDataCardInField">공격자 유닛 참조입니다.</param>
        /// <param name="targetZone">대상 영웅이 위치한 Zone입니다.</param>
        /// <param name="targetBattleDataCardInField">대상 영웅 참조입니다.</param>
        /// <returns>생성된 커맨드 인스턴스입니다.</returns>
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

        /// <summary>
        /// 턴 종료 커맨드를 생성합니다.
        /// </summary>
        /// <param name="side">커맨드를 요청한 진영(Side)입니다.</param>
        /// <returns>생성된 커맨드 인스턴스입니다.</returns>
        public static TcgBattleCommand EndTurn(ConfigCommonTcg.TcgPlayerSide side)
        {
            return new TcgBattleCommand
            {
                CommandType = ConfigCommonTcg.TcgBattleCommandType.EndTurn,
                Side = side
            };
        }

        /// <summary>
        /// Ability의 타겟 타입이 “명시적 단일 대상 선택”을 요구하는지 여부를 반환합니다.
        /// </summary>
        /// <param name="targetType">Ability가 요구하는 타겟 타입입니다.</param>
        /// <returns>명시적 단일 대상 선택이 필요하면 true, 그렇지 않으면 false입니다.</returns>
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

        /// <summary>
        /// 입력(커맨드)으로 전달된 대상 정보를 검증하고, Ability의 TargetType에 맞는 “명시적 타겟”을 해석합니다.
        /// </summary>
        /// <param name="targetType">Ability가 요구하는 타겟 타입입니다.</param>
        /// <param name="caster">능력 시전자 진영(아군) 상태입니다.</param>
        /// <param name="opponent">상대 진영 상태입니다.</param>
        /// <param name="targetBattleDataCardInField">UI/입력으로 전달된 대상 참조입니다.</param>
        /// <param name="targetZone">UI/입력으로 전달된 대상 Zone입니다(현재 구현에서는 참고 용도로만 사용될 수 있음).</param>
        /// <returns>해석된 타겟(유효하지 않으면 null)입니다.</returns>
        /// <remarks>
        /// 타겟이 명시적으로 필요하지 않은 경우에는 null을 반환합니다.
        /// (AbilityHandler 내부에서 타겟 규칙에 따라 처리하거나, 전체 대상/영웅 대상은 암시적으로 처리)
        /// </remarks>
        public static TcgBattleDataCardInField ResolveExplicitTarget(
            TcgAbilityConstants.TcgAbilityTargetType targetType,
            TcgBattleDataSide caster,
            TcgBattleDataSide opponent,
            TcgBattleDataCardInField targetBattleDataCardInField,
            ConfigCommonTcg.TcgZone targetZone)
        {
            if (caster == null || opponent == null)
                return null;

            // NOTE: targetBattleDataCardInField가 null일 수 있는 입력이라면 NRE 위험이 있습니다.
            //       현재 계약상 null이 들어오지 않는다고 가정합니다.
            if (targetBattleDataCardInField.Index < 0) return null;

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
                    return opponent.GetBattleDataCardInFieldByIndex(targetBattleDataCardInField.Index)
                           ?? caster.GetBattleDataCardInFieldByIndex(targetBattleDataCardInField.Index);
                }

                default:
                    return null;
            }
        }

        /// <summary>
        /// Ability의 TargetType에 따라 “자동(랜덤) 타겟”을 선택합니다.
        /// </summary>
        /// <remarks>
        /// - Permanent의 턴 시작/종료 트리거 등, 명시적 타겟이 없는 상황에서 사용합니다.
        /// - 성능을 위해 후보 리스트를 생성하지 않고, 살아있는 대상(Health &gt; 0)만 대상으로
        ///   Reservoir Sampling으로 1개를 균등 선택합니다.
        /// </remarks>
        /// <param name="targetType">Ability가 요구하는 타겟 타입입니다.</param>
        /// <param name="caster">능력 시전자(아군) 상태입니다.</param>
        /// <param name="opponent">상대 진영 상태입니다.</param>
        /// <param name="includeHero">
        /// AllyCreature/EnemyCreature/AnyCreature 후보군에 영웅을 포함할지 여부입니다.
        /// (예: Permanent가 영웅도 랜덤 타겟으로 포함해야 하면 true)
        /// </param>
        /// <returns>선택된 타겟(없으면 null)입니다.</returns>
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
        /// 지정 진영의 (필드 + 옵션 영웅) 중 살아있는 대상(Health &gt; 0)을 랜덤으로 1개 선택합니다.
        /// </summary>
        /// <param name="side">대상 진영 상태입니다.</param>
        /// <param name="includeHero">후보군에 영웅을 포함할지 여부입니다.</param>
        /// <returns>선택된 타겟(없으면 null)입니다.</returns>
        private static TcgBattleDataCardInField PickRandomAliveFromSide(
            TcgBattleDataSide side,
            bool includeHero)
        {
            if (side == null)
                return null;

            // Reservoir Sampling:
            // 살아있는 후보를 순회하며, k번째 후보를 만났을 때 1/k 확률로 교체(균등 선택)
            TcgBattleDataCardInField chosen = null;
            int aliveCount = 0;

            var cards = side.Field.Cards;
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
        /// 양 진영의 (필드 + 옵션 영웅) 중 살아있는 대상(Health &gt; 0)을 랜덤으로 1개 선택합니다.
        /// </summary>
        /// <param name="a">진영 A 상태입니다.</param>
        /// <param name="b">진영 B 상태입니다.</param>
        /// <param name="includeHero">후보군에 영웅을 포함할지 여부입니다.</param>
        /// <returns>선택된 타겟(없으면 null)입니다.</returns>
        private static TcgBattleDataCardInField PickRandomAliveFromBothSides(
            TcgBattleDataSide a,
            TcgBattleDataSide b,
            bool includeHero)
        {
            // NOTE: a/b가 null일 수 있는 호출 경로가 있다면 NRE 위험이 있습니다.
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
