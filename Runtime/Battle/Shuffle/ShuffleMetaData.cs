using System;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 셔플 동작에 필요한 모드, 전략, 설정, 시드 정보를 묶어서 관리하는 컨텍스트.
    /// </summary>
    public class ShuffleMetaData
    {
        /// <summary>현재 셔플 모드.</summary>
        public ConfigCommonTcg.ShuffleMode Mode { get; private set; }

        /// <summary>실제 셔플을 수행하는 전략 객체.</summary>
        public IShuffleStrategy Strategy { get; private set; }

        /// <summary>시드 관리 담당.</summary>
        public SeedManager SeedManager { get; private set; }

        /// <summary>가중 셔플 등에 필요한 설정 값.</summary>
        public ShuffleConfig Config { get; private set; }

        public ShuffleMetaData(
            ConfigCommonTcg.ShuffleMode mode,
            SeedManager seedManager,
            ShuffleConfig config = null)
        {
            SeedManager = seedManager ?? throw new ArgumentNullException(nameof(seedManager));
            Config = config ?? new ShuffleConfig();
            SetMode(mode);
        }

        /// <summary>
        /// 셔플 모드를 변경하고, 해당 모드에 맞는 전략 객체를 설정한다.
        /// </summary>
        public void SetMode(ConfigCommonTcg.ShuffleMode mode)
        {
            Mode = mode;
            switch (mode)
            {
                case ConfigCommonTcg.ShuffleMode.PureRandom:
                    Strategy = new PureRandomShuffleStrategy();
                    break;

                case ConfigCommonTcg.ShuffleMode.Weighted:
                    Strategy = new WeightedShuffleStrategy();
                    break;

                case ConfigCommonTcg.ShuffleMode.SeededReplay:
                    // 알고리즘 자체는 PureRandom 과 동일하게 사용하되,
                    // SeedManager.FixedSeed 를 통해 결과 재현성을 보장한다.
                    Strategy = new PureRandomShuffleStrategy();
                    break;

                default:
                    Strategy = new PureRandomShuffleStrategy();
                    break;
            }
        }
    }
}