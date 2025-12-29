using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    public static class ConfigAddressableTableTcg
    {
        public const string TcgCard = "tcg_card";
        public const string TcgCardCreature = "tcg_card_creature";
        public const string TcgCardSpell = "tcg_card_spell";
        public const string TcgCardEquipment = "tcg_card_equipment";
        public const string TcgCardPermanent = "tcg_card_permanent";
        public const string TcgCardEvent = "tcg_card_event";
        public const string TcgCardHero = "tcg_card_hero";
        
        public static readonly AddressableAssetInfo TableTcgCard = ConfigAddressableTable.Make(TcgCard);
        public static readonly AddressableAssetInfo TableTcgCardCreature = ConfigAddressableTable.Make(TcgCardCreature);
        public static readonly AddressableAssetInfo TableTcgCardSpell = ConfigAddressableTable.Make(TcgCardSpell);
        public static readonly AddressableAssetInfo TableTcgCardEquipment = ConfigAddressableTable.Make(TcgCardEquipment);
        public static readonly AddressableAssetInfo TableTcgCardPermanent = ConfigAddressableTable.Make(TcgCardPermanent);
        public static readonly AddressableAssetInfo TableTcgCardEvent = ConfigAddressableTable.Make(TcgCardEvent);
        public static readonly AddressableAssetInfo TableTcgCardHero = ConfigAddressableTable.Make(TcgCardHero);        
        
        // 전체 목록 + 읽기 전용 뷰
        public static readonly List<AddressableAssetInfo> All = new()
        {
            // 순서 중요.
            // Base(TcgCard)에서 타입별 상세를 참조할 수 있으므로, 상세 테이블을 먼저 로드합니다.
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