using System.Collections.Generic;
using R3;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 셔플/드로우 시스템에서 사용하는 카드 최소 정보 인터페이스.
    /// 가중 셔플 등을 위해 코스트 등 기본 스탯을 제공한다.
    /// </summary>
    public interface ICardInfo
    {
        /// <summary>카드 플레이 코스트.</summary>
        int Cost { get; }
        // 필요 시 Type, Grade 등도 여기서 노출 가능:
        // CardConstants.Type Type { get; }
        // CardConstants.Grade Grade { get; }
    }
    /// <summary>
    /// 전투에서 사용하는 카드 런타임 데이터.
    /// - 테이블에서 읽은 정적 정보를 기반으로 생성합니다.
    /// - 여기에는 "게임 중 변하는 값"은 넣지 않고,
    ///   주로 템플릿/정적 정보(스탯, 키워드, 이펙트)를 유지합니다.
    /// </summary>
    public sealed class TcgBattleDataCard : ICardInfo
    {
        public int Uid { get; }
        public string Name { get; }

        public CardConstants.Type Type { get; }
        public CardConstants.Grade Grade { get; }

        public int Cost { get; }

        public readonly BehaviorSubject<int> attack = new(0);
        public int Attack => attack.Value;
        public readonly BehaviorSubject<int> health = new(0);
        public int Health => health.Value;

        public int MaxCopiesPerDeck { get; }

        public string ImageFileName { get; }
        /// <summary>
/// 타입별 상세 테이블 데이터(있는 경우)입니다.
/// 단일 테이블 기반(legacy) 데이터와 병행 운용하는 동안, 런타임 판단/실행에 사용됩니다.
/// </summary>
        public StruckTableTcgCardSpell SpellDetail { get; }
        public StruckTableTcgCardEquipment EquipmentDetail { get; }
        public StruckTableTcgCardPermanent PermanentDetail { get; }
        public StruckTableTcgCardEvent EventDetail { get; }

        public string Description { get; }

        public IReadOnlyList<ConfigCommonTcg.TcgKeyword> Keywords => _keywords;
        private readonly List<ConfigCommonTcg.TcgKeyword> _keywords = new List<ConfigCommonTcg.TcgKeyword>(4);
        
        public TcgBattleDataCard(
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
