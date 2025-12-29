using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 테이블 기반 Ability 실행기입니다.
    /// 
    /// - <c>tcg_card_*</c> 테이블에서 로드된 Ability 정의를 기반으로 실행합니다.
    /// - <see cref="TcgAbilityConstants.TcgAbilityType"/>에 따라 등록된 핸들러를 선택하여 호출합니다.
    /// - 실행 전/후에 <see cref="TcgAbilityPresentationEvent"/>를 발행할 수 있어,
    ///   UI 연출 레이어를 도메인 로직과 분리하여 연결할 수 있습니다.
    /// </summary>
    public static class TcgAbilityRunner
    {
        /// <summary>
        /// AbilityType → Handler 매핑 테이블입니다.
        /// </summary>
        private static readonly Dictionary<TcgAbilityConstants.TcgAbilityType, ITcgAbilityHandler> Handlers;

        /// <summary>
        /// 기본 핸들러를 초기 등록합니다.
        /// </summary>
        static TcgAbilityRunner()
        {
            Handlers = new Dictionary<TcgAbilityConstants.TcgAbilityType, ITcgAbilityHandler>();
            RegisterDefaultHandlers();
        }

        /// <summary>
        /// 프로젝트에서 제공하는 기본 Ability 핸들러들을 등록합니다.
        /// </summary>
        private static void RegisterDefaultHandlers()
        {
            RegisterHandler(TcgAbilityConstants.TcgAbilityType.Damage, new TcgAbilityHandlersBasic.Damage());
            RegisterHandler(TcgAbilityConstants.TcgAbilityType.Heal, new TcgAbilityHandlersBasic.Heal());
            RegisterHandler(TcgAbilityConstants.TcgAbilityType.Draw, new TcgAbilityHandlersBasic.Draw());
            RegisterHandler(TcgAbilityConstants.TcgAbilityType.BuffAttack, new TcgAbilityHandlersBasic.BuffAttack());
            RegisterHandler(TcgAbilityConstants.TcgAbilityType.BuffHealth, new TcgAbilityHandlersBasic.BuffHealth());
            RegisterHandler(TcgAbilityConstants.TcgAbilityType.GainMana, new TcgAbilityHandlersBasic.GainMana());
            RegisterHandler(TcgAbilityConstants.TcgAbilityType.ExtraAction, new TcgAbilityHandlersBasic.ExtraAction());
        }

        /// <summary>
        /// Ability 타입에 대한 실행 핸들러를 등록합니다.
        /// </summary>
        /// <param name="type">등록할 Ability 타입입니다.</param>
        /// <param name="handler">해당 타입을 처리할 핸들러 인스턴스입니다.</param>
        /// <param name="overwrite">
        /// 이미 등록된 핸들러가 있을 때 덮어쓸지 여부입니다.
        /// false인 경우 중복 등록은 경고 로그 후 무시됩니다.
        /// </param>
        private static void RegisterHandler(
            TcgAbilityConstants.TcgAbilityType type,
            ITcgAbilityHandler handler,
            bool overwrite = false)
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
        /// <param name="battleDataMain">전투 전체를 대표하는 메인 데이터입니다.</param>
        /// <param name="caster">Ability를 시전하는 주체(시전자)입니다.</param>
        /// <param name="opponent">시전자 기준 상대편입니다.</param>
        /// <param name="sourceCard">Ability를 발생시킨 원본 카드 데이터입니다.</param>
        /// <param name="abilityDataList">실행할 Ability 데이터 목록(순서대로 실행)입니다.</param>
        /// <param name="explicitTargetBattleData">
        /// 외부에서 강제로 지정하는 타겟입니다.
        /// 각 Ability 항목에 <c>data.explicitTarget</c>이 존재하면 그것이 우선합니다.
        /// </param>
        /// <param name="tcgAbilityTriggerType">
        /// 트리거 기반 실행(예: OnDraw/OnTurnStart)일 때 트리거 타입을 전달합니다.
        /// 기본값은 <see cref="TcgAbilityConstants.TcgAbilityTriggerType.None"/>입니다.
        /// </param>
        /// <param name="presentationEvent">
        /// UI 연출 훅입니다. null이면 연출 이벤트를 발행하지 않습니다.
        /// Begin/End 두 번 호출됩니다.
        /// </param>
        public static void RunAbility(
            TcgBattleDataMain battleDataMain,
            TcgBattleDataSide caster,
            TcgBattleDataSide opponent,
            TcgBattleDataCard sourceCard,
            IReadOnlyList<TcgAbilityData> abilityDataList,
            TcgBattleDataFieldCard explicitTargetBattleData = null,
            TcgAbilityConstants.TcgAbilityTriggerType tcgAbilityTriggerType = TcgAbilityConstants.TcgAbilityTriggerType.None,
            System.Action<TcgAbilityPresentationEvent> presentationEvent = null)
        {
            if (battleDataMain == null || caster == null || opponent == null || sourceCard == null)
                return;

            if (abilityDataList == null || abilityDataList.Count == 0)
                return;

            foreach (var data in abilityDataList)
            {
                if (data == null || !data.ability.IsValid)
                    continue;

                var ability = data.ability;

                if (!Handlers.TryGetValue(ability.abilityType, out var handler) || handler == null)
                {
                    GcLogger.LogWarning($"[AbilityRunner] No handler. AbilityType={ability.abilityType} (AbilityUid={ability.uid})");
                    continue;
                }

                var ctx = new TcgAbilityContext(battleDataMain, caster, opponent, sourceCard, ability)
                {
                    // 개별 Ability의 explicitTarget이 있으면 그것이 우선, 없으면 외부 explicitTarget을 사용합니다.
                    TargetBattleData = data.explicitTarget ?? explicitTargetBattleData
                };

                // UI 연출 훅: 실행 직전
                presentationEvent?.Invoke(new TcgAbilityPresentationEvent(
                    TcgAbilityPresentationEvent.Phase.Begin,
                    abilityUid: ability.uid,
                    abilityType: ability.abilityType,
                    casterSide: caster.Side,
                    sourceCard: sourceCard,
                    targetCard: ctx.TargetBattleData,
                    tcgAbilityTriggerType: tcgAbilityTriggerType));

                handler.Execute(ctx);

                // UI 연출 훅: 실행 직후
                presentationEvent?.Invoke(new TcgAbilityPresentationEvent(
                    TcgAbilityPresentationEvent.Phase.End,
                    abilityUid: ability.uid,
                    abilityType: ability.abilityType,
                    casterSide: caster.Side,
                    sourceCard: sourceCard,
                    targetCard: ctx.TargetBattleData,
                    tcgAbilityTriggerType: tcgAbilityTriggerType));
            }
        }
    }
}
