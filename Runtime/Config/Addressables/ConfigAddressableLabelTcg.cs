using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// TCG(Addressables) 리소스에서 사용되는 Label 네이밍 규칙을 정의하는 클래스입니다.
    /// Label은 여러 에셋을 논리적으로 묶어 조회하거나 로딩하기 위한 식별자로 사용됩니다.
    /// </summary>
    public static class ConfigAddressableLabelTcg
    {
        /// <summary>
        /// 카드 관련 Addressables Label의 공통 루트 문자열입니다.
        /// SDK 이름 + 패키지 이름 + "Card" 형식으로 구성됩니다.
        /// </summary>
        private const string RootCard =
            ConfigDefine.NameSDK + "_" +
            ConfigPackageInfo.NamePackageTcg + "_Card";

        /// <summary>
        /// 카드 리소스에 적용되는 Addressables Label 모음입니다.
        /// 동일한 성격의 에셋을 그룹핑하여 일괄 로딩 등에 활용됩니다.
        /// </summary>
        public static class Card
        {
            /// <summary>
            /// 카드 일러스트(Artwork) 이미지 에셋에 부여되는 Addressables Label입니다.
            /// 여러 카드 일러스트를 한 번에 조회할 때 사용됩니다.
            /// </summary>
            public const string ImageArt = RootCard + "_ArtWork";

            /// <summary>
            /// 카드 테두리(Border) 이미지 에셋에 부여되는 Addressables Label입니다.
            /// 핸드/필드 등 다양한 테두리 리소스를 묶어 관리하는 데 사용됩니다.
            /// </summary>
            public const string ImageBorder = RootCard + "_Border";
        }
    }
}