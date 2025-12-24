namespace GGemCo2DTcg
{
    /// <summary>
    /// Ability 실행 시 필요한 컨텍스트 정보.
    /// - 실행 대상/파라미터는 <see cref="StruckTableTcgAbility"/> 정의를 기준으로 합니다.
    /// - 타겟이 UI/AI에 의해 이미 결정된 경우 <see cref="TargetBattleData"/>에 주입합니다.
    /// </summary>
    public sealed class TcgAbilityContext
    {
        public TcgBattleDataMain BattleDataMain { get; }
        public TcgBattleDataSide Caster { get; }
        public TcgBattleDataSide Opponent { get; }
        public TcgBattleDataCard SourceCard { get; }

        public StruckTableTcgAbility Ability { get; }

        public TcgBattleDataFieldCard TargetBattleData { get; set; }

        public int ParamA => Ability.paramA;
        public int ParamB => Ability.paramB;
        public int ParamC => Ability.paramC;

        public TcgAbilityContext(
            TcgBattleDataMain battleDataMain,
            TcgBattleDataSide caster,
            TcgBattleDataSide opponent,
            TcgBattleDataCard sourceCard,
            StruckTableTcgAbility ability)
        {
            BattleDataMain = battleDataMain;
            Caster = caster;
            Opponent = opponent;
            SourceCard = sourceCard;
            Ability = ability;
        }
    }
}
