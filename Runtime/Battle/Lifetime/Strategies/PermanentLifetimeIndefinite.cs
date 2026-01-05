namespace GGemCo2DTcg
{
    /// <summary>
    /// 기본 수명 정책: 만료 없음.
    /// (전투 종료 또는 외부 규칙에 의해 제거될 때까지 유지)
    /// </summary>
    public sealed class PermanentLifetimeIndefinite : ITcgPermanentLifetimeStrategy
    {
        public bool IsExpired => false;

        public void OnAdded(in TcgPermanentLifetimeContext context) { }
        public void OnTurnTrigger(in TcgPermanentLifetimeContext context) { }
        public void OnAbilityResolved(in TcgPermanentLifetimeContext context) { }
    }
}
