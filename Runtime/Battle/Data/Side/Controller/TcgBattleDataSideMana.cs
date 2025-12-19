namespace GGemCo2DTcg
{
    /// <summary>
    /// 전투 중 한 쪽 플레이어의 마나(Mana) 상태를 관리하는 도메인 클래스입니다.
    /// 현재 마나와 최대 마나 값을 보관하고, 소모 및 회복 규칙을 담당합니다.
    /// </summary>
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
        /// 마나 데이터를 초기화합니다.
        /// </summary>
        /// <param name="current">초기 현재 마나 값입니다.</param>
        /// <param name="max">초기 최대 마나 값입니다.</param>
        public TcgBattleDataSideMana(int current, int max)
        {
            _current = current;
            _max = max;
        }

        /// <summary>
        /// 지정한 마나를 소모할 수 있는지 확인하고, 가능하다면 소모합니다.
        /// </summary>
        /// <param name="amount">소모할 마나량입니다.</param>
        /// <returns>소모에 성공하면 true, 실패하면 false를 반환합니다.</returns>
        public bool TryConsume(int amount)
        {
            if (amount <= 0) return true;
            if (_current < amount) return false;

            _current -= amount;
            return true;
        }

        /// <summary>
        /// 현재 마나를 최대 마나 값으로 회복합니다.
        /// </summary>
        public void RestoreFull()
            => _current = _max;

        /// <summary>
        /// 최대 마나를 증가시키되, 지정된 한계값을 초과하지 않습니다.
        /// </summary>
        /// <param name="amount">증가시킬 마나량입니다.</param>
        /// <param name="maxLimit">최대 마나가 도달할 수 있는 상한값입니다.</param>
        public void IncreaseMax(int amount, int maxLimit)
        {
            var newMax = _max + amount;
            if (newMax > maxLimit) newMax = maxLimit;

            _max = newMax;
        }
    }
}