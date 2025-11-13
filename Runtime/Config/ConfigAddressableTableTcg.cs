using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    public static class ConfigAddressableTableTcg
    {
        public const string TcgCard = "tcg_card";
        
        public static readonly AddressableAssetInfo TableTcgCard = ConfigAddressableTable.Make(TcgCard);
        
        // 전체 목록 + 읽기 전용 뷰
        public static readonly List<AddressableAssetInfo> All = new()
        {
            TableTcgCard
        };
    }
}