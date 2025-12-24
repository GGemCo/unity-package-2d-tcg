using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 테이블 기반 Ability 실행기.
    /// - <see cref="TableTcgAbility"/>의 AbilityId로 정의를 조회하고,
    ///   <see cref="TcgAbilityType"/>에 따라 핸들러를 선택하여 실행합니다.
    /// </summary>
    public static class TcgAbilityRunner
    {
        private static readonly Dictionary<TcgAbilityType, ITcgAbilityHandler> Handlers;

        static TcgAbilityRunner()
        {
            Handlers = new Dictionary<TcgAbilityType, ITcgAbilityHandler>();
            RegisterDefaultHandlers();
        }

        private static void RegisterDefaultHandlers()
        {
            // 기본 핸들러 등록
            Handlers[TcgAbilityType.Damage] = new TcgAbilityHandlersBasic.Damage();
            Handlers[TcgAbilityType.Heal] = new TcgAbilityHandlersBasic.Heal();
            Handlers[TcgAbilityType.Draw] = new TcgAbilityHandlersBasic.Draw();
            Handlers[TcgAbilityType.BuffAttack] = new TcgAbilityHandlersBasic.BuffAttack();
            Handlers[TcgAbilityType.BuffHealth] = new TcgAbilityHandlersBasic.BuffHealth();
            Handlers[TcgAbilityType.GainMana] = new TcgAbilityHandlersBasic.GainMana();
            Handlers[TcgAbilityType.ExtraAction] = new TcgAbilityHandlersBasic.ExtraAction();
        }

        public static void RegisterHandler(TcgAbilityType type, ITcgAbilityHandler handler, bool overwrite = false)
        {
            if (handler == null) return;
            if (Handlers.TryAdd(type, handler)) return;

            if (!overwrite)
            {
                GcLogger.LogWarning($"[AbilityRunner] Handler for {type} already registered.");
                return;
            }

            Handlers[type] = handler;
        }

        /// <summary>
        /// 카드가 보유한 Ability 리스트를 순서대로 실행합니다.
        /// </summary>
        public static void RunAbility(
            TcgBattleDataMain battleDataMain,
            TcgBattleDataSide caster,
            TcgBattleDataSide opponent,
            TcgBattleDataCard sourceCard,
            IReadOnlyList<TcgAbilityData> abilityDataList,
            TcgBattleDataFieldCard explicitTargetBattleData = null,
            TcgAbilityConstants.TriggerType triggerType = TcgAbilityConstants.TriggerType.None,
            System.Action<TcgAbilityPresentationEvent> presentationEvent = null)
        {
            if (battleDataMain == null || caster == null || opponent == null || sourceCard == null)
                return;
            if (abilityDataList == null || abilityDataList.Count == 0)
                return;

            var abilityTable = TableLoaderManagerTcg.Instance ? TableLoaderManagerTcg.Instance.TableTcgAbility : null;
            if (abilityTable == null)
            {
                GcLogger.LogWarning("[AbilityRunner] TableTcgAbility is null. Ability execution is skipped.");
                return;
            }

            foreach (var data in abilityDataList)
            {
                if (data == null || data.abilityUid <= 0)
                    continue;

                var ability = abilityTable.GetDataByUid(data.abilityUid);
                if (ability == null)
                {
                    GcLogger.LogWarning($"[AbilityRunner] Ability definition not found. AbilityId={data.abilityUid}");
                    continue;
                }

                if (!Handlers.TryGetValue(ability.abilityType, out var handler) || handler == null)
                {
                    GcLogger.LogWarning($"[AbilityRunner] No handler for AbilityType={ability.abilityType} (AbilityId={ability.abilityId})");
                    continue;
                }

                var ctx = new TcgAbilityContext(battleDataMain, caster, opponent, sourceCard, ability)
                {
                    TargetBattleData = data.explicitTarget ?? explicitTargetBattleData
                };

                // UI 연출 훅: 실행 직전
                presentationEvent?.Invoke(new TcgAbilityPresentationEvent(
                    TcgAbilityPresentationEvent.Phase.Begin,
                    abilityUid: data.abilityUid,
                    abilityType: ability.abilityType,
                    casterSide: caster.Side,
                    sourceCard: sourceCard,
                    targetCard: ctx.TargetBattleData,
                    triggerType: triggerType));

                handler.Execute(ctx);

                // UI 연출 훅: 실행 직후
                presentationEvent?.Invoke(new TcgAbilityPresentationEvent(
                    TcgAbilityPresentationEvent.Phase.End,
                    abilityUid: data.abilityUid,
                    abilityType: ability.abilityType,
                    casterSide: caster.Side,
                    sourceCard: sourceCard,
                    targetCard: ctx.TargetBattleData,
                    triggerType: triggerType));
            }
        }
    }
}
