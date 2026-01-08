namespace GGemCo2DTcg
{
    /// <summary>
    /// 내구도(Durability) 기반의 Permanent 수명(Lifetime) 전략입니다.
    /// Ability가 한 번 해결(Resolve)될 때마다 내구도가 1씩 감소하며,
    /// 내구도가 0 이하가 되면 해당 Permanent는 만료(Expired) 상태가 됩니다.
    /// </summary>
    /// <remarks>
    /// 주로 “사용 횟수 제한” 형태의 Permanent(예: N회 발동 후 사라짐)에 사용됩니다.
    /// 내구도 감소 타이밍은 <see cref="OnAbilityResolved"/> 호출 시점에 의존합니다.
    /// </remarks>
    public sealed class PermanentLifetimeDurability : ITcgPermanentLifetimeStrategy
    {
        /// <summary>
        /// 남아 있는 내구도 값입니다.
        /// </summary>
        private int _durability;

        /// <summary>
        /// 내구도가 소진되어 만료되었는지 여부입니다.
        /// </summary>
        public bool IsExpired => _durability <= 0;

        /// <summary>
        /// 내구도 기반 Lifetime 전략을 생성합니다.
        /// </summary>
        /// <param name="durability">초기 내구도 값입니다.</param>
        /// <remarks>
        /// 0 이하로 전달되면 즉시 만료 상태가 됩니다.
        /// </remarks>
        public PermanentLifetimeDurability(int durability)
        {
            _durability = durability;
        }

        /// <summary>
        /// Permanent가 필드/영역에 추가될 때 호출됩니다.
        /// </summary>
        /// <param name="context">Lifetime 처리에 필요한 컨텍스트입니다.</param>
        /// <remarks>
        /// 음수 내구도가 들어온 경우 0으로 보정합니다.
        /// </remarks>
        public void OnAdded(in TcgPermanentLifetimeContext context)
        {
            if (_durability < 0) _durability = 0;
        }

        /// <summary>
        /// 턴 단위 트리거 발생 시 호출됩니다.
        /// </summary>
        /// <param name="context">Lifetime 처리에 필요한 컨텍스트입니다.</param>
        /// <remarks>
        /// 내구도 기반 전략에서는 턴 트리거에 별도 동작을 하지 않습니다.
        /// </remarks>
        public void OnTurnTrigger(in TcgPermanentLifetimeContext context) { }

        /// <summary>
        /// Ability가 해결(Resolve)된 직후 호출됩니다.
        /// </summary>
        /// <param name="context">Lifetime 처리에 필요한 컨텍스트입니다.</param>
        /// <remarks>
        /// 이미 만료된 상태라면 아무 작업도 하지 않습니다.
        /// </remarks>
        public void OnAbilityResolved(in TcgPermanentLifetimeContext context)
        {
            if (IsExpired) return;
            _durability--;
        }
    }
}
