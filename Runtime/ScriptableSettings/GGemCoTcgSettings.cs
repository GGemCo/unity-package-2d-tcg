using System;
using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// TCG 전역 설정을 보관하는 ScriptableObject입니다.
    /// </summary>
    /// <remarks>
    /// 포함 항목:
    /// - 덱 규칙(최소/최대 카드 수)
    /// - 시작 핸드/턴당 드로우 수
    /// - 마나 커브(시작/최대/턴 종료 증가)
    /// - 턴 제한 시간 및 최대 턴 수
    /// - 셔플 모드/설정(플레이어/적)
    /// - 설정 변경 알림(런타임/에디터)
    /// </remarks>
    [CreateAssetMenu(
        fileName = ConfigScriptableObjectTcg.TcgSettings.FileName,
        menuName = ConfigScriptableObjectTcg.TcgSettings.MenuName,
        order    = ConfigScriptableObjectTcg.TcgSettings.Ordering)]
    public class GGemCoTcgSettings : ScriptableObject, ISettingsChangeNotifier
    {
        /// <summary>
        /// 설정 값이 변경되었음을 알리는 이벤트입니다.
        /// </summary>
        /// <remarks>
        /// 런타임/에디터 공용 알림이며, 이벤트 자체는 직렬화되지 않습니다.
        /// </remarks>
        public event Action Changed;

#if UNITY_EDITOR
        /// <summary>
        /// 에디터에서 인스펙터 값이 변경될 때 자동으로 호출되어, 유효성 검증 및 변경 알림을 수행합니다.
        /// </summary>
        /// <remarks>
        /// - 수치의 최소값/상호 제약을 보정합니다.
        /// - 셔플 설정 에셋 타입이 올바른지 검증합니다.
        /// - 마지막에 <see cref="Changed"/> 이벤트를 발생시켜 런타임/툴이 변경을 감지할 수 있게 합니다.
        /// </remarks>
        private void OnValidate()
        {
            // 상호 제약 보정
            if (minDeckCardCount <= 0) minDeckCardCount = 1;
            if (maxDeckCardCount < minDeckCardCount) maxDeckCardCount = minDeckCardCount;

            if (startingHandCardCount <= 0) startingHandCardCount = 0;
            if (cardsDrawnPerTurn <= 0) cardsDrawnPerTurn = 0;

            if (turnTimeLimitSeconds <= 0) turnTimeLimitSeconds = 0;
            if (maxTurns <= 0) maxTurns = 0;

            ValidateShuffleSettings("Player", playerShuffleMode, playerShuffleSettings);
            ValidateShuffleSettings("Enemy", enemyShuffleMode, enemyShuffleSettings);

            Changed?.Invoke();
        }

        /// <summary>
        /// 셔플 모드에 필요한 설정 에셋이 지정되었는지, 그리고 올바른 타입인지 검증합니다.
        /// </summary>
        /// <param name="label">경고 메시지에 사용할 구분 라벨입니다(예: Player/Enemy).</param>
        /// <param name="mode">검증할 셔플 모드입니다.</param>
        /// <param name="settingsAsset">모드에 연결된 설정 ScriptableObject입니다.</param>
        /// <remarks>
        /// PureRandom/SeededReplay/None 등의 모드는 별도 설정 에셋이 없어도 동작하도록 허용합니다.
        /// </remarks>
        private static void ValidateShuffleSettings(string label, ConfigCommonTcg.ShuffleMode mode, ScriptableObject settingsAsset)
        {
            // PureRandom/SeededReplay는 설정이 없어도 문제 없음
            if (mode == ConfigCommonTcg.ShuffleMode.None ||
                mode == ConfigCommonTcg.ShuffleMode.PureRandom ||
                mode == ConfigCommonTcg.ShuffleMode.SeededReplay)
                return;

            if (settingsAsset == null)
            {
                Debug.LogWarning($"[{nameof(GGemCoTcgSettings)}] {label}ShuffleSettings 가 비어있습니다. mode={mode}");
                return;
            }

            if (settingsAsset is not ITcgShuffleSettingsAsset)
            {
                Debug.LogWarning(
                    $"[{nameof(GGemCoTcgSettings)}] {label}ShuffleSettings 타입이 올바르지 않습니다. " +
                    $"(ITcgShuffleSettingsAsset 미구현) mode={mode}, asset={settingsAsset.name}",
                    settingsAsset);
            }
        }
#endif

        /// <summary>
        /// 코드/툴에서 강제로 <see cref="Changed"/> 알림을 발생시킵니다.
        /// </summary>
        public void RaiseChanged() => Changed?.Invoke();

        // ──────────────────────────────────────────────────────────────────────────────
        // Deck Rules
        // ──────────────────────────────────────────────────────────────────────────────
        /// <summary>
        /// 덱 구성 규칙(최소/최대 카드 수) 설정입니다.
        /// </summary>
        [Header("덱 규칙 (Deck Rules)")]
        [Tooltip("게임을 시작할 수 있는 덱 최소 카드 개수")]
        public int minDeckCardCount = 1;

        /// <summary>
        /// 게임을 시작할 수 있는 덱 최대 카드 개수입니다.
        /// </summary>
        [Tooltip("게임을 시작할 수 있는 덱 최대 카드 개수")]
        [Min(0)]
        public int maxDeckCardCount = 5;

        // ──────────────────────────────────────────────────────────────────────────────
        // Hand & Draw
        // ──────────────────────────────────────────────────────────────────────────────
        /// <summary>
        /// 시작 핸드 및 턴당 드로우 수 설정입니다.
        /// </summary>
        [Header("핸드 & 드로우 (Hand & Draw)")]
        [Tooltip("게임 시작시 받는 카드 개수")]
        [Min(0)]
        public int startingHandCardCount = 3;

        /// <summary>
        /// 턴이 지날 때마다 드로우하는 카드 개수입니다.
        /// </summary>
        [Tooltip("턴이 지날때 마다 드로우하는 카드 개수")]
        [Min(0)]
        public int cardsDrawnPerTurn = 1;

        // ──────────────────────────────────────────────────────────────────────────────
        // Mana
        // ──────────────────────────────────────────────────────────────────────────────
        /// <summary>
        /// 전투에서 사용하는 마나 관련 설정입니다.
        /// </summary>
        [Header("마나")]
        [Tooltip("대결 시작 마나 수치")]
        public int countManaBattleStart = 1;

        /// <summary>
        /// 전투 중 도달 가능한 최대 마나 수치입니다.
        /// </summary>
        [Tooltip("대결 시 얻을 수 있는 최대 마나 수치")]
        public int countMaxManaInBattle = 10;

        /// <summary>
        /// 턴 종료 시 증가하는 마나 수치입니다.
        /// </summary>
        [Tooltip("턴이 종료될 때 증가하는 마나 수치")]
        public int countManaAfterTurn = 1;

        // ──────────────────────────────────────────────────────────────────────────────
        // Turn Limits
        // ──────────────────────────────────────────────────────────────────────────────
        /// <summary>
        /// 턴 제한(시간/최대 턴) 설정입니다.
        /// </summary>
        [Header("턴 제한 (Turn Limits)")]
        [Tooltip("턴 제한 시간(초)")]
        [Min(0)]
        public int turnTimeLimitSeconds = 60;

        /// <summary>
        /// 전투의 최대 턴 수입니다. 0이면 무제한으로 해석될 수 있습니다(프로젝트 규칙에 따름).
        /// </summary>
        [Tooltip("최대 턴 수")]
        [Min(0)]
        public int maxTurns = 10;

        // ──────────────────────────────────────────────────────────────────────────────
        // UI 연출
        // ──────────────────────────────────────────────────────────────────────────────
        /// <summary>
        /// UI 연출 관련 타이밍 설정입니다.
        /// </summary>
        [Header("UI 연출")]
        [Tooltip("한개의 Command 종료 후 대기 시간")]
        [Min(0)]
        public float timeWaitAfterCommand = 1.0f;

        // ──────────────────────────────────────────────────────────────────────────────
        // Shuffle
        // ──────────────────────────────────────────────────────────────────────────────
        /// <summary>
        /// 카드 셔플 정책(모드/설정 에셋) 및 적 덱 프리셋 설정입니다.
        /// </summary>
        [Header("카드 섞기")]
        [Tooltip("플레이어 섞기 모드")]
        public ConfigCommonTcg.ShuffleMode playerShuffleMode = ConfigCommonTcg.ShuffleMode.PureRandom;

        /// <summary>
        /// 플레이어 셔플 모드별 세부 설정 에셋입니다. (모드에 맞는 ScriptableObject를 지정)
        /// </summary>
        [Tooltip("플레이어 섞기 모드 설정 파일 (모드에 맞는 ScriptableObject를 지정)")]
        public ScriptableObject playerShuffleSettings;

        /// <summary>
        /// 적 덱 프리셋 설정입니다.
        /// </summary>
        [Tooltip("적 덱 프리셋")]
        public EnemyDeckPreset enemyDeckPreset;

        /// <summary>
        /// 적 섞기 모드입니다.
        /// </summary>
        [Tooltip("적 섞기 모드")]
        public ConfigCommonTcg.ShuffleMode enemyShuffleMode = ConfigCommonTcg.ShuffleMode.PureRandom;

        /// <summary>
        /// 적 셔플 모드별 세부 설정 에셋입니다. (모드에 맞는 ScriptableObject를 지정)
        /// </summary>
        [Tooltip("적 섞기 모드 설정 파일 (모드에 맞는 ScriptableObject를 지정)")]
        public ScriptableObject enemyShuffleSettings;

        /// <summary>
        /// 플레이어/적 구분에 따라 셔플 정책(모드, 설정 에셋)을 반환합니다.
        /// </summary>
        /// <param name="side">셔플 정책을 조회할 대상 진영입니다.</param>
        /// <returns>셔플 모드와 설정 에셋의 튜플입니다.</returns>
        public (ConfigCommonTcg.ShuffleMode mode, ScriptableObject settings) GetShufflePolicy(ConfigCommonTcg.TcgPlayerSide side)
        {
            return side switch
            {
                ConfigCommonTcg.TcgPlayerSide.Player => (playerShuffleMode, playerShuffleSettings),
                ConfigCommonTcg.TcgPlayerSide.Enemy => (enemyShuffleMode, enemyShuffleSettings),
                _ => (ConfigCommonTcg.ShuffleMode.PureRandom, null)
            };
        }

#if UNITY_EDITOR
        // ──────────────────────────────────────────────────────────────────────────────
        // 테스트용
        // ──────────────────────────────────────────────────────────────────────────────
        /// <summary>
        /// 에디터 테스트/디버그를 위한 설정 값들입니다. (런타임 빌드에는 포함되지 않을 수 있음)
        /// </summary>
        [Header("테스트용")]
        [Tooltip("테스트를 위해 고정 Seed 값을 사용할 경우 입력하세요.")]
        public int testSeed;
        [Tooltip("셔플된 덱 정보를 콘솔에 출력 합니다.")]
        public bool showDeckInfo;
        [Tooltip("셔플할때 적용된 Seed값과 셔플된 덱 정보를 콘솔에 출력합니다.")]
        public bool showShuffleInfo;
        [Tooltip("카드 이름 앞에 고유번호 Uid를 출력합니다.")]
        public bool showCardUid;

        /// <summary>
        /// (에디터) 내부 캐시 재빌드가 필요할 때 호출하는 컨텍스트 메뉴 항목입니다.
        /// </summary>
        /// <remarks>
        /// 현재는 변경 알림만 발생시키며, 필요 시 캐시 재빌드 로직을 추가할 수 있습니다.
        /// </remarks>
        [ContextMenu("Rebuild Cache & Raise Changed")]
        private void RebuildCacheAndNotify()
        {
            // 필요 시 내부 캐시 재빌드 로직 추가 지점
            Changed?.Invoke();
        }
#endif
    }
}
