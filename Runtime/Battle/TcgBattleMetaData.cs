using System;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 한 판의 TCG 대결을 시작하기 위해 필요한 설정 값 묶음.
    /// 메뉴/씬 쪽에서 만들어서 TcgBattleManager.StartBattle(...) 에 전달합니다.
    /// </summary>
    [Serializable]
    public sealed class TcgBattleMetaData
    {
        /// <summary>
        /// 플레이어가 사용할 덱 인덱스 또는 Uid.
        /// SaveDataManagerTcg.MyDeck 에서 실제 덱 데이터를 조회할 때 사용합니다.
        /// </summary>
        public int playerDeckIndex;

        /// <summary>
        /// AI가 사용할 덱 프리셋 식별자.
        /// AiDeckPreset ScriptableObject 등을 사용하는 경우 그 Uid 또는 인덱스를 사용합니다.
        /// </summary>
        public int enemyDeckPresetId;

        /// <summary>
        /// 셔플 시드. 0 이하이면 내부에서 자동 생성합니다.
        /// </summary>
        public int initialSeed;

        /// <summary>
        /// 첫 턴을 플레이어가 시작할지 여부. null 이면 규칙에 따라 자동 결정합니다.
        /// </summary>
        public bool? isPlayerFirst;
    }
}