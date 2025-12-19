using System.Collections.Generic;
using R3;

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

        private readonly BehaviorSubject<int> _currentMana = new(0);
        public Observable<int> CurrentMana => _currentMana; // UI용
        public int CurrentManaValue => _currentMana.Value; // 로직용
        private readonly BehaviorSubject<int> _currentManaMax = new(0);
        public Observable<int> CurrentManaMax => _currentManaMax; // UI용
        public int CurrentManaValueMax => _currentManaMax.Value; // 로직용
        
        // 게임 도중 최대로 얻을 수 있는 마나
        private int _maxMana;

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
        public void InitializeHeroHp(int hp, int maxHp)
        {
            HeroHpMax = maxHp;
            HeroHp    = hp > maxHp ? maxHp : hp;
        }

        public void InitializeMana(int current, int currentMax, int maxMana)
        {
            _currentManaMax.OnNext(currentMax);
            _currentMana.OnNext(current > currentMax ? currentMax : current);
            _maxMana = maxMana;
        }

        #region Card In Hand

        public bool ContainsInHand(TcgBattleDataCard card) => _hand.Contains(card);

        public void AddCardToHand(TcgBattleDataCard card)
        {
            if (card == null) return;
            _hand.Add(card);
        }

        public bool RemoveCardFromHand(TcgBattleDataCard card, out int index)
        {
            index = -1;
            if (card == null) return false;
            index = _hand.IndexOf(card);
            _hand.Remove(card);
            return true;
        }

        /// <summary>
        /// 현재 손패 리스트에서 카드의 인덱스를 반환합니다. (없으면 -1)
        /// 연출/로깅용으로만 사용하고, 로직 분기에는 사용하지 않는 것을 권장합니다.
        /// </summary>
        public int GetHandIndex(TcgBattleDataCard card)
        {
            if (card == null) return -1;
            return _hand.IndexOf(card);
        }

        public TcgBattleDataCard GetDataByIndex(int index)
        {
            return Hand.Count > index ? Hand[index] : null;
        }

        #endregion


        #region Card In Board

        public bool ContainsOnBoard(TcgBattleDataFieldCard unit) => _board.Contains(unit);

        public int AddUnitToBoard(TcgBattleDataFieldCard unit)
        {
            if (unit == null) return -1;
            _board.Add(unit);
            int index = _board.IndexOf(unit);
            unit.Index = index;
            return index;
        }

        public bool RemoveUnitFromBoard(TcgBattleDataFieldCard unit)
        {
            if (unit == null) return false;
            return _board.Remove(unit);
        }

        /// <summary>
        /// 현재 보드 리스트에서 유닛의 인덱스를 반환합니다. (없으면 -1)
        /// 연출/로깅용으로만 사용하고, 로직 분기에는 사용하지 않는 것을 권장합니다.
        /// </summary>
        public int GetBoardIndex(TcgBattleDataFieldCard unit)
        {
            if (unit == null) return -1;
            return _board.IndexOf(unit);
        }

        public TcgBattleDataFieldCard GetFieldDataByIndex(int index)
        {
            return _board.Count > index ? _board[index] : null;
        }

        #endregion

        /// <summary>
        /// 보드에 있는 모든 유닛의 CanAttack 값을 설정합니다.
        /// (턴 종료/턴 시작 등에서 일괄 갱신용)
        /// </summary>
        private void SetAllBoardUnitsCanAttack(bool canAttack)
        {
            foreach (var unit in _board)
            {
                if (unit == null) continue;
                unit.CanAttack = canAttack;
            }
        }
        
        /// <summary>
        /// 턴 종료 시 호출: 보드 전체 유닛을 공격 가능 상태로 만듭니다.
        /// </summary>
        public void SetBoardCardCanAttack(bool value)
        {
            SetAllBoardUnitsCanAttack(value);
        }

        #region Mana, Hp

        public bool TryConsumeMana(int amount)
        {
            if (amount <= 0) return true;
            if (_currentMana.Value < amount) return false;

            var manaValue = _currentMana.Value - amount;
            _currentMana.OnNext(manaValue);
            return true;
        }

        public void RestoreManaFull()
        {
            _currentMana.OnNext(_currentManaMax.Value);
        }

        public void IncreaseMaxMana(int amount, int maxLimit)
        {
            var newMaxMana = _currentManaMax.Value + amount;
            if (newMaxMana > maxLimit)
                newMaxMana = maxLimit;

            _currentManaMax.OnNext(newMaxMana);
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

        #endregion

        public void DrawOneCard()
        {
            // 1) 덱에서 카드 1장 뽑기
            if (TcgBattleDataDeck.IsEmpty)
            {
                HandleFatigue();   // 덱 고갈 시 규칙 처리
                return;
            }

            var card = TcgBattleDataDeck.DrawTop();  // GC 없음, 참조 그대로 반환

            // 2) 핸드가 꽉 찼다면 오버드로우 처리
            if (Hand.Count >= MaxHandSize)
            {
                HandleOverdraw(card);
                return;
            }

            // 3) 핸드에 추가
            AddCardToHand(card);

            // 4) 드로우 트리거 처리 (효과 시스템)
            TriggerOnDraw(card);
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
