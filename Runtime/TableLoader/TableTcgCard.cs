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

        // (구) 단일 테이블에 능력을 문자열로 저장하던 방식과의 호환을 위해 남겨둡니다.
        // 신규 테이블 구조에서는 대부분 비어있으며, 향후 제거 대상입니다.
        public string keywordRaw;

        // 세부 카드 데이터 (타입별)
        public StruckTableTcgCardCreature struckTableTcgCardCreature;
        public StruckTableTcgCardSpell struckTableTcgCardSpell;
        public StruckTableTcgCardHero struckTableTcgCardHero;
        public StruckTableTcgCardEquipment struckTableTcgCardEquipment;
        public StruckTableTcgCardPermanent struckTableTcgCardPermanent;
        public StruckTableTcgCardEvent struckTableTcgCardEvent;

        public int GetAttack()
        {
            return type switch
            {
                CardConstants.Type.Hero => struckTableTcgCardHero?.attack ?? 0,
                CardConstants.Type.Creature => struckTableTcgCardCreature?.attack ?? 0,
                _ => 0
            };
        }

        public int GetHealth()
        {
            return type switch
            {
                CardConstants.Type.Hero => struckTableTcgCardHero?.health ?? 0,
                CardConstants.Type.Creature => struckTableTcgCardCreature?.health ?? 0,
                _ => 0
            };
        }
    }

    public class TableTcgCard : DefaultTable<StruckTableTcgCard>
    {
        public override string Key => ConfigAddressableTableTcg.TcgCard;

        private TableTcgCardCreature _tableTcgCardCreature;
        private TableTcgCardSpell _tableTcgCardSpell;
        private TableTcgCardHero _tableTcgCardHero;
        private TableTcgCardEquipment _tableTcgCardEquipment;
        private TableTcgCardPermanent _tableTcgCardPermanent;
        private TableTcgCardEvent _tableTcgCardEvent;
        private TcgAbilityDescriptionProvider _abilityDescriptionProvider;
        private TcgLifetimeLocalizationProvider _lifetimeLocalizationProvider;

        protected override void OnLoadedData(StruckTableTcgCard row)
        {
            if (row == null) return;

            // 로더 준비 여부 확인 및 테이블 캐싱
            if (!TryEnsureTablesCached()) return;

            switch (row.type)
            {
                case CardConstants.Type.Creature:
                    AttachCreature(row);
                    break;

                case CardConstants.Type.Spell:
                    AttachSpell(row);
                    ApplyAbilityDescriptionIfNeeded(row, TcgAbilityBuilder.BuildAbility(row.struckTableTcgCardSpell));
                    break;

                case CardConstants.Type.Equipment:
                    AttachEquipment(row);
                    ApplyAbilityDescriptionIfNeeded(row, TcgAbilityBuilder.BuildAbility(row.struckTableTcgCardEquipment));
                    break;

                case CardConstants.Type.Permanent:
                    AttachPermanent(row);
                    ApplyAbilityDescriptionIfNeeded(row, TcgAbilityBuilder.BuildAbility(row.struckTableTcgCardPermanent));
                    break;

                case CardConstants.Type.Event:
                    AttachEvent(row);
                    ApplyAbilityDescriptionIfNeeded(row, TcgAbilityBuilder.BuildAbility(row.struckTableTcgCardEvent));
                    break;

                case CardConstants.Type.Hero:
                    AttachHero(row);
                    break;
            }
        }

        private bool TryEnsureTablesCached()
        {
            if (!TableLoaderManagerTcg.Instance) return false;

            // 타입별 상세 테이블
            _tableTcgCardCreature ??= TableLoaderManagerTcg.Instance.TableTcgCardCreature;
            _tableTcgCardSpell ??= TableLoaderManagerTcg.Instance.TableTcgCardSpell;
            _tableTcgCardHero ??= TableLoaderManagerTcg.Instance.TableTcgCardHero;
            _tableTcgCardEquipment ??= TableLoaderManagerTcg.Instance.TableTcgCardEquipment;
            _tableTcgCardPermanent ??= TableLoaderManagerTcg.Instance.TableTcgCardPermanent;
            _tableTcgCardEvent ??= TableLoaderManagerTcg.Instance.TableTcgCardEvent;            
            _abilityDescriptionProvider ??= new TcgAbilityDescriptionProvider();
            _lifetimeLocalizationProvider ??= new TcgLifetimeLocalizationProvider();

            return true;
        }

        private void AttachCreature(StruckTableTcgCard row)
        {
            if (_tableTcgCardCreature == null) return;
            row.struckTableTcgCardCreature = _tableTcgCardCreature.GetDataByUid(row.uid);
        }

        private void AttachSpell(StruckTableTcgCard row)
        {
            if (_tableTcgCardSpell == null) return;
            row.struckTableTcgCardSpell = _tableTcgCardSpell.GetDataByUid(row.uid);
        }

        private void AttachHero(StruckTableTcgCard row)
        {
            if (_tableTcgCardHero == null) return;
            row.struckTableTcgCardHero = _tableTcgCardHero.GetDataByUid(row.uid);
        }

        private void AttachEquipment(StruckTableTcgCard row)
        {
            if (_tableTcgCardEquipment == null) return;
            row.struckTableTcgCardEquipment = _tableTcgCardEquipment.GetDataByUid(row.uid);
        }

        private void AttachPermanent(StruckTableTcgCard row)
        {
            if (_tableTcgCardPermanent == null) return;
            row.struckTableTcgCardPermanent = _tableTcgCardPermanent.GetDataByUid(row.uid);
        }

        private void AttachEvent(StruckTableTcgCard row)
        {
            if (_tableTcgCardEvent == null) return;
            row.struckTableTcgCardEvent = _tableTcgCardEvent.GetDataByUid(row.uid);
        }

        /// <summary>
        /// abilityUid가 유효하면 Ability 테이블에서 description을 가져와 카드 description을 갱신합니다.
        /// - 카드 테이블의 Description을 기본값으로 두고,
        /// - Ability.description이 비어있지 않으면 우선 적용합니다.
        /// </summary>
        private void ApplyAbilityDescriptionIfNeeded(StruckTableTcgCard row, in TcgAbilityDefinition ability)
        {
            if (row == null) return;
            var description = _abilityDescriptionProvider.GetDescription(ability);
            if (!string.IsNullOrEmpty(description))
            {
                row.description = description;
            }

            if (row.struckTableTcgCardPermanent == null) return; 
            var lifetime = _lifetimeLocalizationProvider.BuildLifetimeText(row.struckTableTcgCardPermanent);
            if (!string.IsNullOrEmpty(lifetime))
            {
                row.description = $"{row.description}\n({lifetime})";
            }
        }

        protected override StruckTableTcgCard BuildRow(Dictionary<string, string> data)
        {
            // 필수 컬럼은 GetValue로 강제하고, 선택 컬럼은 TryGetValue로 방어합니다.
            var uidStr = GetValue(data, "Uid");
            var name = GetValue(data, "Name");

#if UNITY_EDITOR
            if (AddressableLoaderSettingsTcg.Instance?.tcgSettings &&
                AddressableLoaderSettingsTcg.Instance.tcgSettings.showCardUid)
            {
                name = $"[{uidStr}] {name}";
            }
#endif

            return new StruckTableTcgCard
            {
                uid = MathHelper.ParseInt(uidStr),
                name = name,
                type = EnumHelper.ConvertEnum<CardConstants.Type>(GetValue(data, "Type")),
                grade = EnumHelper.ConvertEnum<CardConstants.Grade>(GetValue(data, "Grade")),
                cost = MathHelper.ParseInt(GetValue(data, "Cost")),
                maxCopiesPerDeck = MathHelper.ParseInt(GetValue(data, "MaxCopiesPerDeck")),
                imageFileName = GetValueOrEmpty(data, "ImageFileName"),
                description = GetValueOrEmpty(data, "Description"),
                keywordRaw = GetValueOrEmpty(data, "Keyword"),
            };
        }

        private static string GetValue(Dictionary<string, string> data, string key)
        {
            // 테이블 포맷 오류를 초기에 빠르게 드러내기 위해 KeyNotFound는 허용합니다.
            // (원하면 여기서 LogError 후 string.Empty 반환으로 변경 가능합니다.)
            return data[key];
        }

        private static string GetValueOrEmpty(Dictionary<string, string> data, string key)
        {
            return data.TryGetValue(key, out var v) ? v : string.Empty;
        }
    }
}
