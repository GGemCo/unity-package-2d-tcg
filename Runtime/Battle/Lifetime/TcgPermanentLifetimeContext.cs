namespace GGemCo2DTcg
{
    /// <summary>
    /// Permanent 수명(Lifetime) 전략이 만료 여부를 판단하는 데 사용하는 컨텍스트 구조체입니다.
    /// </summary>
    /// <remarks>
    /// <para>
    /// 전투 로직에서 특정 시점(턴 트리거, 능력 해결 등)에 수명 상태를 평가하기 위해 전달됩니다.
    /// </para>
    /// <para>
    /// 설계 원칙:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>되도록 작은 값 타입(<c>struct</c>)으로 유지</description></item>
    ///   <item><description>UI, MonoBehaviour, Unity 오브젝트에 의존하지 않음</description></item>
    ///   <item><description>도메인 계층에서 안전하게 사용 가능</description></item>
    /// </list>
    /// </remarks>
    public readonly struct TcgPermanentLifetimeContext
    {
        /// <summary>
        /// 현재 전투에서의 턴 카운트 값입니다.
        /// </summary>
        /// <remarks>
        /// 턴 기반 수명 정책(DurationTurns 등)에서 기준 정보로 사용될 수 있습니다.
        /// </remarks>
        public readonly int turnCount;

        /// <summary>
        /// 현재 수명 갱신을 유발한 능력 트리거 타입입니다.
        /// </summary>
        /// <remarks>
        /// 구현체는 이 값을 기준으로 관심 있는 트리거인지 판단하여
        /// 수명 상태를 갱신할 수 있습니다.
        /// </remarks>
        public readonly TcgAbilityConstants.TcgAbilityTriggerType triggerType;

        /// <summary>
        /// 수명 정책이 적용되는 대상 Permanent 인스턴스입니다.
        /// </summary>
        /// <remarks>
        /// Permanent의 상태나 식별 정보가 필요한 경우 참조 용도로 사용됩니다.
        /// </remarks>
        public readonly TcgBattlePermanentInstance permanent;

        /// <summary>
        /// 수명 판단에 필요한 정보를 포함하는 컨텍스트를 생성합니다.
        /// </summary>
        /// <param name="turnCount">현재 전투의 턴 카운트 값입니다.</param>
        /// <param name="triggerType">수명 갱신을 유발한 트리거 타입입니다.</param>
        /// <param name="permanent">수명 정책이 적용되는 Permanent 인스턴스입니다.</param>
        public TcgPermanentLifetimeContext(
            int turnCount,
            TcgAbilityConstants.TcgAbilityTriggerType triggerType,
            TcgBattlePermanentInstance permanent)
        {
            this.turnCount = turnCount;
            this.triggerType = triggerType;
            this.permanent = permanent;
        }
    }
}
