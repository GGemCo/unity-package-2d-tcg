using System;
using System.Collections.Generic;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 전투 중 한 쪽 플레이어의 손패(Hand) 영역을 관리하는 도메인 클래스입니다.
    /// 카드 추가/제거, 최대 손패 크기 제한을 담당하며,
    /// 외부에서는 읽기 전용 리스트로만 접근하도록 캡슐화합니다.
    /// </summary>
    public sealed class TcgBattleDataSideHand
    {
        /// <summary>
        /// 손패에 보관된 카드의 내부 리스트입니다.
        /// </summary>
        private readonly List<TcgBattleDataCard> _cards;

        /// <summary>
        /// 외부에 노출되는 손패 카드 목록의 읽기 전용 뷰입니다.
        /// </summary>
        public IReadOnlyList<TcgBattleDataCard> Cards => _cards;

        /// <summary>
        /// 손패에 가질 수 있는 최대 카드 개수입니다.
        /// </summary>
        public int MaxSize { get; }

        /// <summary>
        /// 손패 데이터를 초기화합니다.
        /// </summary>
        /// <param name="initialCapacity">내부 리스트의 초기 용량입니다.</param>
        /// <param name="maxSize">손패에 허용되는 최대 카드 수입니다.</param>
        public TcgBattleDataSideHand(int initialCapacity = 16, int maxSize = 10)
        {
            _cards = new List<TcgBattleDataCard>(Math.Max(1, initialCapacity));
            MaxSize = Math.Max(1, maxSize);
        }

        /// <summary>
        /// 지정한 카드가 현재 손패에 포함되어 있는지 확인합니다.
        /// </summary>
        /// <param name="card">확인할 카드 데이터입니다.</param>
        /// <returns>손패에 포함되어 있으면 true, 아니면 false를 반환합니다.</returns>
        public bool Contains(TcgBattleDataCard card)
            => card != null && _cards.Contains(card);

        /// <summary>
        /// 손패 내에서 지정한 카드의 인덱스를 반환합니다.
        /// </summary>
        /// <param name="card">인덱스를 조회할 카드 데이터입니다.</param>
        /// <returns>카드가 존재하면 인덱스, 없으면 -1을 반환합니다.</returns>
        public int IndexOf(TcgBattleDataCard card)
            => card == null ? -1 : _cards.IndexOf(card);

        /// <summary>
        /// 손패 인덱스를 기준으로 카드 데이터를 반환합니다.
        /// </summary>
        /// <param name="index">조회할 손패 인덱스입니다.</param>
        /// <returns>유효한 인덱스이면 카드 데이터, 아니면 null을 반환합니다.</returns>
        public TcgBattleDataCard GetDataByIndex(int index)
            => (uint)index < (uint)_cards.Count ? _cards[index] : null;

        /// <summary>
        /// 현재 손패에 카드를 추가할 수 있는지 여부를 반환합니다.
        /// </summary>
        public bool CanAdd => _cards.Count < MaxSize;

        /// <summary>
        /// 손패에 카드를 추가합니다.
        /// </summary>
        /// <param name="card">손패에 추가할 카드 데이터입니다.</param>
        /// <returns>추가에 성공하면 true, 실패하면 false를 반환합니다.</returns>
        public bool TryAdd(TcgBattleDataCard card)
        {
            if (card == null) return false;
            if (!CanAdd) return false;

            _cards.Add(card);
            return true;
        }

        /// <summary>
        /// 손패에서 카드를 제거합니다.
        /// </summary>
        /// <param name="card">제거할 카드 데이터입니다.</param>
        /// <param name="index">제거된 카드의 기존 손패 인덱스입니다.</param>
        /// <returns>제거에 성공하면 true, 실패하면 false를 반환합니다.</returns>
        public bool TryRemove(TcgBattleDataCard card, out int index)
        {
            index = -1;
            if (card == null) return false;

            index = _cards.IndexOf(card);
            if (index < 0) return false;

            _cards.RemoveAt(index);
            return true;
        }

        /// <summary>
        /// 현재 손패에 있는 카드 개수를 반환합니다.
        /// </summary>
        /// <returns>손패에 존재하는 카드 수입니다.</returns>
        public int GetCount()
        {
            return _cards.Count;
        }
    }
}
