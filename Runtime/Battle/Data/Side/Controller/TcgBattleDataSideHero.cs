namespace GGemCo2DTcg
{
    /// <summary>
    /// 전투 중 한 쪽 플레이어의 영웅(Hero) 상태를 관리하는 도메인 클래스입니다.
    /// 영웅 카드 데이터와 필드에 배치된 영웅 인스턴스를 함께 관리합니다.
    /// </summary>
    public sealed class TcgBattleDataSideHero
    {
        /// <summary>
        /// 이 영웅이 속한 플레이어 진영입니다.
        /// </summary>
        public ConfigCommonTcg.TcgPlayerSide Side { get; }

        /// <summary>
        /// 영웅의 원본 카드 데이터입니다.
        /// </summary>
        private TcgBattleDataCard _heroCard;

        /// <summary>
        /// 전투 필드에 배치된 영웅 카드의 런타임 데이터입니다.
        /// </summary>
        private TcgBattleDataFieldCard _heroField;

        /// <summary>
        /// 영웅의 카드 데이터에 대한 읽기 전용 접근자입니다.
        /// </summary>
        public TcgBattleDataCard HeroCard => _heroCard;

        /// <summary>
        /// 필드에 존재하는 영웅 카드의 런타임 데이터에 대한 읽기 전용 접근자입니다.
        /// </summary>
        public TcgBattleDataFieldCard HeroField => _heroField;

        /// <summary>
        /// 영웅 데이터 슬롯을 초기화합니다.
        /// </summary>
        /// <param name="side">이 영웅이 속한 플레이어 진영입니다.</param>
        public TcgBattleDataSideHero(ConfigCommonTcg.TcgPlayerSide side)
        {
            Side = side;
        }

        /// <summary>
        /// 영웅 카드 데이터를 설정하고 전투용 필드 데이터를 생성합니다.
        /// 일반적으로 전투 시작 시 한 번 호출됩니다.
        /// </summary>
        /// <param name="cardHero">영웅으로 사용할 카드 데이터입니다.</param>
        public void SetHero(TcgBattleDataCard cardHero)
        {
            if (cardHero == null) return;

            _heroCard = cardHero;
            _heroField = TcgBattleDataCardFactory.CreateBattleDataFieldCard(Side, cardHero);
        }

        /// <summary>
        /// 전달된 필드 카드가 이 플레이어의 영웅인지 여부를 확인합니다.
        /// </summary>
        /// <param name="target">확인할 필드 카드 데이터입니다.</param>
        /// <returns>영웅이면 true, 아니면 false를 반환합니다.</returns>
        public bool Contains(TcgBattleDataFieldCard target)
        {
            if (_heroField == null || target == null) return false;
            return _heroField.Uid == target.Uid;
        }

        /// <summary>
        /// 현재 영웅의 체력(Hp)을 반환합니다.
        /// </summary>
        /// <returns>영웅의 현재 Hp 값입니다.</returns>
        public int GetHp()
            => _heroField.Hp;

        /// <summary>
        /// 영웅에게 피해를 적용합니다.
        /// </summary>
        /// <param name="contextValue">적용할 피해량입니다.</param>
        public void TakeDamage(int contextValue)
        {
            _heroField.ApplyDamage(contextValue);
        }
    }
}
