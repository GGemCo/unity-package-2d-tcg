using System;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 한 판의 TCG 전투를 시작하기 위해 필요한 설정 값 묶음입니다.
    /// </summary>
    /// <remarks>
    /// 메뉴/씬 등 외부 진입 지점에서 생성하여
    /// <c>TcgBattleManager.StartBattle(...)</c>에 전달하는 용도로 사용합니다.
    /// </remarks>
    [Serializable]
    public sealed class TcgBattleMetaData
    {
        /// <summary>
        /// 플레이어가 사용할 덱 인덱스(또는 UID)입니다.
        /// </summary>
        /// <remarks>
        /// <c>SaveDataManagerTcg.MyDeck</c>에서 실제 덱 데이터를 조회할 때 사용됩니다.
        /// 기본값(-1)은 “선택되지 않음”을 의미합니다.
        /// </remarks>
        public int playerDeckIndex = -1;

        /// <summary>
        /// AI가 사용할 덱 프리셋 식별자입니다.
        /// </summary>
        /// <remarks>
        /// AI 덱 프리셋 ScriptableObject 등을 사용하는 경우, 해당 UID 또는 인덱스를 전달합니다.
        /// </remarks>
        public int enemyDeckPresetId;

        /// <summary>
        /// 덱 셔플에 사용할 초기 시드입니다.
        /// </summary>
        /// <remarks>
        /// 0 이하이면 고정 시드를 사용하지 않으며(또는 내부 규칙에 따라 자동 생성),
        /// 실행 시점에 적용된 시드는 별도로 기록/로그될 수 있습니다.
        /// </remarks>
        public int initialSeed;

        /// <summary>
        /// 첫 턴을 플레이어가 시작할지 여부입니다.
        /// </summary>
        /// <remarks>
        /// - true: 플레이어 선공 고정
        /// - false: 적 선공 고정
        /// - null: 규칙/정책에 따라 자동 결정
        /// </remarks>
        public bool? isPlayerFirst;
    }
}