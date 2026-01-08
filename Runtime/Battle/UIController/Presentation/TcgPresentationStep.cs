namespace GGemCo2DTcg
{
    /// <summary>
    /// UI 프레젠테이션에서 재생되는 단일 연출 단계(Step)를 나타내는 데이터입니다.
    /// </summary>
    /// <remarks>
    /// - <see cref="Type"/>은 “어떤 연출을 재생할지”를 결정하는 키(핸들러 분기 기준)입니다.
    /// - <see cref="Payload"/>는 Step별로 필요한 최소 데이터를 담는 DTO로,
    ///   복잡한 연출(피해/회복/버프 등)에 대해 구조화된 데이터를 전달하기 위해 사용합니다.
    /// - 좌표/대상 정보는 <see cref="FromZone"/>/<see cref="FromIndex"/> 및 <see cref="ToZone"/>/<see cref="ToIndex"/>로 표현합니다.
    /// </remarks>
    public readonly struct TcgPresentationStep
    {
        /// <summary>
        /// 연출 단계의 종류입니다(UI 핸들러가 분기하는 키).
        /// </summary>
        public TcgPresentationConstants.TcgPresentationStepType Type { get; }

        /// <summary>
        /// 행위자 기준의 플레이어 사이드입니다.
        /// </summary>
        public ConfigCommonTcg.TcgPlayerSide Side { get; }

        /// <summary>
        /// 연출의 출발 존(손패/필드 등)입니다.
        /// </summary>
        public ConfigCommonTcg.TcgZone FromZone { get; }

        /// <summary>
        /// 출발 존 내부 인덱스(손패/보드 슬롯 등)입니다.
        /// </summary>
        public int FromIndex { get; }

        /// <summary>
        /// 연출의 도착 존(손패/필드 등)입니다.
        /// </summary>
        public ConfigCommonTcg.TcgZone ToZone { get; }

        /// <summary>
        /// 도착 존 내부 인덱스(손패/보드 슬롯 등)입니다.
        /// </summary>
        public int ToIndex { get; }

        /// <summary>
        /// 스텝별 확장 데이터를 전달하기 위한 페이로드입니다.
        /// </summary>
        /// <remarks>
        /// - 단순 연출은 null로도 충분할 수 있습니다.
        /// - 복잡한 연출은 <c>TcgAbilityPayload*</c>와 같은 구조화된 DTO를 넣어 사용합니다.
        /// - 이 값은 object이므로, 소비 측에서는 <c>as</c>/<c>is</c> 패턴 매칭 등으로 안전하게 캐스팅해야 합니다.
        /// </remarks>
        public object Payload { get; }

        /// <summary>
        /// 지정한 Step 타입과 좌표 정보, 선택적 페이로드로 <see cref="TcgPresentationStep"/>을 생성합니다.
        /// </summary>
        /// <param name="type">연출 단계 타입입니다.</param>
        /// <param name="side">행위자 기준 플레이어 사이드입니다.</param>
        /// <param name="fromZone">출발 존입니다.</param>
        /// <param name="fromIndex">출발 존 내부 인덱스입니다.</param>
        /// <param name="toZone">도착 존입니다.</param>
        /// <param name="toIndex">도착 존 내부 인덱스입니다.</param>
        /// <param name="payload">Step별 확장 데이터(없으면 null)입니다.</param>
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
