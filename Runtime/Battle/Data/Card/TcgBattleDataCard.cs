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
        public CardConstants.TargetType TargetType { get; }

        public int Cost { get; }

        public readonly BehaviorSubject<int> attack = new(0);
        public int Attack => attack.Value;
        public readonly BehaviorSubject<int> health = new(0);
        public int Health => health.Value;

        public int MaxCopiesPerDeck { get; }

        public string ImageFileName { get; }
        public string Description { get; }

        public IReadOnlyList<ConfigCommonTcg.TcgKeyword> Keywords => _keywords;
        public IReadOnlyList<TcgAbilityData> SummonEffects => _summonEffects;
        public IReadOnlyList<TcgAbilityData> SpellEffects => _spellEffects;
        public IReadOnlyList<TcgAbilityData> DeathEffects => _deathEffects;

        private readonly List<ConfigCommonTcg.TcgKeyword> _keywords = new List<ConfigCommonTcg.TcgKeyword>(4);
        private readonly List<TcgAbilityData> _summonEffects = new List<TcgAbilityData>(4);
        private readonly List<TcgAbilityData> _spellEffects = new List<TcgAbilityData>(4);
        private readonly List<TcgAbilityData> _deathEffects = new List<TcgAbilityData>(4);
        
        public TcgBattleDataCard(
            StruckTableTcgCard row,
            IReadOnlyList<ConfigCommonTcg.TcgKeyword> keywords,
            IReadOnlyList<TcgAbilityData> summonEffects,
            IReadOnlyList<TcgAbilityData> spellEffects,
            IReadOnlyList<TcgAbilityData> deathEffects)
        {
            Uid = row.uid;
            Name = row.name;
            Type = row.type;
            Grade = row.grade;
            Cost = row.cost;
            MaxCopiesPerDeck = row.maxCopiesPerDeck;
            ImageFileName = row.imageFileName;
            Description = row.description;

            attack.OnNext(row.GetAttack());
            health.OnNext(row.GetHealth());

            if (keywords != null) _keywords.AddRange(keywords);
            if (summonEffects != null) _summonEffects.AddRange(summonEffects);
            if (spellEffects != null) _spellEffects.AddRange(spellEffects);
            if (deathEffects != null) _deathEffects.AddRange(deathEffects);
        }
    }
}
