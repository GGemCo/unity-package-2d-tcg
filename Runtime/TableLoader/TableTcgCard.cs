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
        
        public string keywordRaw;          // CSV 문자열 그대로 (예: "Rush|Taunt")
        public string summonEffectsRaw;     // "DealDamageToTargetUnit:3:EnemyCreature;..."
        public string spellEffectsRaw;      // 스펠 사용 시
        public string deathEffectsRaw;      // 사망 시
        
        // 세부 카드 데이터 (Creature/Spell 전용 정보)
        public StruckTableTcgCardCreature struckTableTcgCardCreature;
        public StruckTableTcgCardSpell struckTableTcgCardSpell;
    }
    public class TableTcgCard : DefaultTable<StruckTableTcgCard>
    {
        public override string Key => ConfigAddressableTableTcg.TcgCard;
        
        private TableTcgCardCreature _tableTcgCardCreature;
        private TableTcgCardSpell _tableTcgCardSpell;
        
        protected override void OnLoadedData(StruckTableTcgCard row)
        {
            if (row == null)
            {
                return;
            }
            switch (row.type)
            {
                case CardConstants.Type.Creature:
                    AttachCreatureData(row);
                    break;
                case CardConstants.Type.Spell:
                    AttachSpellData(row);
                    break;
            }
        }
        private void AttachCreatureData(StruckTableTcgCard row)
        {
            _tableTcgCardCreature ??= TableLoaderManagerTcg.Instance.TableTcgCardCreature;
            if (_tableTcgCardCreature == null)
            {
                return;
            }

            row.struckTableTcgCardCreature = _tableTcgCardCreature.GetDataByUid(row.uid);
        }

        private void AttachSpellData(StruckTableTcgCard row)
        {
            _tableTcgCardSpell ??= TableLoaderManagerTcg.Instance.TableTcgCardSpell;
            if (_tableTcgCardSpell == null)
            {
                return;
            }

            row.struckTableTcgCardSpell = _tableTcgCardSpell.GetDataByUid(row.uid);
        }
        protected override StruckTableTcgCard BuildRow(Dictionary<string, string> data)
        {
            return new StruckTableTcgCard
            {
                uid = MathHelper.ParseInt(data["Uid"]),
                name = data["Name"],
                type = EnumHelper.ConvertEnum<CardConstants.Type>(data["Type"]),
                grade = EnumHelper.ConvertEnum<CardConstants.Grade>(data["Grade"]),
                cost = MathHelper.ParseInt(data["Cost"]),
                maxCopiesPerDeck = MathHelper.ParseInt(data["MaxCopiesPerDeck"]),
                imageFileName = data["ImageFileName"],
                description = data["Description"],
                keywordRaw = data["Keyword"],
                summonEffectsRaw = data["SummonEffects"],
                spellEffectsRaw = data["SpellEffects"],
                deathEffectsRaw = data["DeathEffects"]
            };
        }
    }
}