namespace GGemCo2DTcg
{
    /// <summary>
    /// 만료 조건이 없는 기본 영구 효과 수명 정책입니다.
    /// </summary>
    /// <remarks>
    /// <para>
    /// 이 정책은 내부 상태에 의해 만료되지 않으며,
    /// 전투 종료나 외부 규칙에 의해 명시적으로 제거될 때까지 유지됩니다.
    /// </para>
    /// <para>
    /// 시간 경과, 턴 트리거, 능력 해결 여부와 무관하게 동작이 변경되지 않습니다.
    /// </para>
    /// </remarks>
    public sealed class PermanentLifetimeIndefinite : ITcgPermanentLifetimeStrategy
    {
        /// <summary>
        /// 이 수명 정책은 만료되지 않으므로 항상 <c>false</c>를 반환합니다.
        /// </summary>
        public bool IsExpired => false;

        /// <summary>
        /// 영구 효과가 게임에 추가될 때 호출되지만,
        /// 본 수명 정책에서는 별도의 초기화 처리를 수행하지 않습니다.
        /// </summary>
        /// <param name="context">
        /// 영구 효과의 수명 처리에 필요한 컨텍스트 정보입니다.
        /// </param>
        public void OnAdded(in TcgPermanentLifetimeContext context)
        {
            // No-op
        }

        /// <summary>
        /// 턴 관련 트리거가 발생했을 때 호출되지만,
        /// 본 수명 정책에서는 턴 경과에 따른 처리를 수행하지 않습니다.
        /// </summary>
        /// <param name="context">
        /// 현재 트리거 타입 및 관련 정보를 포함하는 컨텍스트입니다.
        /// </param>
        public void OnTurnTrigger(in TcgPermanentLifetimeContext context)
        {
            // No-op
        }

        /// <summary>
        /// 능력이 해결(Resolved)된 이후 호출되지만,
        /// 본 수명 정책에서는 별도의 처리를 수행하지 않습니다.
        /// </summary>
        /// <param name="context">
        /// 능력 해결 시점의 컨텍스트 정보입니다.
        /// </param>
        public void OnAbilityResolved(in TcgPermanentLifetimeContext context)
        {
            // No-op
        }
    }
}