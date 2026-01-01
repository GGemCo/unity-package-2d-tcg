using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// Addressables 관련 모든 경로 규칙의 단일 소스(SoT).
    /// - 경로는 반드시 여기서만 생성/관리합니다.
    /// - OS 간 슬래시/특수문자 정규화를 제공합니다.
    /// - 다른 Config* 클래스는 본 클래스를 통해 경로를 참조하세요.
    /// </summary>
    public static class ConfigAddressablePathTcg
    {
        private static string RootPrefabTcg => ConfigAddressablePath.Combine(ConfigAddressablePath.Prefab.RootPrefab, ConfigPackageInfo.NamePackageTcg);        
        private static string RootImageTcg => ConfigAddressablePath.Combine(ConfigAddressablePath.Images.RootImage, ConfigPackageInfo.NamePackageTcg);
        
        // 카드 (하위 그룹 형태로 구조화)
        public static class Card
        {
            // 일러스트 이미지
            public static string ImageArt => ConfigAddressablePath.Combine(RootImageTcg, "CardArt");
            public static string ImageBorderHand => ConfigAddressablePath.Combine(RootImageTcg, "CardBorderHand");
            public static string ImageBorderField => ConfigAddressablePath.Combine(RootImageTcg, "CardBorderField");
        }
    }
}
