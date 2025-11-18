using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{

    public class StruckTableTcgCard
    {
        public int uid;
        public string name;
        public CardConstants.Type type;
        public CardConstants.Grade grade;
        public int cost;
        public int maxCopiesPerDeck;
        public string imageFileName;
        public string description;
    }
    public class TableTcgCard : DefaultTable<StruckTableTcgCard>
    {
        public override string Key => ConfigAddressableTableTcg.TcgCard;
        
        protected override StruckTableTcgCard BuildRow(Dictionary<string, string> data)
        {
            return new StruckTableTcgCard
            {
                uid = int.Parse(data["Uid"]),
                name = data["Name"],
                type = EnumHelper.ConvertEnum<CardConstants.Type>(data["Type"]),
                grade = EnumHelper.ConvertEnum<CardConstants.Grade>(data["Grade"]),
                cost = int.Parse(data["Cost"]),
                maxCopiesPerDeck = int.Parse(data["MaxCopiesPerDeck"]),
                imageFileName = data["ImageFileName"],
                description = data["Description"],
            };
        }
    }
}