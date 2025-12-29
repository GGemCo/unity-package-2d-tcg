namespace GGemCo2DTcg
{
    /// <summary>
    /// Ability 실행 시 필요한 모든 런타임 컨텍스트 정보를 담는 객체입니다.
    /// 
    /// - 실행 주체(Caster), 상대(Opponent), 전투 전역 정보(BattleDataMain)를 제공합니다.
    /// - 실제 능력 수치 및 설정은 <see cref="TcgAbilityDefinition"/>을 기준으로 합니다.
    /// - 타겟이 UI 또는 AI 로직에서 이미 결정된 경우,
    ///   <see cref="TargetBattleData"/>에 주입하여 실행 로직에서 재선택을 생략할 수 있습니다.
    /// </summary>
    public sealed class TcgAbilityContext
    {
        /// <summary>
        /// 전투 전체를 대표하는 메인 데이터입니다.
        /// 턴, 스택, 공용 규칙 등에 접근하는 데 사용됩니다.
        /// </summary>
        public TcgBattleDataMain BattleDataMain { get; }

        /// <summary>
        /// Ability를 시전하는 주체의 전투 데이터입니다.
        /// </summary>
        public TcgBattleDataSide Caster { get; }

        /// <summary>
        /// Ability 시전자의 상대편 전투 데이터입니다.
        /// </summary>
        public TcgBattleDataSide Opponent { get; }

        /// <summary>
        /// Ability를 발생시킨 원본 카드의 전투 데이터입니다.
        /// </summary>
        public TcgBattleDataCard SourceCard { get; }

        /// <summary>
        /// 실행할 Ability의 정의 데이터입니다.
        /// </summary>
        public TcgAbilityDefinition Ability { get; }

        /// <summary>
        /// Ability의 실제 적용 대상 전투 데이터입니다.
        /// 
        /// - 단일 타겟 Ability의 경우 UI/AI에서 미리 결정하여 설정할 수 있습니다.
        /// - null인 경우, Ability 실행 로직에서 타겟 선택을 수행할 수 있습니다.
        /// </summary>
        public TcgBattleDataFieldCard TargetBattleData { get; set; }

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
        /// <param name="battleDataMain">전투 전체를 대표하는 메인 데이터입니다.</param>
        /// <param name="caster">Ability를 시전하는 주체입니다.</param>
        /// <param name="opponent">시전자 기준 상대편입니다.</param>
        /// <param name="sourceCard">Ability를 발생시킨 원본 카드입니다.</param>
        /// <param name="ability">실행할 Ability 정의 데이터입니다.</param>
        public TcgAbilityContext(
            TcgBattleDataMain battleDataMain,
            TcgBattleDataSide caster,
            TcgBattleDataSide opponent,
            TcgBattleDataCard sourceCard,
            TcgAbilityDefinition ability)
        {
            BattleDataMain = battleDataMain;
            Caster = caster;
            Opponent = opponent;
            SourceCard = sourceCard;
            Ability = ability;
        }
    }
}
