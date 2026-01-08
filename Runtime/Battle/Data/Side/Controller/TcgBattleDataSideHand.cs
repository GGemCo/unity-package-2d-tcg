using System;
using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 전투 중 한 쪽 플레이어의 손패(Hand) 영역을 관리하는 도메인 클래스입니다.
    /// 카드 추가/제거와 최대 손패 크기 제한을 담당하며,
    /// 외부에서는 읽기 전용 목록으로만 접근하도록 캡슐화합니다.
    /// </summary>
    public sealed class TcgBattleDataSideHand
    {
        /// <summary>
        /// 손패에 보관된 카드의 내부 리스트입니다.
        /// </summary>
        private readonly List<TcgBattleDataCardInHand> _cards;

        /// <summary>
        /// 외부에 노출되는 손패 카드 목록의 읽기 전용 뷰입니다.
        /// </summary>
        public IReadOnlyList<TcgBattleDataCardInHand> Cards => _cards;

        /// <summary>
        /// 현재 손패에 있는 카드 수입니다.
        /// </summary>
        public int Count => _cards.Count;

        /// <summary>
        /// 손패와 별도로 관리되는 히어로 카드(존재하는 경우)입니다.
        /// </summary>
        public TcgBattleDataCardInHand Hero { get; private set; }

        /// <summary>
        /// 손패에 허용되는 최대 카드 수입니다.
        /// </summary>
        public int MaxSize { get; }

        /// <summary>
        /// 손패 데이터를 생성합니다.
        /// </summary>
        /// <param name="initialCapacity">내부 리스트의 초기 용량입니다. 1 미만이면 1로 보정됩니다.</param>
        /// <param name="maxSize">손패 최대 크기입니다. 1 미만이면 1로 보정됩니다.</param>
        public TcgBattleDataSideHand(int initialCapacity = 16, int maxSize = 10)
        {
            _cards = new List<TcgBattleDataCardInHand>(Math.Max(1, initialCapacity));
            MaxSize = Math.Max(1, maxSize);
        }

        /// <summary>
        /// 지정한 카드가 현재 손패에 포함되어 있는지 확인합니다.
        /// </summary>
        /// <param name="cardInHand">확인할 카드 런타임 데이터입니다.</param>
        /// <returns>손패에 포함되어 있으면 true, 아니면 false입니다.</returns>
        public bool Contains(TcgBattleDataCardInHand cardInHand)
            => cardInHand != null && _cards.Contains(cardInHand);

        /// <summary>
        /// 손패 내에서 지정한 카드의 인덱스를 반환합니다.
        /// </summary>
        /// <param name="cardInHand">인덱스를 조회할 카드 런타임 데이터입니다.</param>
        /// <returns>존재하면 0 이상의 인덱스, 없거나 null이면 -1입니다.</returns>
        /// <remarks>
        /// 목록에 없는 카드를 조회하면 에러 로그를 남깁니다.
        /// </remarks>
        public int IndexOf(TcgBattleDataCardInHand cardInHand)
        {
            int index = cardInHand == null ? -1 : _cards.IndexOf(cardInHand);
            if (index < 0)
                GcLogger.LogError($"핸드 카드 리스트에 없는 정보{nameof(TcgBattleDataCardInHand)} 입니다. cardInHand: {cardInHand}");
            return index;
        }

        /// <summary>
        /// 손패 인덱스를 기준으로 카드 데이터를 조회합니다.
        /// </summary>
        /// <param name="index">조회할 손패 인덱스입니다.</param>
        /// <returns>유효한 인덱스이면 카드 데이터, 아니면 null입니다.</returns>
        /// <remarks>
        /// 조회 실패 시 에러 로그를 남깁니다.
        /// </remarks>
        public TcgBattleDataCardInHand GetByIndex(int index)
        {
            TcgBattleDataCardInHand data = (uint)index < (uint)_cards.Count ? _cards[index] : null;
            if (data == null)
                GcLogger.LogError($"핸드 카드 리스트에 없는 index 번호 입니다. index: {index}");
            return data;
        }

        /// <summary>
        /// 현재 손패에 카드를 추가할 수 있는지 여부입니다.
        /// </summary>
        public bool CanAdd => _cards.Count < MaxSize;

        /// <summary>
        /// 손패에 카드를 추가합니다.
        /// </summary>
        /// <param name="cardInHand">추가할 카드 런타임 데이터입니다.</param>
        /// <returns>추가에 성공하면 true, 실패하면 false입니다.</returns>
        /// <remarks>
        /// - 카드가 null이거나 손패가 가득 찬 경우 추가하지 않습니다.
        /// - 손패 초과 시의 처리(예: 소각/패널티)는 호출부 규칙에 맡깁니다.
        /// </remarks>
        public bool TryAdd(TcgBattleDataCardInHand cardInHand)
        {
            if (cardInHand == null) return false;
            if (!CanAdd) return false;

            _cards.Add(cardInHand);
            return true;
        }

        /// <summary>
        /// 손패에서 지정한 카드를 제거합니다.
        /// </summary>
        /// <param name="cardInHand">제거할 카드 런타임 데이터입니다.</param>
        /// <param name="index">제거된 카드의 기존 손패 인덱스입니다. 실패 시 -1입니다.</param>
        /// <returns>제거에 성공하면 true, 실패하면 false입니다.</returns>
        public bool TryRemove(TcgBattleDataCardInHand cardInHand, out int index)
        {
            index = -1;
            if (cardInHand == null) return false;

            index = IndexOf(cardInHand);
            if (index < 0) return false;

            _cards.RemoveAt(index);
            return true;
        }

        /// <summary>
        /// 히어로 카드를 설정합니다.
        /// </summary>
        /// <param name="unit">히어로로 등록할 카드 런타임 데이터입니다.</param>
        /// <remarks>
        /// null 체크가 필요하다면 호출부 또는 이 메서드에서 방어 로직을 추가할 수 있습니다.
        /// </remarks>
        public void AddHero(TcgBattleDataCardInHand unit)
        {
            Hero = unit;
        }
    }
}
