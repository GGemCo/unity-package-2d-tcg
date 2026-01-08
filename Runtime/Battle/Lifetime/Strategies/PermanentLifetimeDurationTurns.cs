namespace GGemCo2DTcg
{
    /// <summary>
    /// 일정 턴 수(durationTurns) 동안 유지되며,
    /// 지정된 트리거가 발생할 때마다 남은 턴 수를 감소시키는 영구 효과 수명 정책입니다.
    /// </summary>
    /// <remarks>
    /// <para>- 남은 턴 수가 1 이상일 때만 유효합니다.</para>
    /// <para>- 지정된 트리거가 발생할 때마다 남은 턴 수가 1 감소합니다.</para>
    /// <para>- 남은 턴 수가 0 이하가 되면 만료로 간주됩니다.</para>
    /// </remarks>
    public sealed class PermanentLifetimeDurationTurns : ITcgPermanentLifetimeStrategy
    {
        /// <summary>
        /// 효과가 유지되는 남은 턴 수입니다.
        /// </summary>
        private int _remainingTurns;

        /// <summary>
        /// 턴 경과로 간주되는 능력 트리거 타입입니다.
        /// </summary>
        private readonly TcgAbilityConstants.TcgAbilityTriggerType _tickTrigger;

        /// <summary>
        /// 현재 효과가 만료되었는지 여부를 반환합니다.
        /// </summary>
        public bool IsExpired => _remainingTurns <= 0;

        /// <summary>
        /// 지정된 턴 수와 트리거 타입으로 수명 정책을 생성합니다.
        /// </summary>
        /// <param name="durationTurns">
        /// 효과가 유지될 턴 수로, 트리거 발생 시마다 1씩 감소합니다.
        /// </param>
        /// <param name="tickTrigger">
        /// 턴 경과로 처리할 능력 트리거 타입입니다.
        /// </param>
        public PermanentLifetimeDurationTurns(
            int durationTurns,
            TcgAbilityConstants.TcgAbilityTriggerType tickTrigger)
        {
            _remainingTurns = durationTurns;
            _tickTrigger = tickTrigger;
        }

        /// <summary>
        /// 영구 효과가 게임에 추가될 때 호출되며,
        /// 초기 턴 수에 대한 보정 처리를 수행합니다.
        /// </summary>
        /// <param name="context">
        /// 영구 효과의 수명 처리에 필요한 컨텍스트 정보입니다.
        /// </param>
        public void OnAdded(in TcgPermanentLifetimeContext context)
        {
            // durationTurns가 음수인 경우 즉시 만료 처리 대신,
            // "무한 지속"처럼 오동작하는 것을 방지하기 위해 0으로 고정합니다.
            if (_remainingTurns < 0)
            {
                _remainingTurns = 0;
            }
        }

        /// <summary>
        /// 턴 관련 트리거가 발생했을 때 호출되며,
        /// 지정된 트리거와 일치하는 경우 남은 턴 수를 감소시킵니다.
        /// </summary>
        /// <param name="context">
        /// 현재 트리거 타입 및 관련 정보를 포함하는 컨텍스트입니다.
        /// </param>
        public void OnTurnTrigger(in TcgPermanentLifetimeContext context)
        {
            if (IsExpired)
                return;

            if (context.triggerType != _tickTrigger)
                return;

            _remainingTurns--;
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
