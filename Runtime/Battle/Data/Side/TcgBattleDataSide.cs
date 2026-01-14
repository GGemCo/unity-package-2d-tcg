using System;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 전투에서 한 쪽 플레이어(Player/Enemy)의 런타임 상태를 관리하는 도메인 루트(Aggregate Root)입니다.
    /// 덱/손패/필드/영웅/마나 및 지속 효과(Permanent), 이벤트(Event) 영역을 조합해 한 진영의 전투 상태를 표현합니다.
    /// </summary>
    /// <remarks>
    /// - UI, MonoBehaviour, Scene 등에 의존하지 않는 순수 전투 데이터 모델입니다.
    /// - 외부에서는 가능한 한 읽기 전용으로 접근하고, 상태 변경은 제공된 메서드를 통해서만 일어나도록 설계합니다.
    /// - 전투 규칙의 실제 적용(공격 판정, 능력 실행, 데미지 처리 등)은 BattleManager/이펙트 시스템에서 담당합니다.
    /// </remarks>
    public sealed class TcgBattleDataSide
    {
        /// <summary>
        /// 이 전투 데이터가 속한 플레이어 진영입니다. (Player/Enemy)
        /// </summary>
        public ConfigCommonTcg.TcgPlayerSide Side { get; }

        /// <summary>
        /// 해당 플레이어의 전투용 덱 데이터입니다.
        /// 카드 드로우, 덱 고갈 여부 확인 등에 사용됩니다.
        /// </summary>
        public TcgBattleDataDeck<TcgBattleDataCardInHand> TcgBattleDataDeck { get; }

        /// <summary>
        /// 손패(Hand) 영역을 관리하는 객체입니다.
        /// 카드 추가/제거 및 최대 손패 크기 제한을 담당합니다.
        /// </summary>
        public TcgBattleDataSideHand Hand { get; }

        /// <summary>
        /// 필드(Field) 영역을 관리하는 객체입니다.
        /// 유닛 추가/제거, 조회, 공격 가능 상태 일괄 갱신 등을 담당합니다.
        /// </summary>
        public TcgBattleDataSideField Field { get; }

        /// <summary>
        /// 현재 마나/최대 마나를 관리하는 객체입니다.
        /// </summary>
        public TcgBattleDataSideMana Mana { get; }

        /// <summary>
        /// Permanent(지속 카드) 영역을 관리합니다.
        /// </summary>
        public TcgBattleDataSidePermanents Permanents { get; }

        /// <summary>
        /// Event(이벤트/시크릿) 영역을 관리합니다.
        /// </summary>
        public TcgBattleDataSideEvents Events { get; }

        /// <summary>
        /// 카드가 손패로 드로우되었을 때 발생하는 도메인 이벤트입니다.
        /// 턴 시작 드로우/효과 드로우 등 모든 드로우 경로에서 호출됩니다.
        /// </summary>
        public event Action<TcgBattleDataCardInHand> CardDrawn;

        /// <summary>
        /// 게임 도중 이 플레이어가 가질 수 있는 최대 마나 한계값입니다. (예: 10)
        /// </summary>
        private int _maxMana;

        /// <summary>
        /// 손패의 최대 카드 수입니다.
        /// 이 값을 초과하여 드로우할 경우 오버드로우(Overdraw) 규칙이 적용됩니다.
        /// </summary>
        private const int MaxHandSize = 10;

        /// <summary>
        /// 덱이 고갈된 상태에서 카드를 드로우할 때 증가하는 피로(Fatigue) 카운터입니다.
        /// 일반적으로 영웅에게 누적 피해를 주는 데 사용됩니다.
        /// </summary>
        private int _fatigueCounter = 0;

        /// <summary>
        /// 한쪽 플레이어의 전투 런타임 데이터를 생성합니다.
        /// </summary>
        /// <param name="side">이 데이터가 속한 플레이어 진영(Player/Enemy)입니다.</param>
        /// <param name="deckRuntime">전투에 사용할 덱 런타임 데이터입니다.</param>
        /// <param name="currentMana">전투 시작 시 현재 마나 값입니다.</param>
        /// <param name="currentManaMax">전투 시작 시 최대 마나 값입니다.</param>
        /// <param name="maxMana">전투 중 도달할 수 있는 최대 마나 상한값입니다.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="deckRuntime"/>가 null인 경우(계약을 강제하고 싶을 때) 발생할 수 있습니다.
        /// </exception>
        /// <remarks>
        /// 영웅 카드는 <see cref="TcgBattleDataDeck{TCard}.HeroCard"/>를 기반으로 초기화합니다.
        /// </remarks>
        public TcgBattleDataSide(
            ConfigCommonTcg.TcgPlayerSide side,
            TcgBattleDataDeck<TcgBattleDataCardInHand> deckRuntime,
            int currentMana,
            int currentManaMax,
            int maxMana)
        {
            Side = side;
            TcgBattleDataDeck = deckRuntime;

            Hand = new TcgBattleDataSideHand(initialCapacity: MaxHandSize, maxSize: MaxHandSize);
            Field = new TcgBattleDataSideField(initialCapacity: MaxHandSize);

            InitializeHeroCard(TcgBattleDataDeck.HeroCard);

            // 마나는 현재 값과 최대값으로 초기화되며,
            // 전투 중 IncreaseMaxMana 를 통해 단계적으로 증가합니다.
            Mana = new TcgBattleDataSideMana(currentMana, currentManaMax);

            Permanents = new TcgBattleDataSidePermanents();
            Events = new TcgBattleDataSideEvents();

            _maxMana = maxMana;
        }

        #region Hand

        /// <summary>
        /// 손패 인덱스로 손패 카드 데이터를 조회합니다.
        /// </summary>
        /// <param name="index">손패 인덱스입니다.</param>
        /// <returns>해당 인덱스의 카드이며, 유효하지 않으면 null입니다.</returns>
        public TcgBattleDataCardInHand GetBattleDataCardInHandByIndex(int index)
        {
            return Hand.GetByIndex(index);
        }

        #endregion

        #region Field

        /// <summary>
        /// 필드에 배치된 모든 유닛의 공격 가능 여부를 일괄 설정합니다.
        /// 주로 턴 시작/종료 시점에 사용됩니다.
        /// </summary>
        /// <param name="value">true이면 공격 가능, false이면 공격 불가입니다.</param>
        public void SetFieldCardCanAttack(bool value)
            => Field.SetAllCanAttack(value);

        /// <summary>
        /// 필드 인덱스로 필드 카드(유닛)를 조회합니다.
        /// </summary>
        /// <param name="index">필드 인덱스입니다.</param>
        /// <param name="includeHero">true이면 영웅 슬롯도 조회 대상으로 포함합니다.</param>
        /// <returns>해당 인덱스의 유닛이며, 유효하지 않으면 null입니다.</returns>
        public TcgBattleDataCardInField GetBattleDataCardInFieldByIndex(int index, bool includeHero = false)
        {
            return Field.GetByIndex(index, includeHero);
        }

        /// <summary>
        /// 지정한 유닛이 현재 필드(일반 슬롯)에 존재하는지 확인합니다.
        /// </summary>
        /// <param name="battleDataCardInField">확인할 유닛입니다.</param>
        /// <returns>존재하면 true, 아니면 false입니다.</returns>
        public bool ContainsInField(TcgBattleDataCardInField battleDataCardInField)
        {
            return Field.Contains(battleDataCardInField);
        }

        #endregion

        #region Hero

        /// <summary>
        /// 이 플레이어의 영웅 카드를 초기화합니다.
        /// 일반적으로 전투 시작 시 한 번 호출됩니다.
        /// </summary>
        /// <param name="cardInHandHero">영웅으로 사용할 손패 카드 런타임입니다.</param>
        /// <remarks>
        /// - 손패(Hero 참조)에 영웅 카드를 등록합니다.
        /// - 필드에는 영웅 슬롯(<see cref="ConfigCommonTcg.IndexHeroSlot"/>)에 해당하는 필드 유닛을 생성하여 등록합니다.
        /// </remarks>
        private void InitializeHeroCard(TcgBattleDataCardInHand cardInHandHero)
        {
            if (cardInHandHero == null) return;

            Hand.AddHero(cardInHandHero);

            var battleDataFieldCard = TcgBattleDataCardFactory.CreateBattleDataFieldCard(Side, cardInHandHero);
            battleDataFieldCard.SetIndex(ConfigCommonTcg.IndexHeroSlot);

            Field.AddHero(battleDataFieldCard);
        }

        /// <summary>
        /// 영웅 슬롯 인덱스로 영웅 유닛을 조회합니다.
        /// </summary>
        /// <param name="index">조회할 인덱스입니다. 영웅 슬롯이 아니면 null을 반환합니다.</param>
        /// <returns>영웅 슬롯이면 영웅 유닛, 아니면 null입니다.</returns>
        public TcgBattleDataCardInField GetHeroBattleDataCardInFieldByIndex(int index)
        {
            if (index != ConfigCommonTcg.IndexHeroSlot) return null;
            return Field.Hero;
        }

        /// <summary>
        /// 지정한 유닛이 이 플레이어의 영웅 유닛인지 확인합니다.
        /// </summary>
        /// <param name="target">확인할 대상 유닛입니다.</param>
        /// <returns>영웅이면 true, 아니면 false입니다.</returns>
        public bool ContainsInFieldHero(TcgBattleDataCardInField target)
        {
            return Field.Hero == target;
        }

        #endregion

        #region Mana

        /// <summary>
        /// 지정한 마나를 소모할 수 있는지 확인하고, 가능하다면 소모합니다.
        /// </summary>
        /// <param name="amount">소모할 마나량입니다.</param>
        /// <returns>소모에 성공하면 true, 마나가 부족하면 false입니다.</returns>
        public bool TryConsumeMana(int amount)
            => Mana.TryConsume(amount);

        /// <summary>
        /// 현재 마나를 최대 마나까지 전량 회복합니다.
        /// 일반적으로 턴 시작 처리에서 호출됩니다.
        /// </summary>
        public void RestoreManaFull()
            => Mana.RestoreFull();

        /// <summary>
        /// 최대 마나를 증가시킵니다.
        /// 지정된 상한값을 초과하지 않도록 합니다.
        /// </summary>
        /// <param name="amount">증가시킬 마나량입니다.</param>
        /// <param name="maxLimit">최대 마나 상한값입니다.</param>
        public void IncreaseMaxMana(int amount, int maxLimit)
            => Mana.IncreaseMax(amount, maxLimit);

        #endregion

        /// <summary>
        /// 덱에서 카드 한 장을 드로우하여 손패에 추가합니다.
        /// </summary>
        /// <remarks>
        /// - 덱이 비어 있으면 피로(Fatigue) 규칙이 적용됩니다.
        /// - 손패가 가득 찼다면 오버드로우(Overdraw) 규칙이 적용됩니다.
        /// - 정상적으로 손패에 추가되면 <see cref="CardDrawn"/> 이벤트가 발생합니다.
        /// </remarks>
        public TcgDrawOneCardResult DrawOneCard()
        {
            // 1) 덱 고갈 체크
            if (TcgBattleDataDeck.IsEmpty)
            {
                HandleFatigue();
                return TcgDrawOneCardResult.CreateFatigue();
            }

            // 2) 덱에서 카드 1장 드로우
            var card = TcgBattleDataDeck.DrawTop();

            // 3) 손패 초과 여부 체크
            if (!Hand.TryAdd(card, out int handIndex))
            {
                HandleOverdraw(card);
                return TcgDrawOneCardResult.CreateOverdraw(card);
            }

            // 4) 드로우 트리거(도메인 이벤트)
            CardDrawn?.Invoke(card);

            return TcgDrawOneCardResult.CreateAdded(card, handIndex);
        }

        /// <summary>
        /// 덱이 고갈된 상태에서 드로우를 시도했을 때 호출됩니다.
        /// 피로 카운터를 증가시키며, 일반적으로 영웅에게 누적 피해를 주는 규칙에 사용됩니다.
        /// </summary>
        private void HandleFatigue()
        {
            _fatigueCounter++;
            // 예: Field.Hero.ApplyDamage(_fatigueCounter);
        }

        /// <summary>
        /// 손패가 가득 찬 상태에서 드로우했을 때 호출됩니다.
        /// 손패에 추가되지 못한 카드는 오버드로우 규칙에 따라 처리됩니다(소멸/묘지 이동 등).
        /// </summary>
        /// <param name="cardInHand">오버드로우로 인해 손패에 추가되지 못한 카드입니다.</param>
        private void HandleOverdraw(TcgBattleDataCardInHand cardInHand)
        {
            // 룰에 따라 묘지로 이동하거나 완전히 소멸
        }
    }
}
