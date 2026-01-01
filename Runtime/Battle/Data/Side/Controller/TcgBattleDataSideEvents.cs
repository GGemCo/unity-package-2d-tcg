using System;
using System.Collections.Generic;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 한 쪽 플레이어의 Event(이벤트/시크릿) 영역을 관리합니다.
    /// Event는 특정 TriggerType이 만족될 때 Ability를 실행할 수 있습니다.
    /// </summary>
    public sealed class TcgBattleDataSideEvents
    {
        private readonly List<TcgBattleEventInstance> _items = new List<TcgBattleEventInstance>(8);

        public IReadOnlyList<TcgBattleEventInstance> Items => _items;

        public int Count => _items.Count;

        public void Clear() => _items.Clear();

        public void Add(TcgBattleEventInstance inst)
        {
            if (inst == null) return;
            _items.Add(inst);
        }

        public bool Remove(TcgBattleEventInstance inst)
        {
            if (inst == null) return false;
            return _items.Remove(inst);
        }
    }

    /// <summary>
    /// Event 카드의 런타임 인스턴스.
    /// </summary>
    public sealed class TcgBattleEventInstance
    {
        public TcgBattleDataCardInHand CardInHand { get; }
        public StruckTableTcgCardEvent Definition { get; }

        public TcgBattleEventInstance(TcgBattleDataCardInHand cardInHand, StruckTableTcgCardEvent definition)
        {
            CardInHand = cardInHand ?? throw new ArgumentNullException(nameof(cardInHand));
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        }
    }
}
