namespace GGemCo2DTcg
{
    /// <summary>
    /// Permanent 수명(만료) 정책 인터페이스.
    /// 
    /// - 전투 로직에서 Permanent 효과를 처리하는 동안,
    ///   특정 시점(턴 시작/종료, 발동 완료 등)에 수명 상태를 갱신하고
    ///   만료 여부를 판단합니다.
    /// 
    /// 설계 목표:
    /// - 도메인 계층(순수 C#)에서 동작
    /// - 테이블 기반/코드 기반 모두 지원
    /// - 기존 데이터와의 호환을 위해 기본값은 Indefinite
    /// </summary>
    public interface ITcgPermanentLifetimeStrategy
    {
        /// <summary>만료되었으면 true</summary>
        bool IsExpired { get; }

        /// <summary>
        /// 인스턴스가 Permanent 존에 추가되었을 때 1회 호출됩니다.
        /// </summary>
        void OnAdded(in TcgPermanentLifetimeContext context);

        /// <summary>
        /// 턴의 특정 트리거 시점(OnTurnStart/OnTurnEnd 등)에 호출됩니다.
        /// </summary>
        void OnTurnTrigger(in TcgPermanentLifetimeContext context);

        /// <summary>
        /// Permanent의 Ability가 1회 처리(Resolve)된 직후 호출됩니다.
        /// </summary>
        void OnAbilityResolved(in TcgPermanentLifetimeContext context);
    }
}
