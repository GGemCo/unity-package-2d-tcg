using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// Ability 실행에 필요한 런타임 컨텍스트(전투 상태, 시전자/대상, 능력 정의)를 묶어 전달하는 객체입니다.
    /// </summary>
    /// <remarks>
    /// - 실행 주체(캐스터)의 전투 데이터(<see cref="CasterBattleDataSide"/>)와 전투 전역 정보(<see cref="BattleDataMain"/>)를 제공합니다.
    /// - 실제 능력 수치/타겟 규칙은 <see cref="Ability"/>(<see cref="TcgAbilityDefinition"/>)를 기준으로 해석됩니다.
    /// - 대상이 UI/AI 등에서 이미 결정된 경우, 대상 인덱스(<c>targetIndex</c>)를 통해 필드 카드(<see cref="TargetBattleDataCardInField"/>)를 조회합니다.
    /// NOTE: 현재 구현은 targetZone/casterZone/casterIndex의 일부 인자를 로직에 사용하지 않으며,
    ///       대상 조회 실패 시 로그를 남기고 생성자를 조기 종료하여 일부 프로퍼티가 null일 수 있습니다.
    /// </remarks>
    public sealed class TcgAbilityContext
    {
        /// <summary>
        /// 전투 전체를 대표하는 메인 데이터입니다.
        /// 턴, 스택, 공용 규칙 등에 접근하는 데 사용됩니다.
        /// </summary>
        public TcgBattleDataMain BattleDataMain { get; }

        /// <summary>
        /// Ability를 실행하는 주체(캐스터)의 전투 데이터(사이드 상태)입니다.
        /// </summary>
        public TcgBattleDataSide CasterBattleDataSide { get; }

        /// <summary>
        /// Ability 적용 대상이 되는 필드 카드 데이터입니다.
        /// </summary>
        /// <remarks>
        /// 대상 조회 실패(상대 사이드 없음 등) 시 null일 수 있습니다.
        /// </remarks>
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
        /// <param name="battleDataMain">전투 전역 상태(메인 데이터)입니다.</param>
        /// <param name="casterSide">Ability를 실행하는 주체의 플레이어 사이드입니다.</param>
        /// <param name="casterZone">캐스터가 위치한 존(현재 구현에서는 사용되지 않을 수 있습니다).</param>
        /// <param name="casterIndex">캐스터 인덱스(현재 구현에서는 사용되지 않을 수 있습니다).</param>
        /// <param name="targetZone">대상이 위치한 존(필드의 어느 편인지 판정에 사용됩니다).</param>
        /// <param name="targetIndex">대상 카드의 인덱스입니다.</param>
        /// <param name="ability">실행할 Ability 정의입니다.</param>
        /// <remarks>
        /// 대상은 현재 구현 기준으로 다음 규칙으로 조회됩니다.
        /// - 캐스터가 Player이고 targetZone이 FieldPlayer이면 캐스터 사이드에서 targetIndex를 조회합니다.
        /// - 그 외에는 상대 사이드에서 targetIndex를 조회합니다.
        /// </remarks>
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
