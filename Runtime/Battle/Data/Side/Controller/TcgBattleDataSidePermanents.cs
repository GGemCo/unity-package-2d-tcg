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
    }

    /// <summary>
    /// Permanent 카드의 런타임 인스턴스.
    /// - 스택, 마지막 발동 턴, 상세 테이블 정의 등을 보관합니다.
    /// </summary>
    public sealed class TcgBattlePermanentInstance
    {
        public TcgBattleDataCard Card { get; }
        public StruckTableTcgCardPermanent Definition { get; }

        /// <summary>현재 스택 수.</summary>
        public int Stacks { get; set; }

        /// <summary>마지막으로 발동된 턴 카운트.</summary>
        public int LastResolvedTurn { get; set; }

        public TcgBattlePermanentInstance(TcgBattleDataCard card, StruckTableTcgCardPermanent definition)
        {
            Card = card ?? throw new ArgumentNullException(nameof(card));
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Stacks = 1;
            LastResolvedTurn = -999;
        }
    }
}
