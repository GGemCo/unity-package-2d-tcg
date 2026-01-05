using System;
using System.Collections.Generic;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 한 쪽 플레이어의 Permanent(지속 카드) 영역을 관리합니다.
    /// Permanent는 필드(유닛)와 별도이며, 턴 시작/종료 등 특정 트리거에 반응하여 Ability를 반복 실행할 수 있습니다.
    /// </summary>
    public sealed class TcgBattleDataSidePermanents
    {
        private readonly List<TcgBattlePermanentInstance> _items = new List<TcgBattlePermanentInstance>(8);

        /// <summary>
        /// Permanent가 Lifetime 만료로 제거될 때 호출됩니다.
        /// UI 또는 로그 시스템에서 구독하여 연출/메시지를 추가할 수 있습니다.
        /// </summary>
        public event Action<TcgBattlePermanentInstance> PermanentExpired;

        public IReadOnlyList<TcgBattlePermanentInstance> Items => _items;

        public int Count => _items.Count;

        public void Clear() => _items.Clear();

        public void Add(TcgBattlePermanentInstance inst)
        {
            if (inst == null) return;
            _items.Add(inst);
        }

        public bool Remove(TcgBattlePermanentInstance inst)
        {
            if (inst == null) return false;
            return _items.Remove(inst);
        }

        internal void NotifyExpired(TcgBattlePermanentInstance inst)
        {
            PermanentExpired?.Invoke(inst);
        }
    }

    /// <summary>
    /// Permanent 카드의 런타임 인스턴스.
    /// - 스택, 마지막 발동 턴, 상세 테이블 정의 등을 보관합니다.
    /// </summary>
    public sealed class TcgBattlePermanentInstance
    {
        public ConfigCommonTcg.TcgZone AttackerZone { get; }
        public TcgBattleDataCardInHand CardInHand { get; }
        public StruckTableTcgCardPermanent Definition { get; }

        /// <summary>현재 스택 수.</summary>
        public int Stacks { get; set; }

        /// <summary>마지막으로 발동된 턴 카운트.</summary>
        public int LastResolvedTurn { get; set; }

        /// <summary>Permanent Lifetime 전략(만료 정책).</summary>
        public ITcgPermanentLifetimeStrategy Lifetime { get; }

        public bool IsExpired => Lifetime != null && Lifetime.IsExpired;

        public TcgBattlePermanentInstance(TcgBattleDataCardInHand cardInHand, StruckTableTcgCardPermanent definition, ConfigCommonTcg.TcgZone attackerZone)
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
