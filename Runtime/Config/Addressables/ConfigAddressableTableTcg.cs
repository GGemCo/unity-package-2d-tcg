using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    public static class ConfigAddressableTableTcg
    {
        public const string TcgCard = "tcg_card";
        public const string TcgCardCreature = "tcg_card_creature";
        
        public static readonly AddressableAssetInfo TableTcgCard = ConfigAddressableTable.Make(TcgCard);
        public static readonly AddressableAssetInfo TableTcgCardCreature = ConfigAddressableTable.Make(TcgCardCreature);
        
        // 전체 목록 + 읽기 전용 뷰
        public static readonly List<AddressableAssetInfo> All = new()
        {
            TableTcgCard, TableTcgCardCreature
        };

    }
}