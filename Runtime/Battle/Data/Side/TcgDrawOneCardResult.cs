namespace GGemCo2DTcg
{
    /// <summary>
    /// 덱에서 카드 1장을 드로우했을 때의 결과를 표현하는 값 타입입니다.
    /// UI 연출(예: 어떤 카드가 손패에 추가되었는지)을 위해 필요한 최소 정보를 포함합니다.
    /// </summary>
    public readonly struct TcgDrawOneCardResult
    {
        /// <summary>
        /// 손패에 정상적으로 추가되었는지 여부입니다.
        /// </summary>
        public bool AddedToHand { get; }

        /// <summary>
        /// 손패에 추가된 카드 런타임 데이터입니다. <see cref="AddedToHand"/>가 false이면 null일 수 있습니다.
        /// </summary>
        public TcgBattleDataCardInHand CardInHand { get; }

        /// <summary>
        /// 손패에 추가된 인덱스입니다. 실패 시 -1입니다.
        /// </summary>
        public int HandIndex { get; }

        /// <summary>
        /// 덱이 비어 피로(Fatigue) 규칙이 적용되었는지 여부입니다.
        /// </summary>
        public bool Fatigue { get; }

        /// <summary>
        /// 손패가 가득 차 오버드로우(Overdraw) 규칙이 적용되었는지 여부입니다.
        /// </summary>
        public bool Overdraw { get; }

        public TcgDrawOneCardResult(
            bool addedToHand,
            TcgBattleDataCardInHand cardInHand,
            int handIndex,
            bool fatigue,
            bool overdraw)
        {
            AddedToHand = addedToHand;
            CardInHand = cardInHand;
            HandIndex = handIndex;
            Fatigue = fatigue;
            Overdraw = overdraw;
        }

        public static TcgDrawOneCardResult CreateAdded(TcgBattleDataCardInHand cardInHand, int handIndex)
            => new TcgDrawOneCardResult(true, cardInHand, handIndex, fatigue: false, overdraw: false);

        public static TcgDrawOneCardResult CreateFatigue()
            => new TcgDrawOneCardResult(false, null, -1, fatigue: true, overdraw: false);

        public static TcgDrawOneCardResult CreateOverdraw(TcgBattleDataCardInHand cardInHand)
            => new TcgDrawOneCardResult(false, cardInHand, -1, fatigue: false, overdraw: true);
    }
}
