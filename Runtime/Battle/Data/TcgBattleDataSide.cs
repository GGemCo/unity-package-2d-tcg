using System.Collections.Generic;
using GGemCo2DCore;

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
        // 현재 턴에서 최대로 사용할 수 있는 마나
        public int CurrentMaxMana { get; private set; }
        // 대결 중 얻을 수 있는 최대 마나
        private int _maxMana;
        private SystemMessageManager _systemMessageManager;

        public TcgBattleDataSide(
            ConfigCommonTcg.TcgPlayerSide side,
            TcgBattleDataDeck<TcgBattleDataCard> tcgBattleDataDeck)
        {
            Side = side;
            TcgBattleDataDeck = tcgBattleDataDeck;
        }

        // ===== 초기값 세팅용 =====
        public void SetHeroHp(int hp, int maxHp)
        {
            HeroHpMax = maxHp;
            HeroHp    = hp > maxHp ? maxHp : hp;
        }

        public void InitializeMana(int current)
        {
            _systemMessageManager = SceneGame.Instance.systemMessageManager;
            
            _maxMana = AddressableLoaderSettingsTcg.Instance.tcgSettings.countMaxManaInBattle;
            CurrentMana = current > _maxMana ? _maxMana : current;
            CurrentMaxMana = CurrentMana;
        }

        // ===== 손패 관리 =====

        public bool ContainsInHand(TcgBattleDataCard tcgBattleDataCard) => _hand.Contains(tcgBattleDataCard);

        public void AddCardToHand(TcgBattleDataCard tcgBattleDataCard)
        {
            if (tcgBattleDataCard == null) return;
            _hand.Add(tcgBattleDataCard);
        }

        public bool RemoveCardFromHand(TcgBattleDataCard tcgBattleDataCard)
        {
            if (tcgBattleDataCard == null) return false;

            if (ContainsInHand(tcgBattleDataCard)) return _hand.Remove(tcgBattleDataCard);
            
            GcLogger.LogWarning("[Battle] RemoveCardFromHand: Hand does not contain card.");
            return false;
        }

        // ===== 필드(보드) 관리 =====

        public bool ContainsOnBoard(TcgBattleDataFieldCard battleData) => _board.Contains(battleData);

        public void AddUnitToBoard(TcgBattleDataFieldCard battleData)
        {
            if (battleData == null) return;
            _board.Add(battleData);
        }

        public bool RemoveUnitFromBoard(TcgBattleDataFieldCard battleData)
        {
            if (battleData == null) return false;
            if (ContainsOnBoard(battleData)) return _board.Remove(battleData);
            
            GcLogger.LogWarning("[Battle] RemoveUnitFromBoard: Board does not contain unit.");
            return false;
        }

        // ===== 마나/HP 증감 =====

        public bool TryConsumeMana(int amount)
        {
            if (amount <= 0) return true;
            if (CurrentMana < amount)
            {
                // todo. localization. 자원 소모 이름 tcg settings에 추가하기
                _systemMessageManager?.ShowMessageError("마나가 부족합니다.");
                GcLogger.LogWarning($"[Battle] ExecutePlayCard: Not enough mana. (Need: {amount}, Have: {CurrentMana})");
                return false;
            }

            CurrentMana -= amount;
            return true;
        }

        public void RestoreManaFull()
        {
            CurrentMana = _maxMana;
            CurrentMaxMana = _maxMana;
        }

        public void IncreaseMaxMana(int amount)
        {
            CurrentMaxMana += amount;
            if (CurrentMaxMana > _maxMana)
                CurrentMaxMana = _maxMana;

            CurrentMana = CurrentMaxMana;
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
