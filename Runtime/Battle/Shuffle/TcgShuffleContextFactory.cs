using GGemCo2DTcg;
using UnityEngine;

public static class TcgShuffleContextFactory
{
    public static ShuffleMetaData Create(
        GGemCoTcgSettings settings,
        SeedManager seedManager,
        int seed,
        int deckSize,
        ConfigCommonTcg.ShuffleMode fallbackMode = ConfigCommonTcg.ShuffleMode.PureRandom)
    {
        // seed 적용은 SeedManager에서 수행
        seedManager.SetFixedSeed(seed > 0 ? seed : (int?)null);

        // fallback(기존 Weighted/PureRandom)
        var config2 = new ShuffleConfig();
        var mode = fallbackMode;
        return new ShuffleMetaData(mode, seedManager, config2);
    }
}