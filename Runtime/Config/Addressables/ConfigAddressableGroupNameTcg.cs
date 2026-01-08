using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// TCG(Addressables) 리소스에서 사용되는 Group 이름 규칙을 정의하는 클래스입니다.
    /// 빌드 및 패키징 단위의 Addressables Group 명을 중앙에서 관리합니다.
    /// </summary>
    public static class ConfigAddressableGroupNameTcg
    {
        /// <summary>
        /// 카드 관련 Addressables Group의 공통 접두사입니다.
        /// SDK 이름 + 패키지 이름 + "Card" 형식으로 구성됩니다.
        /// </summary>
        private const string Card =
            ConfigDefine.NameSDK + "_" +
            ConfigPackageInfo.NamePackageTcg + "_Card";

        /// <summary>
        /// 카드 리소스 전용 Addressables Group 모음입니다.
        /// 카드와 관련된 에셋을 하위 그룹 형태로 구조화합니다.
        /// </summary>
        public static class CardGroup
        {
            /// <summary>
            /// 카드 일러스트(Artwork) 이미지 리소스가 포함된 Addressables Group 이름입니다.
            /// </summary>
            public const string ImageArtwork = Card + "_ArtWork";

            /// <summary>
            /// 카드 테두리(Border) 이미지 리소스가 포함된 Addressables Group 이름입니다.
            /// </summary>
            public const string ImageBorder = Card + "_Border";

            /// <summary>
            /// 카드 타입별 UI Element 프리팹이 포함된 Addressables Group 이름입니다.
            /// </summary>
            public const string UIElement = Card + "_UIElement";
        }
    }
}