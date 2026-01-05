namespace GGemCo2DTcg
{
    /// <summary>
    /// Lifetime 전략이 판단에 사용할 컨텍스트.
    /// - 되도록 작은 값 타입으로 유지하고
    /// - UI/MonoBehaviour에 의존하지 않도록 합니다.
    /// </summary>
    public readonly struct TcgPermanentLifetimeContext
    {
        public readonly int turnCount;
        public readonly TcgAbilityConstants.TcgAbilityTriggerType triggerType;
        public readonly TcgBattlePermanentInstance permanent;

        public TcgPermanentLifetimeContext(int turnCount, TcgAbilityConstants.TcgAbilityTriggerType triggerType, TcgBattlePermanentInstance permanent)
        {
            this.turnCount = turnCount;
            this.triggerType = triggerType;
            this.permanent = permanent;
        }
    }
}
