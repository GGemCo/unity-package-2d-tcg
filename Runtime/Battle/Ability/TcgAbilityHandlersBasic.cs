using System;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 기본 Ability 핸들러 묶음.
    /// - 테이블에서 <see cref="TcgAbilityType"/>로 매핑되어 실행됩니다.
    /// - 타겟 자동 선택은 최소 규칙만 제공합니다(명시 타겟이 없으면 첫 번째 후보 등).
    /// </summary>
    public static class TcgAbilityHandlersBasic
    {
        internal static TcgBattleDataFieldCard ResolveTarget(
            TcgAbilityContext ctx)
        {
            if (ctx.TargetBattleData != null)
                return ctx.TargetBattleData;

            var tt = ctx.Ability.targetType;
            switch (tt)
            {
                case TcgAbilityConstants.TargetType.Self:
                {
                    // SourceCard 가 Creature/Hero가 아니라면 Self 타겟은 명시적으로 주입되는 것이 안전합니다.
                    return null;
                }

                case TcgAbilityConstants.TargetType.EnemyHero:
                    return ctx.Opponent?.Hero?.HeroField;

                case TcgAbilityConstants.TargetType.AllyHero:
                    return ctx.Caster?.Hero?.HeroField;

                case TcgAbilityConstants.TargetType.EnemyCreature:
                {
                    var board = ctx.Opponent?.Board;
                    return (board != null && board.Count > 0) ? board.GetByIndex(0) : null;
                }

                case TcgAbilityConstants.TargetType.AllyCreature:
                {
                    var board = ctx.Caster?.Board;
                    return (board != null && board.Count > 0) ? board.GetByIndex(0) : null;
                }

                case TcgAbilityConstants.TargetType.AnyCreature:
                {
                    var enemyBoard = ctx.Opponent?.Board;
                    if (enemyBoard != null && enemyBoard.Count > 0) return enemyBoard.GetByIndex(0);
                    var allyBoard = ctx.Caster?.Board;
                    return (allyBoard != null && allyBoard.Count > 0) ? allyBoard.GetByIndex(0) : null;
                }

                default:
                    return null;
            }
        }

        public sealed class Damage : ITcgAbilityHandler
        {
            public void Execute(TcgAbilityContext context)
            {
                if (context == null) return;

                var value = context.ParamA;
                if (value <= 0) return;

                var target = ResolveTarget(context);
                if (target == null)
                {
                    GcLogger.LogWarning("[Ability] Damage: Target is null.");
                    return;
                }

                target.ApplyDamage(value);
            }
        }

        public sealed class Heal : ITcgAbilityHandler
        {
            public void Execute(TcgAbilityContext context)
            {
                if (context == null) return;

                var value = context.ParamA;
                if (value <= 0) return;

                var target = ResolveTarget(context);
                if (target == null)
                {
                    GcLogger.LogWarning("[Ability] Heal: Target is null.");
                    return;
                }

                target.Heal(value);
            }
        }

        public sealed class Draw : ITcgAbilityHandler
        {
            public void Execute(TcgAbilityContext context)
            {
                if (context == null) return;
                var drawCount = Math.Max(0, context.ParamA);
                if (drawCount <= 0) return;

                // 드로우는 Caster 기준
                for (int i = 0; i < drawCount; i++)
                    context.Caster?.DrawOneCard();
            }
        }

        public sealed class BuffAttack : ITcgAbilityHandler
        {
            public void Execute(TcgAbilityContext context)
            {
                if (context == null) return;
                var value = context.ParamA;
                if (value == 0) return;

                var target = ResolveTarget(context);
                if (target == null)
                {
                    GcLogger.LogWarning("[Ability] BuffAttack: Target is null.");
                    return;
                }

                target.ModifyAttack(value);
            }
        }

        public sealed class BuffHealth : ITcgAbilityHandler
        {
            public void Execute(TcgAbilityContext context)
            {
                if (context == null) return;
                var value = context.ParamA;
                if (value == 0) return;

                var target = ResolveTarget(context);
                if (target == null)
                {
                    GcLogger.LogWarning("[Ability] BuffHealth: Target is null.");
                    return;
                }

                target.ModifyHealth(value);
            }
        }

        public sealed class GainMana : ITcgAbilityHandler
        {
            public void Execute(TcgAbilityContext context)
            {
                if (context == null) return;
                var value = context.ParamA;
                if (value <= 0) return;
                context.Caster?.Mana?.Add(value);
            }
        }

        public sealed class ExtraAction : ITcgAbilityHandler
        {
            public void Execute(TcgAbilityContext context)
            {
                // TODO: 프로젝트 규칙에 맞게 "추가 행동"을 표현하는 상태/리소스를 도입한 후 구현합니다.
                GcLogger.Log("[Ability] ExtraAction is not implemented yet.");
            }
        }
    }
}
