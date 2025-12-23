using System;
using GGemCo2DCore;
using Random = UnityEngine.Random;

namespace GGemCo2DTcg
{
    /// <summary>
    /// UnityEngine.Random 시드 관리 담당.
    /// - 일반 게임: 매 판마다 다른 시드 사용
    /// - 리플레이/테스트: 고정 시드 사용
    /// </summary>
    public class SeedManager
    {
        /// <summary>
        /// 고정 시드.
        /// null 이면 실행 시간 기준으로 새로운 시드를 생성한다.
        /// </summary>
        public int? FixedSeed { get; private set; }

        /// <summary>
        /// 마지막으로 사용된 시드 값.
        /// 리플레이 저장 등에 활용할 수 있다.
        /// </summary>
        public int LastUsedSeed { get; private set; }

        public SeedManager(int? fixedSeed = null)
        {
            FixedSeed = fixedSeed;
        }

        /// <summary>
        /// 고정 시드를 설정한다.
        /// </summary>
        public void SetFixedSeed(int? fixedSeed)
        {
            FixedSeed = fixedSeed;
        }

        /// <summary>
        /// UnityEngine.Random 에 시드를 적용한다.
        /// </summary>
        public void ApplySeed()
        {
            int seed = FixedSeed ?? GenerateRuntimeSeed();
            LastUsedSeed = seed;
            Random.InitState(seed);
        }

        /// <summary>
        /// 실행 시간 기반으로 pseudo-random 시드를 생성한다.
        /// </summary>
        private int GenerateRuntimeSeed()
        {
            long ticks = DateTime.UtcNow.Ticks;
            int seed = (int)(ticks & 0x00000000FFFFFFFF) ^ (int)(ticks >> 32);
            return seed;
        }
    }
}