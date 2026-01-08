namespace GGemCo2DTcg
{
    /// <summary>
    /// Permanent 효과의 수명(만료) 규칙을 정의하는 전략 인터페이스입니다.
    /// </summary>
    /// <remarks>
    /// <para>
    /// 전투 로직에서 Permanent 효과를 처리하는 동안, 특정 시점(턴 시작/종료, 발동 처리 완료 등)에
    /// 수명 상태를 갱신하고 만료 여부를 판단하는 데 사용됩니다.
    /// </para>
    /// <para><b>설계 목표</b></para>
    /// <list type="bullet">
    ///   <item><description>도메인 계층(순수 C#)에서 동작</description></item>
    ///   <item><description>테이블 기반/코드 기반 수명 정책 모두 지원</description></item>
    ///   <item><description>기존 데이터 호환을 위해 기본값은 Indefinite 정책</description></item>
    /// </list>
    /// </remarks>
    public interface ITcgPermanentLifetimeStrategy
    {
        /// <summary>
        /// 현재 수명 정책 기준으로 Permanent가 만료되었는지 여부를 반환합니다.
        /// </summary>
        /// <remarks>
        /// 구현체는 내부 상태(남은 턴 수/남은 발동 횟수/내구도 등)에 따라 값을 계산합니다.
        /// </remarks>
        bool IsExpired { get; }

        /// <summary>
        /// Permanent 인스턴스가 존(Zone)에 추가될 때 1회 호출됩니다.
        /// </summary>
        /// <param name="context">수명 상태 갱신에 필요한 컨텍스트 정보입니다.</param>
        /// <remarks>
        /// 초기값 보정(음수 방지 등)이나, 시작 상태 설정에 사용됩니다.
        /// </remarks>
        void OnAdded(in TcgPermanentLifetimeContext context);

        /// <summary>
        /// 턴 진행 중 특정 트리거 시점(예: 턴 시작/턴 종료)에 호출됩니다.
        /// </summary>
        /// <param name="context">현재 트리거 타입 및 관련 정보를 포함하는 컨텍스트입니다.</param>
        /// <remarks>
        /// 구현체는 <c>context.triggerType</c> 등을 확인하여, 자신이 관심 있는 트리거에서만 상태를 갱신할 수 있습니다.
        /// </remarks>
        void OnTurnTrigger(in TcgPermanentLifetimeContext context);

        /// <summary>
        /// Permanent의 Ability가 1회 처리(Resolve)된 직후 호출됩니다.
        /// </summary>
        /// <param name="context">능력 해결 시점의 컨텍스트 정보입니다.</param>
        /// <remarks>
        /// 발동 횟수 기반 수명(TriggerCount)과 같이, 능력 해결을 “소모”로 간주하는 정책에서 주로 사용됩니다.
        /// </remarks>
        void OnAbilityResolved(in TcgPermanentLifetimeContext context);
    }
}