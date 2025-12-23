using System;
using System.Collections.Generic;
using R3;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 전투 중 한 쪽 플레이어의 필드(Board) 영역을 관리하는 도메인 클래스입니다.
    /// 필드에 배치된 카드의 추가/제거, 인덱스 관리, 공격 가능 상태 갱신을 담당합니다.
    /// </summary>
    public sealed class TcgBattleDataSideBoard
    {
        /// <summary>
        /// 필드에 배치된 카드의 내부 리스트입니다.
        /// </summary>
        private List<TcgBattleDataFieldCard> _cards;

        /// <summary>
        /// 외부에 노출되는 필드 카드 목록의 읽기 전용 뷰입니다.
        /// </summary>
        public IReadOnlyList<TcgBattleDataFieldCard> Cards => _cards;

        /// <summary>
        /// 필드 카드 관리 객체를 초기화합니다.
        /// </summary>
        /// <param name="initialCapacity">내부 리스트의 초기 용량입니다.</param>
        public TcgBattleDataSideBoard(int initialCapacity = 8)
        {
            _cards = new List<TcgBattleDataFieldCard>(Math.Max(1, initialCapacity));
        }

        /// <summary>
        /// 지정한 카드가 현재 필드에 존재하는지 확인합니다.
        /// </summary>
        /// <param name="unit">존재 여부를 확인할 필드 카드입니다.</param>
        /// <returns>필드에 존재하면 true, 아니면 false를 반환합니다.</returns>
        public bool Contains(TcgBattleDataFieldCard unit)
            => unit != null && _cards.Contains(unit);

        /// <summary>
        /// 필드 내에서 지정한 카드의 인덱스를 반환합니다.
        /// </summary>
        /// <param name="unit">인덱스를 조회할 필드 카드입니다.</param>
        /// <returns>카드가 존재하면 인덱스, 없으면 -1을 반환합니다.</returns>
        public int IndexOf(TcgBattleDataFieldCard unit)
            => unit == null ? -1 : _cards.IndexOf(unit);

        /// <summary>
        /// 현재 필드에 배치된 카드의 개수를 반환합니다.
        /// </summary>
        public int Count => _cards.Count;

        /// <summary>
        /// 필드 인덱스를 기준으로 카드 데이터를 반환합니다.
        /// </summary>
        /// <param name="index">조회할 필드 인덱스입니다.</param>
        /// <returns>유효한 인덱스이면 카드 데이터, 아니면 null을 반환합니다.</returns>
        public TcgBattleDataFieldCard GetFieldDataByIndex(int index)
            => index < _cards.Count ? _cards[index] : null;

        /// <summary>
        /// 필드에 카드를 추가하고 인덱스를 설정합니다.
        /// </summary>
        /// <param name="unit">필드에 추가할 카드입니다.</param>
        /// <returns>추가된 카드의 필드 인덱스, 실패 시 -1을 반환합니다.</returns>
        public int Add(TcgBattleDataFieldCard unit)
        {
            if (unit == null) return -1;

            _cards.Add(unit);
            var index = _cards.Count - 1;
            // 연출이 끝난 후 Refresh All 에서 바꿔준다. 
            // unit.Index = index;

            return index;
        }

        /// <summary>
        /// 필드에서 카드를 제거하고 남은 카드들의 인덱스를 재정렬합니다.
        /// </summary>
        /// <param name="unit">필드에서 제거할 카드입니다.</param>
        /// <returns>제거에 성공하면 true, 실패하면 false를 반환합니다.</returns>
        public bool Remove(TcgBattleDataFieldCard unit)
        {
            if (unit == null) return false;

            var index = _cards.IndexOf(unit);
            if (index < 0) return false;

            _cards.RemoveAt(index);

            // 제거 이후 남아 있는 카드들의 Index 값을 재정렬합니다.
            // 연출이 끝난 후 Refresh All 에서 바꿔준다. 
            // for (int i = index; i < _cards.Count; i++)
            // {
            //     var u = _cards[i];
            //     if (u != null) u.Index = i;
            // }

            return true;
        }

        /// <summary>
        /// 필드에 있는 모든 카드의 공격 가능 여부를 일괄 설정합니다.
        /// </summary>
        /// <param name="canAttack">true이면 공격 가능, false이면 공격 불가입니다.</param>
        public void SetAllCanAttack(bool canAttack)
        {
            for (int i = 0; i < _cards.Count; i++)
            {
                var u = _cards[i];
                if (u != null) u.CanAttack = canAttack;
            }
        }

        public int GetCount()
        {
            return _cards.Count;
        }
    }
}
