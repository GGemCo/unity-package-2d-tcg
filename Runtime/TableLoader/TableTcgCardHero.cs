using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    public class StruckTableTcgCardHero
    {
        public int uid;
        public int attack;
        public int health;
        public string keywords;
    }
    public class TableTcgCardHero : DefaultTable<StruckTableTcgCardHero>
    {
        public override string Key => ConfigAddressableTableTcg.TcgCardHero;
        
        protected override StruckTableTcgCardHero BuildRow(Dictionary<string, string> data)
        {
            return new StruckTableTcgCardHero
            {
                uid = MathHelper.ParseInt(data["Uid"]),
                attack = MathHelper.ParseInt(data["Attack"]),
                health = MathHelper.ParseInt(data["Health"]),
                keywords = data["Keywords"],
            };
        }
    }
}