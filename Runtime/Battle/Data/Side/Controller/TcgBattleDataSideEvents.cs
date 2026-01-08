using System;
using System.Collections.Generic;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 한쪽 플레이어가 보유한 Event(이벤트/시크릿) 카드 영역의 런타임 컨테이너입니다.
    /// 특정 Trigger 조건이 만족되면, 등록된 Event의 Ability가 실행될 수 있습니다.
    /// </summary>
    /// <remarks>
    /// - 필드 유닛과 달리 Event는 공개/비공개(시크릿) 상태로 유지될 수 있으며,
    ///   트리거 충족 시 자동으로 소모/제거되는 흐름을 전제로 합니다.
    /// - 실제 트리거 판정 및 Ability 실행은 BattleManager/이벤트 처리 시스템에서 담당합니다.
    /// </remarks>
    public sealed class TcgBattleDataSideEvents
    {
        /// <summary>
        /// 현재 플레이어가 보유한 Event 인스턴스 목록입니다.
        /// </summary>
        private readonly List<TcgBattleEventInstance> _items = new List<TcgBattleEventInstance>(8);

        /// <summary>
        /// Event 인스턴스 목록(읽기 전용)입니다.
        /// </summary>
        public IReadOnlyList<TcgBattleEventInstance> Items => _items;

        /// <summary>
        /// 등록된 Event 개수입니다.
        /// </summary>
        public int Count => _items.Count;

        /// <summary>
        /// 모든 Event를 제거합니다.
        /// </summary>
        public void Clear() => _items.Clear();

        /// <summary>
        /// Event 인스턴스를 영역에 추가합니다.
        /// </summary>
        /// <param name="inst">추가할 Event 인스턴스입니다.</param>
        /// <remarks>
        /// null이 전달되면 아무 작업도 하지 않습니다.
        /// </remarks>
        public void Add(TcgBattleEventInstance inst)
        {
            if (inst == null) return;
            _items.Add(inst);
        }

        /// <summary>
        /// 지정한 Event 인스턴스를 영역에서 제거합니다.
        /// </summary>
        /// <param name="inst">제거할 Event 인스턴스입니다.</param>
        /// <returns>제거에 성공하면 true, 존재하지 않거나 null이면 false입니다.</returns>
        public bool Remove(TcgBattleEventInstance inst)
        {
            if (inst == null) return false;
            return _items.Remove(inst);
        }
    }

    /// <summary>
    /// Event 카드 한 장의 전투 런타임 인스턴스입니다.
    /// 카드 런타임 정보와 Event 전용 테이블 정의를 함께 보관합니다.
    /// </summary>
    /// <remarks>
    /// - 이 인스턴스는 필드에 직접 배치되지 않으며, SideEvents 영역에 등록됩니다.
    /// - Trigger 조건이 만족되면 <see cref="Definition"/>에 정의된 Ability가 실행됩니다.
    /// </remarks>
    public sealed class TcgBattleEventInstance
    {
        /// <summary>
        /// 원본 카드 런타임(손패 카드) 참조입니다.
        /// </summary>
        public TcgBattleDataCardInHand CardInHand { get; }

        /// <summary>
        /// Event 카드의 테이블 정의 데이터입니다.
        /// Trigger 조건, Ability 정보 등을 포함합니다.
        /// </summary>
        public StruckTableTcgCardEvent Definition { get; }

        /// <summary>
        /// Event 카드 런타임 인스턴스를 생성합니다.
        /// </summary>
        /// <param name="cardInHand">원본 카드 런타임입니다.</param>
        /// <param name="definition">Event 카드의 테이블 정의 데이터입니다.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="cardInHand"/> 또는 <paramref name="definition"/>이 null인 경우 발생합니다.
        /// </exception>
        public TcgBattleEventInstance(
            TcgBattleDataCardInHand cardInHand,
            StruckTableTcgCardEvent definition)
        {
            CardInHand = cardInHand ?? throw new ArgumentNullException(nameof(cardInHand));
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        }
    }
}
