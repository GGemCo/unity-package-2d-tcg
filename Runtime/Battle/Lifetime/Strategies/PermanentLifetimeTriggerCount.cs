namespace GGemCo2DTcg
{
    /// <summary>
    /// 트리거(발동) 횟수 기반 수명 정책.
    /// - Permanent가 자신의 트리거로 능력을 실행할 때마다 1 감소
    /// - 0 이하가 되면 만료
    /// </summary>
    public sealed class PermanentLifetimeTriggerCount : ITcgPermanentLifetimeStrategy
    {
        private int _remainingTriggers;

        public bool IsExpired => _remainingTriggers <= 0;

        public PermanentLifetimeTriggerCount(int triggerCount)
        {
            _remainingTriggers = triggerCount;
        }

        public void OnAdded(in TcgPermanentLifetimeContext context)
        {
            if (_remainingTriggers < 0) _remainingTriggers = 0;
        }

        public void OnTurnTrigger(in TcgPermanentLifetimeContext context) { }

        public void OnAbilityResolved(in TcgPermanentLifetimeContext context)
        {
            if (IsExpired) return;
            _remainingTriggers--;
        }
    }
}
