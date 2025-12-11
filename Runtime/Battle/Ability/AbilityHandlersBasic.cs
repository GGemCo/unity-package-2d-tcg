using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 대상 유닛에게 피해를 주는 능력.
    /// - context.TargetUnit 에 타겟이 들어있다고 가정합니다.
    /// </summary>
    public sealed class AbilityDealDamageToTargetUnit : ITcgAbilityHandler
    {
        public void Execute(TcgAbilityContext context)
        {
            var target = context.TargetBattleData;
            if (target == null)
            {
                GcLogger.LogWarning("[Ability] DealDamageToTargetUnit: TargetUnit is null.");
                return;
            }

            target.ApplyDamage(context.Value);

            // 사망 처리
            var sideCaster = context.Caster;
            var sideOpponent = context.Opponent;

            if (target.hp.Value <= 0)
            {
                // 어느 쪽 보드에 있는지 확인 후 제거
                if (sideCaster.ContainsOnBoard(target))
                    sideCaster.RemoveUnitFromBoard(target);
                else if (sideOpponent.ContainsOnBoard(target))
                    sideOpponent.RemoveUnitFromBoard(target);
            }
        }
    }

    /// <summary>
    /// 적 영웅에게 피해를 주는 능력.
    /// </summary>
    public sealed class AbilityDealDamageToEnemyHero : ITcgAbilityHandler
    {
        public void Execute(TcgAbilityContext context)
        {
            var opponent = context.Opponent;
            opponent.TakeHeroDamage(context.Value);

            // 전투 종료 체크는 BattleManager 쪽에서 턴마다/능력마다 검사하도록 구성해도 됩니다.
        }
    }

    /// <summary>
    /// 대상 유닛을 치유하는 능력.
    /// </summary>
    public sealed class AbilityHealTargetUnit : ITcgAbilityHandler
    {
        public void Execute(TcgAbilityContext context)
        {
            var target = context.TargetBattleData;
            if (target == null)
            {
                GcLogger.LogWarning("[Ability] HealTargetUnit: TargetUnit is null.");
                return;
            }

            target.Heal(context.Value);
        }
    }

    /// <summary>
    /// 카드를 드로우하는 능력.
    /// </summary>
    public sealed class AbilityDrawCards : ITcgAbilityHandler
    {
        public void Execute(TcgAbilityContext context)
        {
            var caster = context.Caster;
            var deck   = caster.TcgBattleDataDeck;

            for (int i = 0; i < context.Value; i++)
            {
                if (deck.Count == 0)
                    break;

                if (!deck.TryDraw(out var card))
                {
                    break;
                }
                caster.AddCardToHand(card);
            }
        }
    }
}
