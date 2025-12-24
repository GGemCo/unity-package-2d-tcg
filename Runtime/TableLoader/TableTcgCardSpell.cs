using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    public sealed class StruckTableTcgCardSpell
    {
        // DefaultTable 규약(uid) 유지
        public int uid;
        public int abilityUid;
        public TcgAbilityConstants.TriggerType triggerType;

        public float castingTime;
        public float duration;
        public string areaShape;
        public int areaSize;
    }

    public sealed class TableTcgCardSpell : DefaultTable<StruckTableTcgCardSpell>
    {
        public override string Key => ConfigAddressableTableTcg.TcgCardSpell;

        protected override StruckTableTcgCardSpell BuildRow(Dictionary<string, string> data)
        {
            return new StruckTableTcgCardSpell
            {
                uid = MathHelper.ParseInt(data["Uid"]),
                abilityUid = MathHelper.ParseInt(data["AbilityUid"]),
                triggerType = EnumHelper.ConvertEnum<TcgAbilityConstants.TriggerType>(data["TriggerType"]),

                castingTime = data.TryGetValue("CastingTime", out var ct) ? MathHelper.ParseFloat(ct) : 0f,
                duration = data.TryGetValue("Duration", out var du) ? MathHelper.ParseFloat(du) : 0f,
                areaShape = data.TryGetValue("AreaShape", out var shape) ? shape : string.Empty,
                areaSize = data.TryGetValue("AreaSize", out var size) ? MathHelper.ParseInt(size) : 0,
            };
        }
    }
}
