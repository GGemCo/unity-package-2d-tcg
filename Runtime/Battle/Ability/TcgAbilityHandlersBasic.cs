using System;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 기본 Ability 타입에 대한 실행 핸들러 모음입니다.
    /// 
    /// - 테이블에서 정의된 <see cref="TcgAbilityConstants.TcgAbilityType"/> 값에 따라
    ///   대응되는 핸들러가 선택되어 실행됩니다.
    /// - 명시적인 타겟이 지정되지 않은 경우, 최소한의 규칙으로 자동 타겟을 결정합니다.
    ///   (예: 첫 번째 크리처, 영웅 등)
    /// </summary>
    public static class TcgAbilityHandlersBasic
    {
        /// <summary>
        /// Damage Ability 실행 핸들러입니다.
        /// </summary>
        public sealed class Damage : ITcgAbilityHandler
        {
            /// <summary>
            /// 대상에게 피해를 적용합니다.
            /// </summary>
            /// <param name="context">Ability 실행 컨텍스트입니다.</param>
            public void Execute(TcgAbilityContext context)
            {
                if (GcLogger.IsNull(context, $"[Ability] Damage: {nameof(TcgAbilityContext)} is null.")) return;

                var value = context.ParamA;
                if (value <= 0) return;

                var target = context.TargetBattleDataCardInField;
                if (GcLogger.IsNull(target, "[Ability] Damage: Target is null.")) return;

                target.ApplyDamage(value);
            }
        }

        /// <summary>
        /// Heal Ability 실행 핸들러입니다.
        /// </summary>
        public sealed class Heal : ITcgAbilityHandler
        {
            /// <summary>
            /// 대상의 체력을 회복시킵니다.
            /// </summary>
            /// <param name="context">Ability 실행 컨텍스트입니다.</param>
            public void Execute(TcgAbilityContext context)
            {
                if (context == null) return;

                var value = context.ParamA;
                if (value <= 0) return;

                var target = context.TargetBattleDataCardInField;
                if (target == null)
                {
                    GcLogger.LogWarning("[Ability] Heal: Target is null.");
                    return;
                }

                target.Heal(value);
            }
        }

        /// <summary>
        /// Draw Ability 실행 핸들러입니다.
        /// </summary>
        public sealed class Draw : ITcgAbilityHandler
        {
            /// <summary>
            /// 시전자 기준으로 카드를 드로우합니다.
            /// </summary>
            /// <param name="context">Ability 실행 컨텍스트입니다.</param>
            public void Execute(TcgAbilityContext context)
            {
                if (context == null) return;

                var drawCount = Math.Max(0, context.ParamA);
                if (drawCount <= 0) return;

                for (int i = 0; i < drawCount; i++)
                    context.CasterBattleDataSide?.DrawOneCard();
            }
        }

        /// <summary>
        /// 공격력 증가 버프 Ability 실행 핸들러입니다.
        /// </summary>
        public sealed class BuffAttack : ITcgAbilityHandler
        {
            /// <summary>
            /// 대상의 공격력을 변경합니다.
            /// </summary>
            /// <param name="context">Ability 실행 컨텍스트입니다.</param>
            public void Execute(TcgAbilityContext context)
            {
                if (context == null) return;

                var value = context.ParamA;
                if (value == 0) return;

                var target = context.TargetBattleDataCardInField;
                if (target == null)
                {
                    GcLogger.LogWarning("[Ability] BuffAttack: Target is null.");
                    return;
                }

                target.ModifyAttack(value);
            }
        }

        /// <summary>
        /// 체력 증가 버프 Ability 실행 핸들러입니다.
        /// </summary>
        public sealed class BuffHealth : ITcgAbilityHandler
        {
            /// <summary>
            /// 대상의 체력을 변경합니다.
            /// </summary>
            /// <param name="context">Ability 실행 컨텍스트입니다.</param>
            public void Execute(TcgAbilityContext context)
            {
                if (context == null) return;

                var value = context.ParamA;
                if (value == 0) return;

                var target = context.TargetBattleDataCardInField;
                if (target == null)
                {
                    GcLogger.LogWarning("[Ability] BuffHealth: Target is null.");
                    return;
                }

                target.ModifyHealth(value);
            }
        }

        /// <summary>
        /// 마나 획득 Ability 실행 핸들러입니다.
        /// </summary>
        public sealed class GainMana : ITcgAbilityHandler
        {
            /// <summary>
            /// 시전자에게 마나를 추가합니다.
            /// </summary>
            /// <param name="context">Ability 실행 컨텍스트입니다.</param>
            public void Execute(TcgAbilityContext context)
            {
                if (context == null) return;

                var value = context.ParamA;
                if (value <= 0) return;

                context.CasterBattleDataSide?.Mana?.Add(value);
            }
        }

        /// <summary>
        /// 추가 행동 Ability 실행 핸들러입니다.
        /// </summary>
        public sealed class ExtraAction : ITcgAbilityHandler
        {
            /// <summary>
            /// 추가 행동을 부여합니다.
            /// (현재는 미구현 상태입니다.)
            /// </summary>
            /// <param name="context">Ability 실행 컨텍스트입니다.</param>
            public void Execute(TcgAbilityContext context)
            {
                // TODO: 프로젝트 규칙에 맞는 "추가 행동" 상태/리소스 도입 후 구현
                GcLogger.Log("[Ability] ExtraAction is not implemented yet.");
            }
        }
    }
}
