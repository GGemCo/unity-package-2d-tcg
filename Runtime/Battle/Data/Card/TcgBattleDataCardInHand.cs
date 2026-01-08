using System;
using System.Collections.Generic;
using R3;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 셔플/드로우(덱 처리) 시스템에서 사용하는 카드의 최소 정보 인터페이스입니다.
    /// 가중 셔플(weighted shuffle) 등 덱 알고리즘에서 필요한 기본 스탯(예: 코스트)을 노출합니다.
    /// </summary>
    public interface ICardInfo
    {
        /// <summary>
        /// 카드 플레이 코스트입니다.
        /// </summary>
        int Cost { get; }

        // 필요 시 Type, Grade 등도 여기서 노출 가능:
        // CardConstants.Type Type { get; }
        // CardConstants.Grade Grade { get; }
    }

    /// <summary>
    /// 전투에서 사용하는 손패(Hand) 카드의 런타임 데이터(주로 정적/템플릿 정보)입니다.
    /// 테이블에서 읽은 정적 정보를 기반으로 생성되며, 전투 중 변하는 값(버프/디버프 등)은 별도 런타임(예: 필드 유닛)에서 관리하는 것을 전제로 합니다.
    /// </summary>
    public sealed class TcgBattleDataCardInHand : ICardInfo
    {
        /// <summary>
        /// 카드(템플릿) 식별자입니다.
        /// </summary>
        public int Uid { get; }

        /// <summary>
        /// 카드 이름입니다.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 카드 타입(크리처/스펠 등)입니다.
        /// </summary>
        public CardConstants.Type Type { get; }

        /// <summary>
        /// 카드 등급(레어도)입니다.
        /// </summary>
        public CardConstants.Grade Grade { get; }

        /// <summary>
        /// 카드 플레이 코스트입니다.
        /// </summary>
        public int Cost { get; }

        /// <summary>
        /// 공격력 값 스트림(Observable)입니다.
        /// UI/연출에서 구독해 자동 갱신할 수 있습니다.
        /// </summary>
        public readonly BehaviorSubject<int> attack = new(0);

        /// <summary>
        /// 현재 공격력(편의용 프로퍼티)입니다.
        /// </summary>
        public int Attack => attack.Value;

        /// <summary>
        /// 체력 값 스트림(Observable)입니다.
        /// UI/연출에서 구독해 자동 갱신할 수 있습니다.
        /// </summary>
        public readonly BehaviorSubject<int> health = new(0);

        /// <summary>
        /// 현재 체력(편의용 프로퍼티)입니다.
        /// </summary>
        public int Health => health.Value;

        /// <summary>
        /// 덱에 포함 가능한 최대 복제 수(동일 카드 최대 장수)입니다.
        /// </summary>
        public int MaxCopiesPerDeck { get; }

        /// <summary>
        /// 카드 이미지 파일명(리소스 키)입니다.
        /// </summary>
        public string ImageFileName { get; }

        /// <summary>
        /// 타입별 상세 테이블 데이터(존재하는 경우)입니다.
        /// 단일 테이블 기반(legacy) 데이터와 병행 운용하는 동안 런타임 판단/실행에 사용됩니다.
        /// </summary>
        public StruckTableTcgCardSpell SpellDetail { get; }

        /// <summary>
        /// 장비(Equipment) 타입의 상세 테이블 데이터(존재하는 경우)입니다.
        /// </summary>
        public StruckTableTcgCardEquipment EquipmentDetail { get; }

        /// <summary>
        /// 영구(Permanent) 타입의 상세 테이블 데이터(존재하는 경우)입니다.
        /// </summary>
        public StruckTableTcgCardPermanent PermanentDetail { get; }

        /// <summary>
        /// 이벤트(Event) 타입의 상세 테이블 데이터(존재하는 경우)입니다.
        /// </summary>
        public StruckTableTcgCardEvent EventDetail { get; }

        /// <summary>
        /// 카드 설명(툴팁/텍스트)입니다.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// 카드가 보유한 키워드 목록(읽기 전용)입니다.
        /// </summary>
        public IReadOnlyList<ConfigCommonTcg.TcgKeyword> Keywords => _keywords;

        private readonly List<ConfigCommonTcg.TcgKeyword> _keywords = new List<ConfigCommonTcg.TcgKeyword>(4);

        /// <summary>
        /// 카드 테이블 행 데이터를 기반으로 손패 카드 런타임을 생성합니다.
        /// </summary>
        /// <param name="row">카드의 정적 정보를 담은 테이블 행 데이터입니다.</param>
        /// <param name="keywords">카드에 부여된 키워드 목록입니다(없으면 null 가능).</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="row"/>가 null인 경우(계약을 강제하고 싶을 때) 발생할 수 있습니다.
        /// </exception>
        /// <remarks>
        /// 현재 구현은 예외를 실제로 throw 하지는 않으며, 호출부에서 유효한 <paramref name="row"/>가 들어온다는 전제를 두고 있습니다.
        /// </remarks>
        public TcgBattleDataCardInHand(
            StruckTableTcgCard row,
            IReadOnlyList<ConfigCommonTcg.TcgKeyword> keywords)
        {
            Uid = row.uid;
            Name = row.name;
            Type = row.type;
            Grade = row.grade;
            Cost = row.cost;
            MaxCopiesPerDeck = row.maxCopiesPerDeck;
            ImageFileName = row.imageFileName;
            Description = row.description;

            SpellDetail = row.struckTableTcgCardSpell;
            EquipmentDetail = row.struckTableTcgCardEquipment;
            PermanentDetail = row.struckTableTcgCardPermanent;
            EventDetail = row.struckTableTcgCardEvent;

            attack.OnNext(row.GetAttack());
            health.OnNext(row.GetHealth());

            if (keywords != null) _keywords.AddRange(keywords);
        }
    }
}
