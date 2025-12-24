using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    public sealed class StruckTableTcgCardEquipment
    {
        public int uid;
        public int abilityUid;
        public TcgAbilityConstants.TriggerType triggerType;

        public string slot;
        public int attackBonus;
        public int healthBonus;
        public int durability;
    }

    public sealed class TableTcgCardEquipment : DefaultTable<StruckTableTcgCardEquipment>
    {
        public override string Key => ConfigAddressableTableTcg.TcgCardEquipment;

        protected override StruckTableTcgCardEquipment BuildRow(Dictionary<string, string> data)
        {
            return new StruckTableTcgCardEquipment
            {
                uid = MathHelper.ParseInt(data["Uid"]),
                abilityUid = MathHelper.ParseInt(data["AbilityUid"]),
                triggerType = EnumHelper.ConvertEnum<TcgAbilityConstants.TriggerType>(data["TriggerType"]),
                slot = data.TryGetValue("Slot", out var v) ? v : string.Empty,
                attackBonus = data.TryGetValue("AttackBonus", out var a) ? MathHelper.ParseInt(a) : 0,
                healthBonus = data.TryGetValue("HealthBonus", out var h) ? MathHelper.ParseInt(h) : 0,
                durability = data.TryGetValue("Durability", out var d) ? MathHelper.ParseInt(d) : 0,
            };
        }
    }
}
