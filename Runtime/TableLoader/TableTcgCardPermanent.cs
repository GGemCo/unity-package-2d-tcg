using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    public sealed class StruckTableTcgCardPermanent
    {
        public int uid;
        public int abilityUid;
        public TcgAbilityConstants.TcgAbilityTriggerType tcgAbilityTriggerType;

        public int intervalTurn;
        public int maxStacks;
    }

    public sealed class TableTcgCardPermanent : DefaultTable<StruckTableTcgCardPermanent>
    {
        public override string Key => ConfigAddressableTableTcg.TcgCardPermanent;

        protected override StruckTableTcgCardPermanent BuildRow(Dictionary<string, string> data)
        {
            return new StruckTableTcgCardPermanent
            {
                uid = MathHelper.ParseInt(data["Uid"]),
                abilityUid = MathHelper.ParseInt(data["AbilityUid"]),
                tcgAbilityTriggerType = EnumHelper.ConvertEnum<TcgAbilityConstants.TcgAbilityTriggerType>(data["TriggerType"]),
                intervalTurn = data.TryGetValue("IntervalTurn", out var it) ? MathHelper.ParseInt(it) : 1,
                maxStacks = data.TryGetValue("MaxStacks", out var ms) ? MathHelper.ParseInt(ms) : 1,
            };
        }
    }
}
