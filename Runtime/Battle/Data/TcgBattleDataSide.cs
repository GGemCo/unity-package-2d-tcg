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
    }
}
