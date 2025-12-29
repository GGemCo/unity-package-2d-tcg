using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    public sealed class StruckTableTcgCardSpell : ITcgCardAbilityRow
    {
        public int uid { get; set; }
        public TcgAbilityConstants.TcgAbilityType abilityType { get; set; }
        public TcgAbilityConstants.TcgAbilityTriggerType tcgAbilityTriggerType { get; set; }
        public TcgAbilityConstants.TcgAbilityTargetType tcgAbilityTargetType { get; set; }
        public int paramA { get; set; }
        public int paramB { get; set; }
        public int paramC { get; set; }
    }

    public sealed class TableTcgCardSpell : DefaultTable<StruckTableTcgCardSpell>
    {
        public override string Key => ConfigAddressableTableTcg.TcgCardSpell;

        protected override StruckTableTcgCardSpell BuildRow(Dictionary<string, string> data)
        {
            return new StruckTableTcgCardSpell
            {
                uid = MathHelper.ParseInt(data["Uid"]),
                abilityType = EnumHelper.ConvertEnum<TcgAbilityConstants.TcgAbilityType>(data["AbilityType"]),
                tcgAbilityTriggerType = EnumHelper.ConvertEnum<TcgAbilityConstants.TcgAbilityTriggerType>(data["TriggerType"]),
                tcgAbilityTargetType = EnumHelper.ConvertEnum<TcgAbilityConstants.TcgAbilityTargetType>(data["TargetType"]),
                paramA = MathHelper.ParseInt(data["ParamA"]),
                paramB = MathHelper.ParseInt(data["ParamB"]),
                paramC = MathHelper.ParseInt(data["ParamC"]),
            };
        }
    }
}
