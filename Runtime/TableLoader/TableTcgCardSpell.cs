using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{						
    public class StruckTableTcgCardSpell
    {
        public int uid;
        public int effectId;
        public float castingTime;
        public float duration;
        public string areaShape;
        public int areaSize;
        public int additionalValue;
    }
    public class TableTcgCardSpell : DefaultTable<StruckTableTcgCardSpell>
    {
        public override string Key => ConfigAddressableTableTcg.TcgCardSpell;
        
        protected override StruckTableTcgCardSpell BuildRow(Dictionary<string, string> data)
        {
            return new StruckTableTcgCardSpell
            {
                uid = MathHelper.ParseInt(data["Uid"]),
                effectId = MathHelper.ParseInt(data["EffectId"]),
                castingTime = MathHelper.ParseFloat(data["CastingTime"]),
                duration = MathHelper.ParseFloat(data["Duration"]),
                areaShape = data["AreaShape"],
                areaSize = MathHelper.ParseInt(data["AreaSize"]),
                additionalValue = MathHelper.ParseInt(data["AdditionalValue"]),
            };
        }
    }
}