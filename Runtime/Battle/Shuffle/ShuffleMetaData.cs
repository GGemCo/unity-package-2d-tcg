using System;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 셔플 동작에 필요한 모드, 전략, 설정, 시드 정보를 하나로 묶어 관리하는 컨텍스트 클래스.
    /// </summary>
    public class ShuffleMetaData
    {
        /// <summary>
        /// 현재 적용 중인 셔플 모드.
        /// </summary>
        public ConfigCommonTcg.ShuffleMode Mode { get; private set; }

        /// <summary>
        /// 실제 셔플 알고리즘을 수행하는 전략 객체.
        /// </summary>
        public IShuffleStrategy Strategy { get; private set; }

        /// <summary>
        /// 셔플 시 사용되는 난수 시드 관리 객체.
        /// </summary>
        public SeedManager SeedManager { get; private set; }

        /// <summary>
        /// 가중 셔플 등 셔플 로직에 필요한 추가 설정 값.
        /// </summary>
        public ShuffleConfig Config { get; private set; }

        /// <summary>
        /// 셔플에 필요한 메타데이터를 초기화한다.
        /// </summary>
        /// <param name="mode">초기 셔플 모드.</param>
        /// <param name="seedManager">난수 시드를 관리하는 객체.</param>
        /// <param name="config">
        /// 셔플 설정 값. 지정하지 않으면 기본 설정(<see cref="ShuffleConfig"/>)이 사용된다.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="seedManager"/>가 null인 경우 발생한다.
        /// </exception>
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
        /// 셔플 모드를 변경하고, 해당 모드에 대응하는 셔플 전략 객체를 설정한다.
        /// </summary>
        /// <param name="mode">적용할 셔플 모드.</param>
        private void SetMode(ConfigCommonTcg.ShuffleMode mode)
        {
            Mode = mode;

            switch (mode)
            {
                case ConfigCommonTcg.ShuffleMode.None:
                    Strategy = new ShuffleStrategyNone();
                    break;

                case ConfigCommonTcg.ShuffleMode.PureRandom:
                    Strategy = new ShuffleStrategyPureRandom();
                    break;

                case ConfigCommonTcg.ShuffleMode.Weighted:
                    Strategy = new ShuffleStrategyWeighted();
                    break;

                case ConfigCommonTcg.ShuffleMode.PhaseWeighted:
                    Strategy = new ShuffleStrategyPhaseWeighted();
                    break;

                case ConfigCommonTcg.ShuffleMode.SeededReplay:
                    // 알고리즘은 PureRandom과 동일하지만,
                    // SeedManager.FixedSeed를 사용하여 결과 재현성을 보장한다.
                    Strategy = new ShuffleStrategyPureRandom();
                    break;

                default:
                    // 알 수 없는 모드의 경우 기본 랜덤 셔플 전략을 사용한다.
                    Strategy = new ShuffleStrategyPureRandom();
                    break;
            }
        }
    }
}
