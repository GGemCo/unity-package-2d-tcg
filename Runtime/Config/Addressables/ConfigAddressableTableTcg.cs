using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// TCG에서 사용되는 Addressables 테이블(Table) 에셋 정의 클래스입니다.
    /// 카드 데이터 테이블의 Key 문자열과 AddressableAssetInfo를 중앙에서 관리합니다.
    /// </summary>
    public static class ConfigAddressableTableTcg
    {
        /// <summary>
        /// 카드 공통(Base) 테이블 Key입니다.
        /// 모든 카드 타입에서 공통으로 참조되는 기본 정보가 포함됩니다.
        /// </summary>
        public const string TcgCard = "tcg_card";

        /// <summary>
        /// 크리처 카드 전용 테이블 Key입니다.
        /// 크리처 카드의 스탯, 전투 관련 속성 등을 포함합니다.
        /// </summary>
        public const string TcgCardCreature = "tcg_card_creature";

        /// <summary>
        /// 스펠 카드 전용 테이블 Key입니다.
        /// 스펠 효과, 발동 조건, 대상 정보 등을 포함합니다.
        /// </summary>
        public const string TcgCardSpell = "tcg_card_spell";

        /// <summary>
        /// 장비 카드 전용 테이블 Key입니다.
        /// 장착 효과, 능력치 보정 정보 등을 포함합니다.
        /// </summary>
        public const string TcgCardEquipment = "tcg_card_equipment";

        /// <summary>
        /// 영속(Permanent) 카드 전용 테이블 Key입니다.
        /// 필드, 건물 등 지속 효과 카드 정보를 포함합니다.
        /// </summary>
        public const string TcgCardPermanent = "tcg_card_permanent";

        /// <summary>
        /// 이벤트(Event) 카드 전용 테이블 Key입니다.
        /// 트리거, 일회성 특수 효과 카드 정보를 포함합니다.
        /// </summary>
        public const string TcgCardEvent = "tcg_card_event";

        /// <summary>
        /// 영웅(Hero) 카드 전용 테이블 Key입니다.
        /// 플레이어를 대표하는 영웅 카드의 고유 정보가 포함됩니다.
        /// </summary>
        public const string TcgCardHero = "tcg_card_hero";

        /// <summary>
        /// 카드 공통(Base) 테이블의 Addressables 에셋 정보입니다.
        /// </summary>
        public static readonly AddressableAssetInfo TableTcgCard =
            ConfigAddressableTable.Make(TcgCard);

        /// <summary>
        /// 크리처 카드 테이블의 Addressables 에셋 정보입니다.
        /// </summary>
        public static readonly AddressableAssetInfo TableTcgCardCreature =
            ConfigAddressableTable.Make(TcgCardCreature);

        /// <summary>
        /// 스펠 카드 테이블의 Addressables 에셋 정보입니다.
        /// </summary>
        public static readonly AddressableAssetInfo TableTcgCardSpell =
            ConfigAddressableTable.Make(TcgCardSpell);

        /// <summary>
        /// 장비 카드 테이블의 Addressables 에셋 정보입니다.
        /// </summary>
        public static readonly AddressableAssetInfo TableTcgCardEquipment =
            ConfigAddressableTable.Make(TcgCardEquipment);

        /// <summary>
        /// 영속 카드 테이블의 Addressables 에셋 정보입니다.
        /// </summary>
        public static readonly AddressableAssetInfo TableTcgCardPermanent =
            ConfigAddressableTable.Make(TcgCardPermanent);

        /// <summary>
        /// 이벤트 카드 테이블의 Addressables 에셋 정보입니다.
        /// </summary>
        public static readonly AddressableAssetInfo TableTcgCardEvent =
            ConfigAddressableTable.Make(TcgCardEvent);

        /// <summary>
        /// 영웅 카드 테이블의 Addressables 에셋 정보입니다.
        /// </summary>
        public static readonly AddressableAssetInfo TableTcgCardHero =
            ConfigAddressableTable.Make(TcgCardHero);

        /// <summary>
        /// TCG 카드 테이블 전체 목록입니다.
        /// 로딩 순서가 중요한 테이블들을 의존성에 맞게 정렬합니다.
        /// </summary>
        public static readonly List<AddressableAssetInfo> All = new()
        {
            // 순서 중요:
            // 공통 카드 테이블(TcgCard)은 타입별 상세 테이블을 참조할 수 있으므로
            // 상세 테이블을 먼저 로드한 뒤 Base 테이블을 마지막에 로드합니다.
            TableTcgCardHero,
            TableTcgCardCreature,
            TableTcgCardSpell,
            TableTcgCardEquipment,
            TableTcgCardPermanent,
            TableTcgCardEvent,
            TableTcgCard
        };
    }
}
