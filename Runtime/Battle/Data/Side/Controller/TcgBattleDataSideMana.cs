namespace GGemCo2DTcg
{
    /// <summary>
    /// 전투 중 한 쪽 플레이어의 마나(Mana) 상태를 관리하는 도메인 클래스입니다.
    /// 현재 마나와 최대 마나를 보관하며, 소모/회복/증가 규칙을 제공합니다.
    /// </summary>
    /// <remarks>
    /// 이 클래스는 순수 상태(값)만 관리하며, 턴 시작/종료 시점의 갱신 정책(예: 턴 시작 시 최대 마나 증가 후 전량 회복)은
    /// BattleManager 등 상위 로직에서 호출 순서로 구현하는 것을 전제로 합니다.
    /// </remarks>
    public sealed class TcgBattleDataSideMana
    {
        /// <summary>
        /// 현재 사용 가능한 마나 값입니다.
        /// </summary>
        private int _current;

        /// <summary>
        /// 현재 턴 기준 최대 마나 값입니다.
        /// </summary>
        private int _max;

        /// <summary>
        /// 현재 사용 가능한 마나를 반환합니다.
        /// </summary>
        public int Current => _current;

        /// <summary>
        /// 현재 최대 마나를 반환합니다.
        /// </summary>
        public int Max => _max;

        /// <summary>
        /// 마나 상태를 생성합니다.
        /// </summary>
        /// <param name="current">초기 현재 마나 값입니다.</param>
        /// <param name="max">초기 최대 마나 값입니다.</param>
        /// <remarks>
        /// 호출부에서 유효한 값(예: 0 이상, current &lt;= max)을 보장한다는 전제를 둘 수 있습니다.
        /// 필요하다면 생성 시점에 클램프/검증 로직을 추가하세요.
        /// </remarks>
        public TcgBattleDataSideMana(int current, int max)
        {
            _current = current;
            _max = max;
        }

        /// <summary>
        /// 지정한 마나를 소모할 수 있는지 확인하고, 가능하다면 즉시 소모합니다.
        /// </summary>
        /// <param name="amount">소모할 마나량입니다. 0 이하이면 소모 없이 성공 처리합니다.</param>
        /// <returns>소모에 성공하면 true, 현재 마나가 부족하면 false입니다.</returns>
        public bool TryConsume(int amount)
        {
            if (amount <= 0) return true;
            if (_current < amount) return false;

            _current -= amount;
            return true;
        }

        /// <summary>
        /// 현재 마나를 최대 마나 값으로 전량 회복합니다.
        /// </summary>
        public void RestoreFull()
            => _current = _max;

        /// <summary>
        /// 최대 마나를 증가시키되, 지정된 상한(<paramref name="maxLimit"/>)을 초과하지 않도록 합니다.
        /// </summary>
        /// <param name="amount">증가시킬 마나량입니다.</param>
        /// <param name="maxLimit">최대 마나 상한값입니다.</param>
        /// <remarks>
        /// 현재 구현은 최대 마나만 증가시키며, 현재 마나는 자동으로 증가시키지 않습니다.
        /// (예: 턴 시작 규칙에서 IncreaseMax 후 RestoreFull을 호출하는 형태를 전제로 할 수 있습니다.)
        /// </remarks>
        public void IncreaseMax(int amount, int maxLimit)
        {
            var newMax = _max + amount;
            if (newMax > maxLimit) newMax = maxLimit;

            _max = newMax;
        }

        /// <summary>
        /// 현재 마나를 증가시킵니다.
        /// </summary>
        /// <param name="amount">증가시킬 마나량입니다. 0 이하이면 무시합니다.</param>
        /// <remarks>
        /// - 현재 마나만 증가시키며, 최대 마나(<see cref="Max"/>)를 초과하지 않도록 클램프(clamp)합니다.
        /// - 최대 마나 자체를 늘리려면 <see cref="IncreaseMax"/>를 사용하세요.
        /// </remarks>
        public void Add(int amount)
        {
            if (amount <= 0) return;

            _current += amount;
            if (_current > _max) _current = _max;
        }
    }
}
