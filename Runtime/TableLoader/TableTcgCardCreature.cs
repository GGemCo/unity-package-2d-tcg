using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    public class StruckTableTcgCardCreature
    {
        public int uid;
        public int attack;
        public int health;
        public string keywords;
    }
    public class TableTcgCardCreature : DefaultTable<StruckTableTcgCardCreature>
    {
        public override string Key => ConfigAddressableTableTcg.TcgCardCreature;
        
        protected override StruckTableTcgCardCreature BuildRow(Dictionary<string, string> data)
        {
            return new StruckTableTcgCardCreature
            {
                uid = MathHelper.ParseInt(data["Uid"]),
                attack = MathHelper.ParseInt(data["Attack"]),
                health = MathHelper.ParseInt(data["Health"]),
                keywords = data["Keywords"],
            };
        }
    }
}