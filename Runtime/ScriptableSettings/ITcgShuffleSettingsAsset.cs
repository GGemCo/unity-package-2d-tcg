namespace GGemCo2DTcg
{
    /// <summary>
    /// 셔플 모드별 설정 ScriptableObject가 구현해야 하는 공통 인터페이스입니다.
    /// </summary>
    /// <remarks>
    /// 런타임 로직(예: TcgBattleDeckController)에서는 설정 에셋을 ScriptableObject로 다형 처리하되,
    /// 이 인터페이스를 통해 “현재 설정을 <see cref="ShuffleConfig"/>로 변환”하는 책임을 에셋에 위임합니다.
    /// <para>
    /// 모드별 설정(예: Weighted / PhaseWeighted 등)은 내부 필드 구조가 달라도,
    /// <see cref="BuildShuffleConfig"/>만 일관되게 제공하면 동일한 파이프라인으로 사용할 수 있습니다.
    /// </para>
    /// </remarks>
    public interface ITcgShuffleSettingsAsset
    {
        /// <summary>
        /// 현재 설정 에셋을 <see cref="ShuffleConfig"/>로 변환합니다.
        /// </summary>
        /// <param name="deckSize">덱의 총 카드 수입니다.</param>
        /// <param name="startMana">전투 시작 마나입니다.</param>
        /// <param name="maxMana">전투 중 도달 가능한 최대 마나입니다.</param>
        /// <param name="manaPerTurn">턴당 증가 마나입니다.</param>
        /// <param name="initialDrawCount">초기 드로우 카드 수입니다.</param>
        /// <param name="drawPerTurn">턴당 드로우 카드 수입니다.</param>
        /// <returns>설정이 반영된 셔플 구성(<see cref="ShuffleConfig"/>)입니다.</returns>
        /// <remarks>
        /// 구현체는 입력 파라미터(덱 크기/마나 커브/드로우 규칙)를 기반으로
        /// FrontLoadedCount, CostWeights 등 셔플에 필요한 값을 계산/보정해 반환해야 합니다.
        /// </remarks>
        ShuffleConfig BuildShuffleConfig(
            int deckSize,
            int startMana,
            int maxMana,
            int manaPerTurn,
            int initialDrawCount,
            int drawPerTurn);
    }
}