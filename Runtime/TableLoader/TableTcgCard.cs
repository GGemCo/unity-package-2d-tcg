using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// TCG 카드의 공통(기본) 테이블 행(row) 데이터입니다.
    /// 타입별 상세 데이터(크리처/스펠/영웅 등)는 로딩 후 해당 필드에 연결(Attach)됩니다.
    /// </summary>
    public class StruckTableTcgCard
    {
        /// <summary>카드의 고유 식별자(Uid)입니다.</summary>
        public int uid;

        /// <summary>카드 표시 이름입니다.</summary>
        public string name;

        /// <summary>카드 타입(예: Hero, Creature, Spell 등)입니다.</summary>
        public CardConstants.Type type;

        /// <summary>카드 등급(희귀도/티어 등)입니다.</summary>
        public CardConstants.Grade grade;

        /// <summary>카드 비용(코스트)입니다.</summary>
        public int cost;

        /// <summary>덱에 포함 가능한 최대 장 수입니다.</summary>
        public int maxCopiesPerDeck;

        /// <summary>카드 이미지 파일명(리소스 키)입니다.</summary>
        public string imageFileName;

        /// <summary>
        /// 카드 설명 텍스트입니다.
        /// 로딩 과정에서 능력(Ability) 기반 설명으로 덮어써질 수 있으며, 영구(Permanent) 타입은 수명(lifetime) 텍스트가 추가될 수 있습니다.
        /// </summary>
        public string description;

        /// <summary>
        /// (구) 단일 테이블에 키워드/능력을 문자열로 저장하던 방식과의 호환을 위해 남겨둔 필드입니다.
        /// 신규 테이블 구조에서는 대부분 비어있으며, 향후 제거 대상입니다.
        /// </summary>
        public string keywordRaw;

        // 세부 카드 데이터 (타입별)

        /// <summary>Creature 타입 카드의 상세 테이블 행 데이터입니다.</summary>
        public StruckTableTcgCardCreature struckTableTcgCardCreature;

        /// <summary>Spell 타입 카드의 상세 테이블 행 데이터입니다.</summary>
        public StruckTableTcgCardSpell struckTableTcgCardSpell;

        /// <summary>Hero 타입 카드의 상세 테이블 행 데이터입니다.</summary>
        public StruckTableTcgCardHero struckTableTcgCardHero;

        /// <summary>Equipment 타입 카드의 상세 테이블 행 데이터입니다.</summary>
        public StruckTableTcgCardEquipment struckTableTcgCardEquipment;

        /// <summary>Permanent 타입 카드의 상세 테이블 행 데이터입니다.</summary>
        public StruckTableTcgCardPermanent struckTableTcgCardPermanent;

        /// <summary>Event 타입 카드의 상세 테이블 행 데이터입니다.</summary>
        public StruckTableTcgCardEvent struckTableTcgCardEvent;

        /// <summary>
        /// 카드 타입에 따라 공격력(Attack)을 반환합니다.
        /// Hero/Creature 타입만 공격력을 가지며, 그 외 타입은 0을 반환합니다.
        /// </summary>
        /// <returns>카드의 공격력(없으면 0)입니다.</returns>
        public int GetAttack()
        {
            return type switch
            {
                CardConstants.Type.Hero => struckTableTcgCardHero?.attack ?? 0,
                CardConstants.Type.Creature => struckTableTcgCardCreature?.attack ?? 0,
                _ => 0
            };
        }

        /// <summary>
        /// 카드 타입에 따라 체력(Health)을 반환합니다.
        /// Hero/Creature 타입만 체력을 가지며, 그 외 타입은 0을 반환합니다.
        /// </summary>
        /// <returns>카드의 체력(없으면 0)입니다.</returns>
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

    /// <summary>
    /// TCG 카드의 기본 테이블을 로드하고, 타입별 상세 테이블(크리처/스펠/장비 등)과 결합하여 완성된 카드 데이터를 제공합니다.
    /// </summary>
    public class TableTcgCard : DefaultTable<StruckTableTcgCard>
    {
        /// <summary>
        /// Addressables(또는 테이블 로더)에서 이 테이블을 식별하기 위한 키입니다.
        /// </summary>
        public override string Key => ConfigAddressableTableTcg.TcgCard;

        // 타입별 상세 테이블 캐시
        private TableTcgCardCreature _tableTcgCardCreature;
        private TableTcgCardSpell _tableTcgCardSpell;
        private TableTcgCardHero _tableTcgCardHero;
        private TableTcgCardEquipment _tableTcgCardEquipment;
        private TableTcgCardPermanent _tableTcgCardPermanent;
        private TableTcgCardEvent _tableTcgCardEvent;

        // 부가 텍스트/로컬라이징 제공자
        private TcgAbilityDescriptionProvider _abilityDescriptionProvider;
        private TcgLifetimeLocalizationProvider _lifetimeLocalizationProvider;

        /// <summary>
        /// 행(row) 하나가 로드된 직후 호출되며, 타입에 따라 상세 테이블 행을 연결하고
        /// 필요 시 능력(Ability) 기반 설명/수명(lifetime) 정보를 카드 설명에 반영합니다.
        /// </summary>
        /// <param name="row">로딩된 카드 기본 행 데이터입니다.</param>
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

        /// <summary>
        /// 테이블 로더(<see cref="TableLoaderManagerTcg"/>)가 준비되었는지 확인하고,
        /// 타입별 상세 테이블 및 보조 Provider를 로컬 캐시에 바인딩합니다.
        /// </summary>
        /// <returns>캐시 준비에 성공하면 true, 로더가 없으면 false입니다.</returns>
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

            // 설명/로컬라이징 제공자 (경량 객체라면 필요 시 교체 가능)
            _abilityDescriptionProvider ??= new TcgAbilityDescriptionProvider();
            _lifetimeLocalizationProvider ??= new TcgLifetimeLocalizationProvider();

            return true;
        }

        /// <summary>
        /// Creature 타입 카드에 대해 상세 행 데이터를 연결합니다.
        /// </summary>
        /// <param name="row">대상 카드 기본 행 데이터입니다.</param>
        private void AttachCreature(StruckTableTcgCard row)
        {
            if (_tableTcgCardCreature == null) return;
            row.struckTableTcgCardCreature = _tableTcgCardCreature.GetDataByUid(row.uid);
        }

        /// <summary>
        /// Spell 타입 카드에 대해 상세 행 데이터를 연결합니다.
        /// </summary>
        /// <param name="row">대상 카드 기본 행 데이터입니다.</param>
        private void AttachSpell(StruckTableTcgCard row)
        {
            if (_tableTcgCardSpell == null) return;
            row.struckTableTcgCardSpell = _tableTcgCardSpell.GetDataByUid(row.uid);
        }

        /// <summary>
        /// Hero 타입 카드에 대해 상세 행 데이터를 연결합니다.
        /// </summary>
        /// <param name="row">대상 카드 기본 행 데이터입니다.</param>
        private void AttachHero(StruckTableTcgCard row)
        {
            if (_tableTcgCardHero == null) return;
            row.struckTableTcgCardHero = _tableTcgCardHero.GetDataByUid(row.uid);
        }

        /// <summary>
        /// Equipment 타입 카드에 대해 상세 행 데이터를 연결합니다.
        /// </summary>
        /// <param name="row">대상 카드 기본 행 데이터입니다.</param>
        private void AttachEquipment(StruckTableTcgCard row)
        {
            if (_tableTcgCardEquipment == null) return;
            row.struckTableTcgCardEquipment = _tableTcgCardEquipment.GetDataByUid(row.uid);
        }

        /// <summary>
        /// Permanent 타입 카드에 대해 상세 행 데이터를 연결합니다.
        /// </summary>
        /// <param name="row">대상 카드 기본 행 데이터입니다.</param>
        private void AttachPermanent(StruckTableTcgCard row)
        {
            if (_tableTcgCardPermanent == null) return;
            row.struckTableTcgCardPermanent = _tableTcgCardPermanent.GetDataByUid(row.uid);
        }

        /// <summary>
        /// Event 타입 카드에 대해 상세 행 데이터를 연결합니다.
        /// </summary>
        /// <param name="row">대상 카드 기본 행 데이터입니다.</param>
        private void AttachEvent(StruckTableTcgCard row)
        {
            if (_tableTcgCardEvent == null) return;
            row.struckTableTcgCardEvent = _tableTcgCardEvent.GetDataByUid(row.uid);
        }

        /// <summary>
        /// Ability 정의를 기반으로 카드 설명(description)을 갱신합니다.
        /// 카드 테이블의 기본 설명을 유지하되, Ability 기반 설명이 존재하면 이를 우선 적용합니다.
        /// 또한 Permanent 상세 데이터가 존재하는 경우 수명(lifetime) 텍스트를 조합하여 추가합니다.
        /// </summary>
        /// <param name="row">대상 카드 기본 행 데이터입니다.</param>
        /// <param name="ability">카드 상세 데이터로부터 구성된 Ability 정의입니다.</param>
        private void ApplyAbilityDescriptionIfNeeded(StruckTableTcgCard row, in TcgAbilityDefinition ability)
        {
            if (row == null) return;

            var description = _abilityDescriptionProvider.GetDescription(ability);
            if (!string.IsNullOrEmpty(description))
            {
                row.description = description;
            }

            // Permanent 상세가 있을 때만 lifetime 텍스트를 조합합니다.
            if (row.struckTableTcgCardPermanent == null) return;

            var lifetime = _lifetimeLocalizationProvider.BuildLifetimeText(row.struckTableTcgCardPermanent);
            if (!string.IsNullOrEmpty(lifetime))
            {
                row.description = $"{row.description}\n({lifetime})";
            }
        }

        /// <summary>
        /// 테이블 한 행의 원시(Dictionary) 데이터를 <see cref="StruckTableTcgCard"/>로 변환합니다.
        /// 필수 컬럼은 <see cref="GetValue"/>로 강제하며, 선택 컬럼은 <see cref="GetValueOrEmpty"/>로 안전하게 처리합니다.
        /// </summary>
        /// <param name="data">컬럼명-문자열 값 형태의 원시 행 데이터입니다.</param>
        /// <returns>변환된 카드 기본 행 데이터입니다.</returns>
        /// <exception cref="KeyNotFoundException">
        /// 필수 컬럼 키가 누락된 경우 <see cref="GetValue"/> 내부에서 발생할 수 있습니다.
        /// </exception>
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

        /// <summary>
        /// 원시 행 데이터에서 필수 컬럼 값을 가져옵니다.
        /// 테이블 포맷 오류를 빠르게 드러내기 위해 키 누락 시 예외를 허용합니다.
        /// </summary>
        /// <param name="data">원시 행 데이터입니다.</param>
        /// <param name="key">필수 컬럼 키입니다.</param>
        /// <returns>해당 키의 문자열 값입니다.</returns>
        /// <exception cref="KeyNotFoundException">키가 존재하지 않으면 발생합니다.</exception>
        private static string GetValue(Dictionary<string, string> data, string key)
        {
            // (원하면 여기서 LogError 후 string.Empty 반환으로 변경 가능합니다.)
            return data[key];
        }

        /// <summary>
        /// 원시 행 데이터에서 선택 컬럼 값을 가져오며, 없으면 빈 문자열을 반환합니다.
        /// </summary>
        /// <param name="data">원시 행 데이터입니다.</param>
        /// <param name="key">선택 컬럼 키입니다.</param>
        /// <returns>값이 존재하면 문자열 값, 없으면 <see cref="string.Empty"/>입니다.</returns>
        private static string GetValueOrEmpty(Dictionary<string, string> data, string key)
        {
            return data.TryGetValue(key, out var v) ? v : string.Empty;
        }
    }
}
