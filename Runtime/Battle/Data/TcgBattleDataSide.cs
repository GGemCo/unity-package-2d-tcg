using System.Collections.Generic;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 전투에서 한 쪽 플레이어의 런타임 상태.
    /// 덱, 손패, 필드, 영웅 상태를 관리합니다.
    /// 외부에서는 IReadOnlyList 로만 접근하고,
    /// 추가/삭제는 제공된 메서드를 통해서만 처리하도록 캡슐화합니다.
    /// </summary>
    public sealed class TcgBattleDataSide
    {
        public ConfigCommonTcg.TcgPlayerSide Side { get; }

        public TcgBattleDataDeck<TcgBattleDataCard> TcgBattleDataDeck { get; }

        // 내부 리스트
        private readonly List<TcgBattleDataCard> _hand = new List<TcgBattleDataCard>(16);
        private readonly List<TcgBattleDataFieldCard> _board = new List<TcgBattleDataFieldCard>(8);

        // 외부에 노출할 읽기 전용 뷰
        public IReadOnlyList<TcgBattleDataCard> Hand => _hand;
        public IReadOnlyList<TcgBattleDataFieldCard> Board => _board;

        public int HeroHp { get; private set; }
        public int HeroHpMax { get; private set; }

        public int CurrentMana { get; private set; }
        public int MaxMana { get; private set; }

        private const int MaxHandSize = 10;
        private int _fatigueCounter = 0;
        
        public TcgBattleDataSide(
            ConfigCommonTcg.TcgPlayerSide side,
            TcgBattleDataDeck<TcgBattleDataCard> deckRuntime)
        {
            Side = side;
            TcgBattleDataDeck = deckRuntime;
        }

        // ===== 초기값 세팅용 =====
        public void SetHeroHp(int hp, int maxHp)
        {
            HeroHpMax = maxHp;
            HeroHp    = hp > maxHp ? maxHp : hp;
        }

        public void SetMana(int current, int max)
        {
            MaxMana     = max;
            CurrentMana = current > max ? max : current;
        }

        // ===== 손패 관리 =====

        public bool ContainsInHand(TcgBattleDataCard card) => _hand.Contains(card);

        public void AddCardToHand(TcgBattleDataCard card)
        {
            if (card == null) return;
            _hand.Add(card);
        }

        public bool RemoveCardFromHand(TcgBattleDataCard card)
        {
            if (card == null) return false;
            return _hand.Remove(card);
        }

        // ===== 필드(보드) 관리 =====

        public bool ContainsOnBoard(TcgBattleDataFieldCard unit) => _board.Contains(unit);

        public void AddUnitToBoard(TcgBattleDataFieldCard unit)
        {
            if (unit == null) return;
            _board.Add(unit);
        }

        public bool RemoveUnitFromBoard(TcgBattleDataFieldCard unit)
        {
            if (unit == null) return false;
            return _board.Remove(unit);
        }

        // ===== 마나/HP 증감 =====

        public bool TryConsumeMana(int amount)
        {
            if (amount <= 0) return true;
            if (CurrentMana < amount) return false;

            CurrentMana -= amount;
            return true;
        }

        public void RestoreManaFull()
        {
            CurrentMana = MaxMana;
        }

        public void IncreaseMaxMana(int amount, int maxLimit)
        {
            MaxMana += amount;
            if (MaxMana > maxLimit)
                MaxMana = maxLimit;

            CurrentMana = MaxMana;
        }

        public void TakeHeroDamage(int amount)
        {
            if (amount <= 0) return;
            HeroHp -= amount;
            if (HeroHp < 0) HeroHp = 0;
        }

        public void HealHero(int amount)
        {
            if (amount <= 0) return;
            HeroHp += amount;
            if (HeroHp > HeroHpMax) HeroHp = HeroHpMax;
        }
        public TcgBattleDataCard DrawOneCard()
        {
            // 1) 덱에서 카드 1장 뽑기
            if (TcgBattleDataDeck.IsEmpty)
            {
                HandleFatigue();   // 덱 고갈 시 규칙 처리
                return null;
            }

            var card = TcgBattleDataDeck.DrawTop();  // GC 없음, 참조 그대로 반환

            // 2) 핸드가 꽉 찼다면 오버드로우 처리
            if (Hand.Count >= MaxHandSize)
            {
                HandleOverdraw(card);
                return null;
            }

            // 3) 핸드에 추가
            AddCardToHand(card);

            // 4) 드로우 트리거 처리 (효과 시스템)
            TriggerOnDraw(card);

            return card;
        }
        
        private void HandleFatigue()
        {
            _fatigueCounter++;
            HeroHp -= _fatigueCounter;
        }

        private void HandleOverdraw(TcgBattleDataCard card)
        {
            // 오버드로우된 카드는 소멸
            // 덱에서 뽑혔으므로 묘지로 이동시키지 않음 (룰에 따라 변경 가능)
        }

        private void TriggerOnDraw(TcgBattleDataCard card)
        {
            // 키워드 or 지속효과 처리
        }

    }
}
