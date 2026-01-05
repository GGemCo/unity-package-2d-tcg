namespace GGemCo2DTcg
{
    /// <summary>
    /// 일정 턴 수가 지나면 만료되는 정책.
    /// 
    /// - remainingTurns 는 1 이상일 때 유효
    /// - tickTrigger 가 발생할 때마다 remainingTurns 가 1 감소
    /// </summary>
    public sealed class PermanentLifetimeDurationTurns : ITcgPermanentLifetimeStrategy
    {
        private int _remainingTurns;
        private readonly TcgAbilityConstants.TcgAbilityTriggerType _tickTrigger;

        public bool IsExpired => _remainingTurns <= 0;

        public PermanentLifetimeDurationTurns(int durationTurns, TcgAbilityConstants.TcgAbilityTriggerType tickTrigger)
        {
            _remainingTurns = durationTurns;
            _tickTrigger = tickTrigger;
        }

        public void OnAdded(in TcgPermanentLifetimeContext context)
        {
            // durationTurns <= 0 이면 즉시 만료로 취급하지 않고, "무한"처럼 동작하는 것을 막기 위해 0으로 고정합니다.
            if (_remainingTurns < 0) _remainingTurns = 0;
        }

        public void OnTurnTrigger(in TcgPermanentLifetimeContext context)
        {
            if (IsExpired) return;
            if (context.triggerType != _tickTrigger) return;
            _remainingTurns--;
        }

        public void OnAbilityResolved(in TcgPermanentLifetimeContext context) { }
    }
}
