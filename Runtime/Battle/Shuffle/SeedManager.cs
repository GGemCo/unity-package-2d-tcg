using System;
using GGemCo2DCore;
using Random = UnityEngine.Random;

namespace GGemCo2DTcg
{
    /// <summary>
    /// UnityEngine.Random의 시드 적용을 일원화하여 관리하는 클래스입니다.
    /// </summary>
    /// <remarks>
    /// <para>
    /// 게임 플레이, 테스트, 리플레이 등 다양한 실행 환경에서
    /// 난수 결과를 제어/재현할 수 있도록 시드 정책을 캡슐화합니다.
    /// </para>
    /// <para><b>사용 시나리오</b></para>
    /// <list type="bullet">
    ///   <item><description>일반 게임 플레이: 실행 시점마다 다른 시드 사용</description></item>
    ///   <item><description>리플레이/자동 테스트: 고정 시드 사용</description></item>
    /// </list>
    /// </remarks>
    public class SeedManager
    {
        /// <summary>
        /// 고정 시드 값입니다.
        /// </summary>
        /// <remarks>
        /// <para>
        /// 값이 <c>null</c>이면 실행 시간 기반으로 새로운 시드를 생성합니다.
        /// </para>
        /// <para>
        /// 고정 시드가 설정된 경우, 동일한 시드로 항상 동일한 난수 결과를 재현할 수 있습니다.
        /// </para>
        /// </remarks>
        public int? FixedSeed { get; private set; }

        /// <summary>
        /// 마지막으로 <see cref="ApplySeed"/>에서 실제로 적용된 시드 값입니다.
        /// </summary>
        /// <remarks>
        /// 리플레이 데이터 저장, 디버깅, 로그 기록 등에 활용할 수 있습니다.
        /// </remarks>
        public int LastUsedSeed { get; private set; }

        /// <summary>
        /// 시드 매니저를 생성합니다.
        /// </summary>
        /// <param name="fixedSeed">
        /// 고정 시드 값입니다. <c>null</c>이면 실행 시간 기반 시드를 사용합니다.
        /// </param>
        public SeedManager(int? fixedSeed = null)
        {
            FixedSeed = fixedSeed;
        }

        /// <summary>
        /// 고정 시드를 설정하거나 해제합니다.
        /// </summary>
        /// <param name="fixedSeed">
        /// 설정할 고정 시드 값입니다.
        /// <c>null</c>을 전달하면 고정 시드를 해제합니다.
        /// </param>
        public void SetFixedSeed(int? fixedSeed)
        {
            FixedSeed = fixedSeed;
        }

        /// <summary>
        /// 현재 시드 정책에 따라 UnityEngine.Random에 시드를 적용합니다.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="FixedSeed"/>가 설정되어 있으면 해당 값을 사용하고,
        /// 그렇지 않으면 실행 시간 기반 시드를 생성하여 사용합니다.
        /// </para>
        /// <para>
        /// 적용된 시드 값은 <see cref="LastUsedSeed"/>에 기록됩니다.
        /// </para>
        /// </remarks>
        public void ApplySeed()
        {
            int seed = FixedSeed ?? GenerateRuntimeSeed();
            LastUsedSeed = seed;
            Random.InitState(seed);
        }

        /// <summary>
        /// 실행 시간(<see cref="DateTime.UtcNow"/>)을 기반으로
        /// pseudo-random 시드 값을 생성합니다.
        /// </summary>
        /// <returns>생성된 시드 값입니다.</returns>
        /// <remarks>
        /// <para>
        /// 현재 UTC tick 값을 상/하위 32비트로 분리한 뒤 XOR하여
        /// 비교적 분산된 정수 시드를 생성합니다.
        /// </para>
        /// <para>
        /// 암호학적으로 안전한 난수는 아니며,
        /// 게임 플레이용 시드 생성 목적에 적합합니다.
        /// </para>
        /// </remarks>
        private int GenerateRuntimeSeed()
        {
            long ticks = DateTime.UtcNow.Ticks;
            int seed = (int)(ticks & 0x00000000FFFFFFFF) ^ (int)(ticks >> 32);
            return seed;
        }
    }
}
