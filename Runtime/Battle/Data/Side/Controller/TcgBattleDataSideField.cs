using System;
using System.Collections.Generic;
using GGemCo2DCore;
using R3;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 전투 중 한 쪽 플레이어의 필드(Field) 영역을 관리하는 도메인 클래스입니다.
    /// 필드 유닛의 추가/제거, 조회(인덱스 기반), 공격 가능 상태 일괄 갱신 등을 담당합니다.
    /// </summary>
    /// <remarks>
    /// - 실제 수치 변경(공격력/체력 등)과 전투 규칙 판정은 BattleManager/이펙트 시스템에서 수행하는 것을 전제로 합니다.
    /// - 인덱스 재정렬은 연출(애니메이션) 완료 후 Refresh All에서 처리한다는 정책이 코드 주석으로 남아 있습니다.
    /// </remarks>
    public sealed class TcgBattleDataSideField
    {
        /// <summary>
        /// 필드에 배치된 유닛의 내부 리스트입니다.
        /// </summary>
        private List<TcgBattleDataCardInField> _cards;

        /// <summary>
        /// 외부에 노출되는 필드 카드 목록의 읽기 전용 뷰입니다.
        /// </summary>
        public IReadOnlyList<TcgBattleDataCardInField> Cards => _cards;

        /// <summary>
        /// 현재 필드에 배치된 카드(유닛) 수입니다. (Hero는 별도 보관될 수 있습니다)
        /// </summary>
        public int Count => _cards.Count;

        /// <summary>
        /// 해당 진영의 영웅 유닛(존재하는 경우)입니다.
        /// </summary>
        public TcgBattleDataCardInField Hero { get; private set; }

        /// <summary>
        /// 필드 카드 관리 객체를 생성합니다.
        /// </summary>
        /// <param name="initialCapacity">내부 리스트의 초기 용량입니다. 1 미만이면 1로 보정됩니다.</param>
        public TcgBattleDataSideField(int initialCapacity = 8)
        {
            _cards = new List<TcgBattleDataCardInField>(Math.Max(1, initialCapacity));
        }

        /// <summary>
        /// 지정한 유닛이 현재 필드에 포함되어 있는지 확인합니다.
        /// </summary>
        /// <param name="unit">존재 여부를 확인할 필드 유닛입니다.</param>
        /// <returns>필드에 존재하면 true, 아니면 false입니다.</returns>
        public bool Contains(TcgBattleDataCardInField unit)
        {
            return _cards.Contains(unit);
        }

        /// <summary>
        /// 필드 내에서 지정한 유닛의 인덱스를 반환합니다.
        /// </summary>
        /// <param name="cardInField">인덱스를 조회할 필드 유닛입니다.</param>
        /// <returns>존재하면 0 이상의 인덱스, 없거나 null이면 -1입니다.</returns>
        /// <remarks>
        /// 목록에 없는 유닛을 요청하면 에러 로그를 남깁니다.
        /// </remarks>
        public int IndexOf(TcgBattleDataCardInField cardInField)
        {
            int index = cardInField == null ? -1 : _cards.IndexOf(cardInField);
            if (index < 0)
                GcLogger.LogError($"핸드 카드 리스트에 없는 정보{nameof(TcgBattleDataCardInHand)} 입니다. cardInHand: {cardInField}");
            return index;
        }

        /// <summary>
        /// 필드 인덱스를 기준으로 유닛을 조회합니다.
        /// </summary>
        /// <param name="index">조회할 필드 인덱스입니다.</param>
        /// <param name="includeHero">true이면 Hero도 인덱스 비교 대상으로 포함합니다.</param>
        /// <returns>유효한 인덱스이면 유닛, 아니면 null입니다.</returns>
        /// <remarks>
        /// - 기본적으로는 내부 리스트(<see cref="Cards"/>)에서만 조회합니다.
        /// - <paramref name="includeHero"/>가 true이고, Hero가 존재하며 Hero의 Index가 일치하면 Hero를 반환합니다.
        /// - 조회 실패 시 에러 로그를 남깁니다.
        /// </remarks>
        public TcgBattleDataCardInField GetByIndex(int index, bool includeHero = false)
        {
            if (index < 0) return null;

            TcgBattleDataCardInField data = (uint)index < (uint)_cards.Count ? _cards[index] : null;

            if (includeHero && Hero != null && Hero.Index == index) data = Hero;

            if (data == null)
                GcLogger.LogError($"핸드 카드 리스트에 없는 index 번호 입니다. index: {index}");

            return data;
        }

        /// <summary>
        /// 유닛을 필드에 추가합니다.
        /// </summary>
        /// <param name="unit">추가할 필드 유닛입니다.</param>
        /// <returns>추가된 위치의 인덱스이며, <paramref name="unit"/>이 null이면 -1입니다.</returns>
        /// <remarks>
        /// 인덱스(<see cref="TcgBattleDataCardInField.Index"/>)는 연출 완료 후 Refresh All에서 설정한다는 정책으로 인해
        /// 현재 메서드에서는 직접 갱신하지 않습니다.
        /// </remarks>
        public int Add(TcgBattleDataCardInField unit)
        {
            if (unit == null) return -1;

            _cards.Add(unit);
            var index = _cards.Count - 1;

            // 연출이 끝난 후 Refresh All 에서 바꿔준다.
            // unit.Index = index;

            return index;
        }

        /// <summary>
        /// 영웅 유닛을 설정합니다.
        /// </summary>
        /// <param name="unit">영웅으로 등록할 필드 유닛입니다.</param>
        public void AddHero(TcgBattleDataCardInField unit) => Hero = unit;

        /// <summary>
        /// 지정한 유닛을 필드에서 제거합니다.
        /// </summary>
        /// <param name="unit">제거할 필드 유닛입니다.</param>
        /// <returns>제거에 성공하면 true, 실패하면 false입니다.</returns>
        /// <remarks>
        /// 제거 후 남은 유닛들의 인덱스 재정렬은 연출 완료 후 Refresh All에서 처리한다는 정책으로 인해
        /// 현재 메서드에서는 수행하지 않습니다.
        /// </remarks>
        public bool Remove(TcgBattleDataCardInField unit)
        {
            if (unit == null) return false;

            var index = IndexOf(unit);
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
        /// 필드에 있는 모든 유닛의 공격 가능 여부를 일괄 설정합니다.
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
    }
}
