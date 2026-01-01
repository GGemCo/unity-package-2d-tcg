using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// Ability 실행 시 필요한 모든 런타임 컨텍스트 정보를 담는 객체입니다.
    /// 
    /// - 실행 주체(Caster), 상대(Opponent), 전투 전역 정보(BattleDataMain)를 제공합니다.
    /// - 실제 능력 수치 및 설정은 <see cref="TcgAbilityDefinition"/>을 기준으로 합니다.
    /// - 타겟이 UI 또는 AI 로직에서 이미 결정된 경우,
    /// </summary>
    public sealed class TcgAbilityContext
    {
        /// <summary>
        /// 전투 전체를 대표하는 메인 데이터입니다.
        /// 턴, 스택, 공용 규칙 등에 접근하는 데 사용됩니다.
        /// </summary>
        public TcgBattleDataMain BattleDataMain { get; }

        public TcgBattleDataSide CasterBattleDataSide { get; }
        
        public TcgBattleDataCardInField TargetBattleDataCardInField { get; }

        /// <summary>
        /// 실행할 Ability의 정의 데이터입니다.
        /// </summary>
        public TcgAbilityDefinition Ability { get; }


        /// <summary>
        /// Ability 정의에 포함된 첫 번째 정수 파라미터입니다.
        /// (예: 피해량, 회복량, 증가 수치 등)
        /// </summary>
        public int ParamA => Ability.paramA;

        /// <summary>
        /// Ability 정의에 포함된 두 번째 정수 파라미터입니다.
        /// </summary>
        public int ParamB => Ability.paramB;

        /// <summary>
        /// Ability 정의에 포함된 세 번째 정수 파라미터입니다.
        /// </summary>
        public int ParamC => Ability.paramC;

        /// <summary>
        /// Ability 실행을 위한 컨텍스트를 생성합니다.
        /// </summary>
        public TcgAbilityContext(
            TcgBattleDataMain battleDataMain,
            ConfigCommonTcg.TcgPlayerSide casterSide,
            ConfigCommonTcg.TcgZone casterZone,
            int casterIndex,
            ConfigCommonTcg.TcgZone targetZone,
            int targetIndex,
            TcgAbilityDefinition ability)
        {
            BattleDataMain = battleDataMain;
            
            CasterBattleDataSide = battleDataMain.GetSideState(casterSide);

            if (casterSide == ConfigCommonTcg.TcgPlayerSide.Player && targetZone == ConfigCommonTcg.TcgZone.FieldPlayer)
            {
                TargetBattleDataCardInField = CasterBattleDataSide.GetBattleDataCardInFieldByIndex(targetIndex, true);
            }
            else
            {
                var targetDataSide = battleDataMain.GetOpponentState(casterSide);
                if (targetDataSide == null)
                {
                    GcLogger.LogError($"{casterSide}의 상대 {nameof(TcgBattleDataCardInField)} 값이 없습니다.");
                    return;
                }
                TargetBattleDataCardInField = targetDataSide.GetBattleDataCardInFieldByIndex(targetIndex, true);
            }

            Ability = ability;
        }
    }
}
