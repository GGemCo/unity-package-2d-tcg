namespace GGemCo2DTcg
{
    /// <summary>
    /// 영구 효과가 자신의 트리거에 의해 능력을 발동한 횟수에 따라 만료되는 수명 정책입니다.
    /// </summary>
    /// <remarks>
    /// <para>
    /// 능력이 성공적으로 해결(Resolved)될 때마다 남은 발동 가능 횟수가 1 감소합니다.
    /// </para>
    /// <para>
    /// 남은 발동 횟수가 0 이하가 되면 만료로 간주됩니다.
    /// </para>
    /// </remarks>
    public sealed class PermanentLifetimeTriggerCount : ITcgPermanentLifetimeStrategy
    {
        /// <summary>
        /// 능력을 추가로 발동할 수 있는 남은 횟수입니다.
        /// </summary>
        private int _remainingTriggers;

        /// <summary>
        /// 현재 효과가 만료되었는지 여부를 반환합니다.
        /// </summary>
        public bool IsExpired => _remainingTriggers <= 0;

        /// <summary>
        /// 지정된 발동 가능 횟수로 수명 정책을 생성합니다.
        /// </summary>
        /// <param name="triggerCount">
        /// 능력이 해결될 수 있는 최대 횟수입니다.
        /// </param>
        public PermanentLifetimeTriggerCount(int triggerCount)
        {
            _remainingTriggers = triggerCount;
        }

        /// <summary>
        /// 영구 효과가 게임에 추가될 때 호출되며,
        /// 초기 발동 횟수에 대한 보정 처리를 수행합니다.
        /// </summary>
        /// <param name="context">
        /// 영구 효과의 수명 처리에 필요한 컨텍스트 정보입니다.
        /// </param>
        public void OnAdded(in TcgPermanentLifetimeContext context)
        {
            // 발동 횟수가 음수인 경우,
            // "무한 발동"처럼 해석되는 것을 방지하기 위해 0으로 고정합니다.
            if (_remainingTriggers < 0)
            {
                _remainingTriggers = 0;
            }
        }

        /// <summary>
        /// 턴 경과와 관련된 트리거가 발생했을 때 호출되지만,
        /// 본 수명 정책에서는 턴 기반 처리를 수행하지 않습니다.
        /// </summary>
        /// <param name="context">
        /// 현재 트리거 타입 및 관련 정보를 포함하는 컨텍스트입니다.
        /// </param>
        public void OnTurnTrigger(in TcgPermanentLifetimeContext context)
        {
            // No-op
        }

        /// <summary>
        /// 영구 효과의 능력이 해결(Resolved)된 이후 호출되며,
        /// 남은 발동 가능 횟수를 1 감소시킵니다.
        /// </summary>
        /// <param name="context">
        /// 능력 해결 시점의 컨텍스트 정보입니다.
        /// </param>
        public void OnAbilityResolved(in TcgPermanentLifetimeContext context)
        {
            if (IsExpired)
                return;

            _remainingTriggers--;
        }
    }
}
