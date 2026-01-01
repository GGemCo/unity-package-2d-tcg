
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    public static class ConfigAddressableLabelTcg
    {
        // 카드 (하위 그룹 형태로 구조화)
        private const string RootCard = ConfigDefine.NameSDK + "_" + ConfigPackageInfo.NamePackageTcg + "_Card";
        public static class Card
        {
            // 일러스트 이미지
            public const string ImageArt = RootCard + "_ArtWork";
            // 테두리 이미지
            public const string ImageBorder = RootCard + "_Border";
        }
    }
}