using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 대상 유닛에게 피해를 주는 이펙트.
    /// - context.TargetUnit 에 타겟이 들어있다고 가정합니다.
    /// </summary>
    public sealed class EffectDealDamageToTargetUnit : ITcgEffectHandler
    {
        public void Execute(TcgEffectContext context)
        {
            var target = context.TargetUnit;
            if (target == null)
            {
                GcLogger.LogWarning("[Effect] DealDamageToTargetUnit: TargetUnit is null.");
                return;
            }

            target.ApplyDamage(context.Value);

            // 사망 처리
            var sideCaster = context.Caster;
            var sideOpponent = context.Opponent;

            if (target.Hp <= 0)
            {
                // 어느 쪽 보드에 있는지 확인 후 제거
                if (sideCaster.Board.Contains(target))
                    sideCaster.Board.Remove(target);
                else if (sideOpponent.Board.Contains(target))
                    sideOpponent.Board.Remove(target);
            }
        }
    }

    /// <summary>
    /// 적 영웅에게 피해를 주는 이펙트.
    /// </summary>
    public sealed class EffectDealDamageToEnemyHero : ITcgEffectHandler
    {
        public void Execute(TcgEffectContext context)
        {
            var opponent = context.Opponent;
            opponent.HeroHp -= context.Value;
            if (opponent.HeroHp < 0)
                opponent.HeroHp = 0;

            // 전투 종료 체크는 BattleManager 쪽에서 턴마다/이펙트마다 검사하도록 구성해도 됩니다.
        }
    }

    /// <summary>
    /// 대상 유닛을 치유하는 이펙트.
    /// </summary>
    public sealed class EffectHealTargetUnit : ITcgEffectHandler
    {
        public void Execute(TcgEffectContext context)
        {
            var target = context.TargetUnit;
            if (target == null)
            {
                GcLogger.LogWarning("[Effect] HealTargetUnit: TargetUnit is null.");
                return;
            }

            target.Heal(context.Value);
        }
    }

    /// <summary>
    /// 카드를 드로우하는 이펙트.
    /// </summary>
    public sealed class EffectDrawCards : ITcgEffectHandler
    {
        public void Execute(TcgEffectContext context)
        {
            var caster = context.Caster;
            var deck   = caster.DeckRuntime;

            for (int i = 0; i < context.Value; i++)
            {
                if (deck.Count == 0)
                    break;

                if (!deck.TryDraw(out var card))
                {
                    break;
                }
                caster.Hand.Add(card);
            }
        }
    }
}
