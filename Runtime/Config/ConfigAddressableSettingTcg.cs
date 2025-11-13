using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    public static class ConfigAddressableSettingTcg
    {
        public static readonly AddressableAssetInfo TcgSettings = ConfigAddressableSetting.Make(nameof(TcgSettings));
        
        /// <summary>
        /// 로딩 씬에서 로드해야 하는 리스트
        /// </summary>
        public static readonly List<AddressableAssetInfo> NeedLoadInLoadingScene = new()
        {
            TcgSettings,
        };
    }
}