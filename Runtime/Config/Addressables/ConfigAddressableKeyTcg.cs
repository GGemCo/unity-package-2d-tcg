using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// TCG(Addressables) 리소스에서 사용되는 Key 네이밍 규칙을 정의하는 클래스입니다.
    /// Key는 개별 에셋을 식별하기 위한 고유 식별자로 사용됩니다.
    /// </summary>
    public static class ConfigAddressableKeyTcg
    {
        /// <summary>
        /// 카드 관련 Addressables Key의 공통 루트 문자열입니다.
        /// SDK 이름 + 패키지 이름 + "Card" 형식으로 구성됩니다.
        /// </summary>
        private const string RootCard =
            ConfigDefine.NameSDK + "_" +
            ConfigPackageInfo.NamePackageTcg + "_Card";

        /// <summary>
        /// 카드 에셋에서 사용되는 Addressables Key 모음입니다.
        /// 카드 리소스를 용도별로 구분하여 개별 Key를 정의합니다.
        /// </summary>
        public static class Card
        {
            /// <summary>
            /// 카드 일러스트(Artwork) 이미지 에셋의 Addressables Key입니다.
            /// </summary>
            public const string ImageArt = RootCard + "_ArtWork";

            /// <summary>
            /// 핸드 영역에 표시되는 카드 테두리(Border) 이미지 에셋의 Addressables Key입니다.
            /// </summary>
            public const string ImageBorderHand = RootCard + "_BorderHand";

            /// <summary>
            /// 필드 영역에 표시되는 카드 테두리(Border) 이미지 에셋의 Addressables Key입니다.
            /// </summary>
            public const string ImageBorderField = RootCard + "_BorderField";
        }
    }
}