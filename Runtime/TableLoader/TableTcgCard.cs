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
        public StruckTableTcgCardHero struckTableTcgCardHero;
    }
    public class TableTcgCard : DefaultTable<StruckTableTcgCard>
    {
        public override string Key => ConfigAddressableTableTcg.TcgCard;
        
        private TableTcgCardCreature _tableTcgCardCreature;
        private TableTcgCardSpell _tableTcgCardSpell;
        private TableTcgCardHero _tableTcgCardHero;
        
        protected override void OnLoadedData(StruckTableTcgCard row)
        {
            if (row == null)
            {
                return;
            }
            switch (row.type)
            {
                case CardConstants.Type.Creature:
                    AttachDataCreature(row);
                    break;
                case CardConstants.Type.Spell:
                    AttachDataSpell(row);
                    break;
                case CardConstants.Type.Hero:
                    AttachDataHero(row);
                    break;
            }
        }
        private void AttachDataCreature(StruckTableTcgCard row)
        {
            if (!TableLoaderManagerTcg.Instance) return;
            _tableTcgCardCreature ??= TableLoaderManagerTcg.Instance.TableTcgCardCreature;
            if (_tableTcgCardCreature == null)
            {
                return;
            }

            row.struckTableTcgCardCreature = _tableTcgCardCreature.GetDataByUid(row.uid);
        }

        private void AttachDataSpell(StruckTableTcgCard row)
        {
            if (!TableLoaderManagerTcg.Instance) return;
            _tableTcgCardSpell ??= TableLoaderManagerTcg.Instance.TableTcgCardSpell;
            if (_tableTcgCardSpell == null)
            {
                return;
            }

            row.struckTableTcgCardSpell = _tableTcgCardSpell.GetDataByUid(row.uid);
        }

        private void AttachDataHero(StruckTableTcgCard row)
        {
            if (!TableLoaderManagerTcg.Instance) return;
            _tableTcgCardHero ??= TableLoaderManagerTcg.Instance.TableTcgCardHero;
            if (_tableTcgCardHero == null)
            {
                return;
            }

            row.struckTableTcgCardHero = _tableTcgCardHero.GetDataByUid(row.uid);
        }
        protected override StruckTableTcgCard BuildRow(Dictionary<string, string> data)
        {
            var name = data["Name"];
# if UNITY_EDITOR
            if (AddressableLoaderSettingsTcg.Instance?.tcgSettings && AddressableLoaderSettingsTcg.Instance.tcgSettings.showCardUid)
                name = $"[{data["Uid"]}] {name}";
# endif
            return new StruckTableTcgCard
            {
                uid = MathHelper.ParseInt(data["Uid"]),
                name = name,
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