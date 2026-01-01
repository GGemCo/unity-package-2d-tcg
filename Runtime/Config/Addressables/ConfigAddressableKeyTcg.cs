using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// Addressables Key 네이밍 규칙(개별 에셋 식별자).
    /// </summary>
    public static class ConfigAddressableKeyTcg
    {
        // 카드 (하위 그룹 형태로 구조화)
        private const string RootCard = ConfigDefine.NameSDK + "_" + ConfigPackageInfo.NamePackageTcg + "_Card";
        public static class Card
        {
            // 일러스트 이미지
            public const string ImageArt = RootCard + "_ArtWork";
            // 핸드 카드 테두리 이미지
            public const string ImageBorderHand = RootCard + "_BorderHand";
            // 필드 카드 테두리 이미지
            public const string ImageBorderField = RootCard + "_BorderField";
        }
    }
}