namespace GGemCo2DTcg
{
    /// <summary>
    /// 전투에서 한 쪽 플레이어(Player / Enemy)의 런타임 상태를 관리하는 도메인 클래스입니다.
    /// 
    /// 이 클래스는 다음 하위 영역(Zone/Slot/Pool)을 조합하여 구성됩니다.
    /// - 덱 (Deck)
    /// - 손패 (Hand)
    /// - 필드 (Board)
    /// - 영웅 (Hero)
    /// - 마나 (Mana)
    /// 
    /// UI, MonoBehaviour, Scene 에 의존하지 않는 순수 전투 데이터이며,
    /// 외부에서는 읽기 전용 접근만 허용하고, 변경은 제공된 메서드를 통해서만 이루어지도록 설계합니다.
    /// </summary>
    public sealed class TcgBattleDataSide
    {
        /// <summary>
        /// 이 전투 데이터가 속한 플레이어 진영입니다.
        /// (Player / Enemy)
        /// </summary>
        public ConfigCommonTcg.TcgPlayerSide Side { get; }

        /// <summary>
        /// 해당 플레이어의 전투용 덱 데이터입니다.
        /// 카드 드로우, 덱 고갈 여부 확인 등에 사용됩니다.
        /// </summary>
        public TcgBattleDataDeck<TcgBattleDataCard> TcgBattleDataDeck { get; }

        /// <summary>
        /// 손패 영역을 관리하는 객체입니다.
        /// 카드 추가/제거, 최대 손패 크기 제한을 담당합니다.
        /// </summary>
        public TcgBattleDataSideHand Hand { get; }

        /// <summary>
        /// 필드(보드)에 배치된 카드들을 관리하는 객체입니다.
        /// 유닛 추가/제거, 공격 가능 여부 일괄 처리 등을 담당합니다.
        /// </summary>
        public TcgBattleDataSideBoard Board { get; }

        /// <summary>
        /// 영웅 카드 및 영웅 필드 데이터를 관리하는 객체입니다.
        /// </summary>
        public TcgBattleDataSideHero Hero { get; }

        /// <summary>
        /// 현재 마나 / 최대 마나를 관리하는 객체입니다.
        /// </summary>
        public TcgBattleDataSideMana Mana { get; }

        /// <summary>
        /// 게임 도중 이 플레이어가 가질 수 있는 최대 마나 한계값입니다.
        /// (예: 하스스톤의 10 마나)
        /// </summary>
        private int _maxMana;

        /// <summary>
        /// 손패의 최대 카드 수입니다.
        /// 이 값을 초과하여 드로우할 경우 오버드로우 규칙이 적용됩니다.
        /// </summary>
        private const int MaxHandSize = 10;

        /// <summary>
        /// 덱이 고갈된 상태에서 카드를 드로우할 때 증가하는 피로 카운터입니다.
        /// 일반적으로 영웅에게 누적 피해를 주는 데 사용됩니다.
        /// </summary>
        private int _fatigueCounter = 0;

        /// <summary>
        /// 전투 중 한 쪽 플레이어의 런타임 데이터를 생성합니다.
        /// </summary>
        /// <param name="side"> 이 데이터가 속한 플레이어 진영(Player / Enemy) </param>
        /// <param name="deckRuntime"> 전투에 사용할 덱 런타임 데이터 </param>
        /// <param name="currentMana"> 전투 시작 시 현재 마나 값 </param>
        /// <param name="currentManaMax"> 전투 시작 시 최대 마나 값 </param>
        /// <param name="maxMana"> 전투 중 도달할 수 있는 최대 마나 한계값 </param>
        public TcgBattleDataSide(
            ConfigCommonTcg.TcgPlayerSide side,
            TcgBattleDataDeck<TcgBattleDataCard> deckRuntime,
            int currentMana,
            int currentManaMax,
            int maxMana)
        {
            Side = side;
            TcgBattleDataDeck = deckRuntime;

            Hand = new TcgBattleDataSideHand(initialCapacity: 16, maxSize: MaxHandSize);
            Board = new TcgBattleDataSideBoard(initialCapacity: 8);
            Hero = new TcgBattleDataSideHero(side);

            // 마나는 현재 값과 최대값으로 초기화되며,
            // 전투 중 IncreaseMaxMana 를 통해 단계적으로 증가합니다.
            Mana = new TcgBattleDataSideMana(currentMana, currentManaMax);

            _maxMana = maxMana;
        }

        #region Board

        /// <summary>
        /// 필드에 배치된 모든 카드의 공격 가능 여부를 일괄 설정합니다.
        /// 주로 턴 시작/종료 시점에 사용됩니다.
        /// </summary>
        /// <param name="value"> true: 공격 가능 / false: 공격 불가 </param>
        public void SetBoardCardCanAttack(bool value)
            => Board.SetAllCanAttack(value);

        #endregion

        #region Hero

        /// <summary>
        /// 이 플레이어의 영웅 카드를 설정합니다.
        /// 전투 시작 시 한 번 호출되는 것이 일반적입니다.
        /// </summary>
        /// <param name="cardHero"> 영웅으로 사용할 카드 데이터 </param>
        public void AddCardToHandHero(TcgBattleDataCard cardHero)
            => Hero.SetHero(cardHero);

        /// <summary>
        /// 전달된 필드 카드가 이 플레이어의 영웅인지 여부를 반환합니다.
        /// </summary>
        /// <param name="target"> 비교할 필드 카드 </param>
        /// <returns> 영웅이면 true, 아니면 false </returns>
        public bool ContainsInHero(TcgBattleDataFieldCard target)
            => Hero.Contains(target);

        #endregion

        #region Mana

        /// <summary>
        /// 지정한 마나를 소모할 수 있는지 확인하고, 가능하다면 소모합니다.
        /// </summary>
        /// <param name="amount"> 소모할 마나량 </param>
        /// <returns> 소모 성공 여부 </returns>
        public bool TryConsumeMana(int amount)
            => Mana.TryConsume(amount);

        /// <summary>
        /// 현재 마나를 최대 마나 값으로 회복합니다.
        /// 일반적으로 턴 시작 시 호출됩니다.
        /// </summary>
        public void RestoreManaFull()
            => Mana.RestoreFull();

        /// <summary>
        /// 최대 마나를 증가시킵니다.
        /// 지정한 최대 한계값을 초과하지 않습니다.
        /// </summary>
        /// <param name="amount"> 증가시킬 마나량 </param>
        /// <param name="maxLimit"> 최대 마나 한계값 </param>
        public void IncreaseMaxMana(int amount, int maxLimit)
            => Mana.IncreaseMax(amount, maxLimit);

        #endregion

        /// <summary>
        /// 덱에서 카드 한 장을 드로우하여 손패에 추가합니다.
        /// 
        /// - todo 덱이 비어 있으면 피로 규칙이 적용됩니다.
        /// - todo 손패가 가득 찼다면 오버드로우 규칙이 적용됩니다.
        /// </summary>
        public void DrawOneCard()
        {
            // 1) 덱 고갈 체크
            if (TcgBattleDataDeck.IsEmpty)
            {
                HandleFatigue();
                return;
            }

            // 2) 덱에서 카드 1장 드로우
            var card = TcgBattleDataDeck.DrawTop();

            // 3) 손패 초과 여부 체크
            if (!Hand.TryAdd(card))
            {
                HandleOverdraw(card);
                return;
            }

            // 4) 드로우 트리거 처리
            TriggerOnDraw(card);
        }

        /// <summary>
        /// 덱이 비어 있는 상태에서 카드를 드로우하려 할 때 호출됩니다.
        /// 피로 카운터를 증가시키고, 일반적으로 영웅에게 누적 피해를 줍니다.
        /// </summary>
        private void HandleFatigue()
        {
            _fatigueCounter++;
            // 예: Hero.TakeDamage(_fatigueCounter);
        }

        /// <summary>
        /// 손패가 가득 찬 상태에서 카드를 드로우했을 때 호출됩니다.
        /// 오버드로우된 카드는 일반적으로 소멸 처리됩니다.
        /// </summary>
        /// <param name="card">
        /// 오버드로우로 인해 손패에 추가되지 못한 카드
        /// </param>
        private void HandleOverdraw(TcgBattleDataCard card)
        {
            // 룰에 따라 묘지로 이동하거나 완전히 소멸
        }

        /// <summary>
        /// 카드 드로우 시 발동되는 효과(키워드, 지속 효과 등)를 처리합니다.
        /// </summary>
        /// <param name="card">
        /// 드로우된 카드
        /// </param>
        private void TriggerOnDraw(TcgBattleDataCard card)
        {
            // Draw 트리거 처리
        }
    }
}
