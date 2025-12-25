using GGemCo2DTcg;

#if UNITY_EDITOR
namespace GGemCo2DTcgEditor
{
    /// <summary>TSV 파싱 결과 DTO</summary>
    public readonly struct TcgAbilityTsvRow
    {
        public int Uid { get; }
        public string Name { get; }
        public TcgAbilityConstants.TcgAbilityType AbilityType { get; }
        public TcgAbilityConstants.TcgAbilityTriggerType TcgAbilityTriggerType { get; }
        public TcgAbilityConstants.TcgAbilityTargetType TcgAbilityTargetType { get; }
        public int ParamA { get; }
        public int ParamB { get; }
        public int ParamC { get; }

        public TcgAbilityTsvRow(int uid, string name, TcgAbilityConstants.TcgAbilityType abilityType,
            TcgAbilityConstants.TcgAbilityTriggerType tcgAbilityTriggerType, TcgAbilityConstants.TcgAbilityTargetType tcgAbilityTargetType, int paramA,
            int paramB, int paramC)
        {
            Uid = uid;
            Name = name;
            AbilityType = abilityType;
            TcgAbilityTriggerType = tcgAbilityTriggerType;
            TcgAbilityTargetType = tcgAbilityTargetType;
            ParamA = paramA;
            ParamB = paramB;
            ParamC = paramC;
        }
    }
}
#endif