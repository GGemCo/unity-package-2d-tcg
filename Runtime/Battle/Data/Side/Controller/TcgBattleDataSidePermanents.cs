using System;
using System.Collections.Generic;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 한쪽 플레이어의 Permanent(지속 카드) 영역을 관리하는 런타임 컨테이너입니다.
    /// Permanent는 필드(유닛)와 별도 영역에 존재하며, 턴 시작/종료 등 특정 트리거에 반응해 Ability를 반복 실행할 수 있습니다.
    /// </summary>
    /// <remarks>
    /// - 트리거 판정/Ability 실행/만료 처리(Expired)는 BattleManager 또는 Permanent 처리 시스템에서 수행하는 것을 전제로 합니다.
    /// - 이 클래스는 보관/추가/제거 및 만료 이벤트 알림을 제공합니다.
    /// </remarks>
    public sealed class TcgBattleDataSidePermanents
    {
        private readonly List<TcgBattlePermanentInstance> _items = new List<TcgBattlePermanentInstance>(8);

        /// <summary>
        /// Permanent가 Lifetime 만료로 제거될 때 호출되는 이벤트입니다.
        /// UI/로그 시스템이 구독하여 연출 또는 메시지를 표시할 수 있습니다.
        /// </summary>
        public event Action<TcgBattlePermanentInstance> PermanentExpired;

        /// <summary>
        /// 등록된 Permanent 인스턴스 목록(읽기 전용)입니다.
        /// </summary>
        public IReadOnlyList<TcgBattlePermanentInstance> Items => _items;

        /// <summary>
        /// 등록된 Permanent 개수입니다.
        /// </summary>
        public int Count => _items.Count;

        /// <summary>
        /// 모든 Permanent를 제거합니다.
        /// </summary>
        public void Clear() => _items.Clear();

        /// <summary>
        /// Permanent 인스턴스를 영역에 추가합니다.
        /// </summary>
        /// <param name="inst">추가할 Permanent 인스턴스입니다.</param>
        /// <remarks>
        /// null이면 아무 작업도 하지 않습니다.
        /// </remarks>
        public void Add(TcgBattlePermanentInstance inst)
        {
            if (inst == null) return;
            _items.Add(inst);
        }

        /// <summary>
        /// Permanent 인스턴스를 영역에서 제거합니다.
        /// </summary>
        /// <param name="inst">제거할 Permanent 인스턴스입니다.</param>
        /// <returns>제거에 성공하면 true, 존재하지 않거나 null이면 false입니다.</returns>
        public bool Remove(TcgBattlePermanentInstance inst)
        {
            if (inst == null) return false;
            return _items.Remove(inst);
        }

        /// <summary>
        /// 특정 Permanent가 만료(Expired)되었음을 구독자에게 알립니다.
        /// </summary>
        /// <param name="inst">만료된 Permanent 인스턴스입니다.</param>
        /// <remarks>
        /// 내부 처리 로직(예: Lifetime 전략)에서 호출하기 위한 용도이며, 외부에는 노출하지 않습니다.
        /// </remarks>
        internal void NotifyExpired(TcgBattlePermanentInstance inst)
        {
            PermanentExpired?.Invoke(inst);
        }
    }

    /// <summary>
    /// Permanent 카드 한 장의 전투 런타임 인스턴스입니다.
    /// 카드 런타임 정보, 상세 테이블 정의, 스택/발동 상태, 만료(Lifetime) 전략을 함께 보관합니다.
    /// </summary>
    public sealed class TcgBattlePermanentInstance
    {
        /// <summary>
        /// 이 Permanent가 발동/적용된 공격자(또는 시전자) 기준 존(Zone)입니다.
        /// </summary>
        public ConfigCommonTcg.TcgZone AttackerZone { get; }

        /// <summary>
        /// 원본 카드 런타임(손패 카드) 참조입니다.
        /// </summary>
        public TcgBattleDataCardInHand CardInHand { get; }

        /// <summary>
        /// Permanent 카드의 상세 테이블 정의 데이터입니다.
        /// </summary>
        public StruckTableTcgCardPermanent Definition { get; }

        /// <summary>
        /// 현재 스택 수입니다.
        /// </summary>
        public int Stacks { get; set; }

        /// <summary>
        /// 마지막으로 발동(Resolve)된 턴 카운트입니다.
        /// 중복 발동 방지/쿨타임 처리 등에 사용할 수 있습니다.
        /// </summary>
        public int LastResolvedTurn { get; set; }

        /// <summary>
        /// Permanent의 Lifetime(만료 정책) 전략입니다.
        /// </summary>
        public ITcgPermanentLifetimeStrategy Lifetime { get; }

        /// <summary>
        /// Lifetime 전략 기준으로 만료되었는지 여부입니다.
        /// </summary>
        public bool IsExpired => Lifetime != null && Lifetime.IsExpired;

        /// <summary>
        /// Permanent 런타임 인스턴스를 생성합니다.
        /// </summary>
        /// <param name="cardInHand">원본 카드 런타임입니다.</param>
        /// <param name="definition">Permanent 카드의 상세 정의 데이터입니다.</param>
        /// <param name="attackerZone">발동/적용의 기준이 되는 공격자(시전자) 존입니다.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="cardInHand"/> 또는 <paramref name="definition"/>이 null이면 발생합니다.
        /// </exception>
        public TcgBattlePermanentInstance(
            TcgBattleDataCardInHand cardInHand,
            StruckTableTcgCardPermanent definition,
            ConfigCommonTcg.TcgZone attackerZone)
        {
            CardInHand = cardInHand ?? throw new ArgumentNullException(nameof(cardInHand));
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));

            Stacks = 1;
            LastResolvedTurn = -999;
            AttackerZone = attackerZone;

            Lifetime = TcgPermanentLifetimeStrategyFactory.Build(definition);
        }
    }
}
