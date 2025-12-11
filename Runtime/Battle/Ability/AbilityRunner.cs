using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 카드 능력을 실행하는 런타임 시스템.
    /// - AbilityId -> 핸들러 매핑을 관리하고,
    ///   카드가 가진 여러 카드 능력을 순서대로 실행합니다.
    /// </summary>
    public static class AbilityRunner
    {
        private static readonly Dictionary<TcgAbilityId, ITcgAbilityHandler> Handlers;

        static AbilityRunner()
        {
            Handlers = new Dictionary<TcgAbilityId, ITcgAbilityHandler>();

            // 기본 핸들러 등록
            RegisterDefaultHandlers();
        }

        private static void RegisterDefaultHandlers()
        {
            Handlers[TcgAbilityId.DealDamageToTargetUnit] =
                new AbilityDealDamageToTargetUnit();

            Handlers[TcgAbilityId.DealDamageToEnemyHero] =
                new AbilityDealDamageToEnemyHero();

            Handlers[TcgAbilityId.HealTargetUnit] =
                new AbilityHealTargetUnit();

            Handlers[TcgAbilityId.DrawCards] =
                new AbilityDrawCards();

            // 필요 시 추가 등록
        }

        public static void RegisterHandler(
            TcgAbilityId id,
            ITcgAbilityHandler handler,
            bool overwrite = false)
        {
            if (Handlers.TryAdd(id, handler)) return;
            
            if (!overwrite)
            {
                GcLogger.LogWarning($"[AbilityRunner] Handler for {id} already registered.");
                return;
            }

            Handlers[id] = handler;
        }

        /// <summary>
        /// 카드가 가진 능력 리스트를 순서대로 실행합니다.
        /// </summary>
        public static void RunAbility(
            TcgBattleDataSide caster,
            TcgBattleDataSide opponent,
            TcgBattleDataCard sourceTcgBattleDataCard,
            IReadOnlyList<TcgAbilityData> abilityDataList,
            TcgBattleDataFieldCard explicitTargetBattleData = null)
        {
            if (abilityDataList == null || abilityDataList.Count == 0)
                return;

            foreach (var abilityData in abilityDataList)
            {
                if (abilityData == null || abilityData.abilityId == TcgAbilityId.None)
                    continue;

                if (!Handlers.TryGetValue(abilityData.abilityId, out var handler) ||
                    handler == null)
                {
                    GcLogger.LogWarning($"[AbilityRunner] No handler for AbilityId={abilityData.abilityId}");
                    continue;
                }

                var ctx = new TcgAbilityContext(
                    caster,
                    opponent,
                    sourceTcgBattleDataCard,
                    abilityData.value,
                    abilityData.extraParams)
                {
                    // 타겟 지정이 필요하다면 UI/AI에서 결정한 유닛을 넘겨주거나,
                    // 여기에서 TargetType 에 맞게 선택하도록 구현할 수 있습니다.
                    TargetBattleData = explicitTargetBattleData
                };

                handler.Execute(ctx);
            }
        }
    }
}
