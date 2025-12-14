namespace GGemCo2DTcg
{
    /// <summary>
    /// 셔플 모드별 설정 ScriptableObject가 구현해야 하는 공통 인터페이스.
    ///
    /// - 런타임 로직(TcgBattleDeckController)에서는 ScriptableObject를 다형으로 받되,
    ///   이 인터페이스 구현 여부를 통해 "셔플 설정을 config로 변환"하는 책임을 위임합니다.
    /// - 각 모드 전용 설정 에셋(Weighted/PhaseWeighted 등)은 내부 필드 구조가 달라도,
    ///   BuildShuffleConfig 반환만 일관되게 제공하면 됩니다.
    /// </summary>
    public interface ITcgShuffleSettingsAsset
    {
        /// <summary>
        /// 현재 설정 에셋을 ShuffleConfig로 변환합니다.
        /// </summary>
        ShuffleConfig BuildShuffleConfig(
            int deckSize,
            int startMana,
            int maxMana,
            int manaPerTurn,
            int initialDrawCount,
            int drawPerTurn);
    }
}