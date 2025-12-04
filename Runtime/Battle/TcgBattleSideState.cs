using System.Collections.Generic;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 전투에서 한 쪽 플레이어의 런타임 상태.
    /// - 덱, 손패, 필드, 영웅 정보 등을 한 곳에 모읍니다.
    /// - 내부 컬렉션은 BattleManager 가 관리하고,
    ///   외부에는 읽기 전용으로 노출하는 것을 권장합니다.
    /// </summary>
    public sealed class TcgBattleSideState
    {
        public ConfigCommonTcg.TcgPlayerSide Side { get; }

        /// <summary>
        /// 이 플레이어의 덱 런타임.
        /// </summary>
        public DeckRuntime<CardRuntime> DeckRuntime { get; }

        /// <summary>
        /// 손패. 외부에는 IReadOnlyList 로 노출하는 것이 안전합니다.
        /// </summary>
        public List<CardRuntime> Hand { get; } = new List<CardRuntime>(16);

        /// <summary>
        /// 필드 위의 유닛들.
        /// </summary>
        public List<TcgUnitRuntime> Board { get; } = new List<TcgUnitRuntime>(8);

        /// <summary>
        /// 영웅(플레이어) 체력 등.
        /// </summary>
        public int HeroHp { get; set; }
        public int HeroHpMax { get; set; }

        /// <summary>
        /// 현재 마나/코스트.
        /// </summary>
        public int CurrentMana { get; set; }
        public int MaxMana { get; set; }

        public TcgBattleSideState(
            ConfigCommonTcg.TcgPlayerSide side,
            DeckRuntime<CardRuntime> deckRuntime)
        {
            Side = side;
            DeckRuntime = deckRuntime;
        }

        public IReadOnlyList<CardRuntime> ReadOnlyHand => Hand;
        public IReadOnlyList<TcgUnitRuntime> ReadOnlyBoard => Board;
    }
}