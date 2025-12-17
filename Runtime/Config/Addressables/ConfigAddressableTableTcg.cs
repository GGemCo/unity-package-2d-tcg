using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    public static class ConfigAddressableTableTcg
    {
        public const string TcgCard = "tcg_card";
        public const string TcgCardCreature = "tcg_card_creature";
        public const string TcgCardSpell = "tcg_card_spell";
        public const string TcgCardHero = "tcg_card_hero";
        
        public static readonly AddressableAssetInfo TableTcgCard = ConfigAddressableTable.Make(TcgCard);
        public static readonly AddressableAssetInfo TableTcgCardCreature = ConfigAddressableTable.Make(TcgCardCreature);
        public static readonly AddressableAssetInfo TableTcgCardSpell = ConfigAddressableTable.Make(TcgCardSpell);
        public static readonly AddressableAssetInfo TableTcgCardHero = ConfigAddressableTable.Make(TcgCardHero);
        
        // 전체 목록 + 읽기 전용 뷰
        public static readonly List<AddressableAssetInfo> All = new()
        {
            // 순서 중요.
            // Hero, Creature, Spell 테이블 내용을 TableTcgCard 에서 사용함 마지막에 있어야 함.
            TableTcgCardHero, TableTcgCardCreature, TableTcgCardSpell, TableTcgCard
        };

    }
}