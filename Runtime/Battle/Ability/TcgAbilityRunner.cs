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
        /// AbilityType은 반드시 “연출 StepType”을 가진다(없으면 명시적 NoPresentation)
        /// 원칙: TcgAbilityType은 UI가 있는 프로젝트라면 표준 StepType을 갖는다
        /// 예외: 정말 연출이 필요 없는 타입은 NoPresentation로 문서에 명시하고 의도적으로 Factory에서 false를 반환한다
        /// “1 Ability → 1 Step”이 기본, 단 복합 Ability는 “1 Ability → N Step” 허용
        /// BuffAttackHealth 같은 복합 효과는 실제 연출도 두 번(공격/체력) 처리하는 것이 자연스러움
        /// 따라서 Factory는 “다중 Step 생성”을 지원해야 유지보수/확장이 쉬움
        /// </summary>
        private static void RegisterDefaultHandlers()
        {
            RegisterHandler(TcgAbilityConstants.TcgAbilityType.Damage, new TcgAbilityHandlersBasic.Damage());
            RegisterHandler(TcgAbilityConstants.TcgAbilityType.Heal, new TcgAbilityHandlersBasic.Heal());
            RegisterHandler(TcgAbilityConstants.TcgAbilityType.Draw, new TcgAbilityHandlersBasic.Draw());
            RegisterHandler(TcgAbilityConstants.TcgAbilityType.BuffAttack, new TcgAbilityHandlersBasic.BuffAttack());
            RegisterHandler(TcgAbilityConstants.TcgAbilityType.BuffHealth, new TcgAbilityHandlersBasic.BuffHealth());
            RegisterHandler(TcgAbilityConstants.TcgAbilityType.BuffAttackHealth, new TcgAbilityHandlersBasic.BuffAttackHealth());
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

        public static void RunAbility(
            TcgBattleDataMain battleDataMain,
            ConfigCommonTcg.TcgPlayerSide casterSide,
            ConfigCommonTcg.TcgZone casterZone,
            int casterIndex,
            ConfigCommonTcg.TcgZone targetZone,
            int targetIndex,
            IReadOnlyList<TcgAbilityData> abilityDataList,
            TcgAbilityConstants.TcgAbilityTriggerType tcgAbilityTriggerType = TcgAbilityConstants.TcgAbilityTriggerType.None,
            System.Action<TcgAbilityPresentationEvent> presentationEvent = null)
        {
            if (battleDataMain == null || casterSide == ConfigCommonTcg.TcgPlayerSide.None)
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

                var ctx = new TcgAbilityContext(battleDataMain, casterSide, casterZone, casterIndex,
                    targetZone, targetIndex, ability);

                // UI 연출 훅: 실행 직전
                presentationEvent?.Invoke(new TcgAbilityPresentationEvent(
                    TcgAbilityPresentationEvent.Phase.Begin,
                    ability: ability,
                    casterSide: casterSide,
                    casterZone: casterZone,
                    casterIndex: casterIndex,
                    targetZone: targetZone,
                    targetIndex: targetIndex,
                    abilityTriggerType: tcgAbilityTriggerType));

                // Ability 시스템 처리. UI 연출이 아님.
                handler.Execute(ctx);

                // UI 연출 처리
                presentationEvent?.Invoke(new TcgAbilityPresentationEvent(
                    TcgAbilityPresentationEvent.Phase.End,
                    ability: ability,
                    abilityTriggerType: tcgAbilityTriggerType,
                    casterSide: casterSide,
                    casterZone: casterZone,
                    casterIndex: casterIndex,
                    targetZone: targetZone,
                    targetIndex: targetIndex,
                    userData: null));
            }
        }
        
        public static void TryRunOnPlayAbility(
            TcgBattleDataMain battleDataMain,
            ConfigCommonTcg.TcgPlayerSide casterSide,
            ConfigCommonTcg.TcgZone casterZone,
            int casterIndex,
            ConfigCommonTcg.TcgZone targetZone,
            int targetIndex,
            in TcgAbilityDefinition ability,
            List<TcgPresentationStep> steps)
        {
            if (!ability.IsValid)
                return;

            // 도메인: 능력 실행 (타겟 규칙은 상세 테이블의 Ability 정의 기반)
            var list = new List<TcgAbilityData>(1)
            {
                new TcgAbilityData { ability = ability }
            };
            var session = battleDataMain.Owner as TcgBattleSession;

            // Ability 기반 UI 연출도 커맨드 기반 연출과 동일한 타임라인에서 재생될 수 있도록,
            // AbilityPresentationEvent를 PresentationStep으로 변환하여 steps에 누적합니다.
            void PresentationEventBridge(TcgAbilityPresentationEvent ev)
            {
                // 기존 외부 구독(디버그/로그/별도 UI)도 유지
                session?.PublishAbilityPresentation(ev);

                if (steps == null)
                    return;

                // Ability는 다중 스텝을 생성할 수 있으므로(예: BuffAttackHealth),
                // 단일 타임라인 리스트에 누적합니다.
                TcgAbilityPresentationStepFactory.TryCreateSteps(ev, steps);
            }

            RunAbility(
                battleDataMain,
                casterSide,
                casterZone,
                casterIndex,
                targetZone,
                targetIndex,
                list,
                tcgAbilityTriggerType: TcgAbilityConstants.TcgAbilityTriggerType.OnPlay,
                presentationEvent: session != null ? PresentationEventBridge : null);
        }

    }
}
