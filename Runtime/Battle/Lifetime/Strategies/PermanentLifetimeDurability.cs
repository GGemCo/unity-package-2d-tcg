namespace GGemCo2DTcg
{
    /// <summary>
    /// 내구도 기반 수명 정책.
    /// - Ability가 해결(Resolved)될 때마다 내구도 1 감소
    /// - 0 이하가 되면 만료
    /// 
    /// 주로 "사용 횟수 제한" 형태의 Permanent에 사용합니다.
    /// </summary>
    public sealed class PermanentLifetimeDurability : ITcgPermanentLifetimeStrategy
    {
        private int _durability;

        public bool IsExpired => _durability <= 0;

        public PermanentLifetimeDurability(int durability)
        {
            _durability = durability;
        }

        public void OnAdded(in TcgPermanentLifetimeContext context)
        {
            if (_durability < 0) _durability = 0;
        }

        public void OnTurnTrigger(in TcgPermanentLifetimeContext context) { }

        public void OnAbilityResolved(in TcgPermanentLifetimeContext context)
        {
            if (IsExpired) return;
            _durability--;
        }
    }
}
