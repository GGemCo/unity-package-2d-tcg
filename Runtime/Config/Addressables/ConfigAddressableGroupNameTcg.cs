using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// Addressables Group 네이밍 규칙(빌드/패키징 단위).
    /// </summary>
    public static class ConfigAddressableGroupNameTcg
    {
        // 카드 (하위 그룹 형태로 구조화)
        private const string Card = ConfigDefine.NameSDK + "_" + ConfigPackageInfo.NamePackageTcg + "_Card";
        public static class CardGroup
        {
            // 일러스트 이미지
            public const string ImageArtwork  = Card + "_ArtWork";
            // 테두리 이미지
            public const string ImageBorder = Card + "_Border";
            // 타입별 UI Element 프리팹
            public const string UIElement  = Card + "_UIElement";
        }
    }
}