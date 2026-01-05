namespace GGemCo2DTcg
{
    /// <summary>
    /// UI 연출 단계 데이터.
    /// - Side: 행위자 기준 side
    /// - FromIndex/ToIndex: 손패/보드 인덱스 등
    /// - ValueA/ValueB: 데미지 등 보조 값
    /// </summary>
    public readonly struct TcgPresentationStep
    {
        //StepType은 “연출 단위”이며, Payload는 “연출 데이터 DTO”
        //StepType은 UI 핸들러가 분기하는 키
        //Payload는 UI가 필요한 최소 데이터를 담는 값 타입(이미 TcgAbilityPayload*로 구현 중)
        public TcgPresentationConstants.TcgPresentationStepType Type { get; }
        public ConfigCommonTcg.TcgPlayerSide Side { get; }
        public ConfigCommonTcg.TcgZone FromZone { get; }
        public int FromIndex { get; }
        public ConfigCommonTcg.TcgZone ToZone { get; }
        public int ToIndex { get; }

        /// <summary>
        /// 스텝별 확장 데이터를 전달하기 위한 페이로드입니다.
        /// 
        /// - 기존 ValueA~F는 레거시 호환/간단 값 전달용으로 유지합니다.
        /// - 복잡한 연출(피해/회복 팝업, 버프 등)이 필요할 경우, 구조화된 payload를 넣어 사용합니다.
        /// </summary>
        public object Payload { get; }

        public TcgPresentationStep(
            TcgPresentationConstants.TcgPresentationStepType type,
            ConfigCommonTcg.TcgPlayerSide side,
            ConfigCommonTcg.TcgZone fromZone,
            int fromIndex,
            ConfigCommonTcg.TcgZone toZone,
            int toIndex,
            object payload = null)
        {
            Type = type;
            Side = side;
            FromZone = fromZone;
            FromIndex = fromIndex;
            ToZone = toZone;
            ToIndex = toIndex;
            Payload = payload;
        }
    }
}