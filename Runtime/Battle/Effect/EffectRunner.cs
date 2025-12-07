using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 이펙트를 실행하는 런타임 시스템.
    /// - EffectId -> 핸들러 매핑을 관리하고,
    ///   카드가 가진 여러 이펙트를 순서대로 실행합니다.
    /// </summary>
    public static class EffectRunner
    {
        private static readonly Dictionary<TcgEffectId, ITcgEffectHandler> _handlers;

        static EffectRunner()
        {
            _handlers = new Dictionary<TcgEffectId, ITcgEffectHandler>();

            // 기본 핸들러 등록
            RegisterDefaultHandlers();
        }

        private static void RegisterDefaultHandlers()
        {
            _handlers[TcgEffectId.DealDamageToTargetUnit] =
                new EffectDealDamageToTargetUnit();

            _handlers[TcgEffectId.DealDamageToEnemyHero] =
                new EffectDealDamageToEnemyHero();

            _handlers[TcgEffectId.HealTargetUnit] =
                new EffectHealTargetUnit();

            _handlers[TcgEffectId.DrawCards] =
                new EffectDrawCards();

            // 필요 시 추가 등록
        }

        public static void RegisterHandler(
            TcgEffectId id,
            ITcgEffectHandler handler,
            bool overwrite = false)
        {
            if (_handlers.ContainsKey(id))
            {
                if (!overwrite)
                {
                    GcLogger.LogWarning($"[EffectRunner] Handler for {id} already registered.");
                    return;
                }

                _handlers[id] = handler;
            }
            else
            {
                _handlers.Add(id, handler);
            }
        }

        /// <summary>
        /// 카드가 가진 이펙트 리스트를 순서대로 실행합니다.
        /// </summary>
        public static void RunEffects(
            TcgBattleDataSide caster,
            TcgBattleDataSide opponent,
            TcgBattleDataCard sourceTcgBattleDataCard,
            IReadOnlyList<TcgEffectData> effects,
            TcgBattleDataFieldCard explicitTargetBattleData = null)
        {
            if (effects == null || effects.Count == 0)
                return;

            foreach (var effect in effects)
            {
                if (effect == null || effect.EffectId == TcgEffectId.None)
                    continue;

                if (!_handlers.TryGetValue(effect.EffectId, out var handler) ||
                    handler == null)
                {
                    GcLogger.LogWarning($"[EffectRunner] No handler for EffectId={effect.EffectId}");
                    continue;
                }

                var ctx = new TcgEffectContext(
                    caster,
                    opponent,
                    sourceTcgBattleDataCard,
                    effect.Value,
                    effect.ExtraParams);

                // 타겟 지정이 필요하다면 UI/AI에서 결정한 유닛을 넘겨주거나,
                // 여기에서 TargetType 에 맞게 선택하도록 구현할 수 있습니다.
                ctx.TargetBattleData = explicitTargetBattleData;

                handler.Execute(ctx);
            }
        }
    }
}
