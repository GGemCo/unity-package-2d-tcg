using System;
using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// TCG 전역 설정
    /// - 덱 규칙(최소/최대 카드 수)
    /// - 시작 핸드/턴당 드로우 수
    /// - 턴 제한 시간, 최대 턴 수
    /// - 셔플 모드/설정 (플레이어/적)
    /// - 설정 변경 알림(런타임/에디터)
    /// </summary>
    [CreateAssetMenu(
        fileName = ConfigScriptableObjectTcg.TcgSettings.FileName,
        menuName = ConfigScriptableObjectTcg.TcgSettings.MenuName,
        order    = ConfigScriptableObjectTcg.TcgSettings.Ordering)]
    public class GGemCoTcgSettings : ScriptableObject, ISettingsChangeNotifier
    {
        // 런타임/에디터 공용 알림 (직렬화되지 않음)
        public event Action Changed;

#if UNITY_EDITOR
        // 인스펙터 값 변경 시 자동 검증 및 알림
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

        private static void ValidateShuffleSettings(string label, ConfigCommonTcg.ShuffleMode mode, ScriptableObject settingsAsset)
        {
            // PureRandom/SeededReplay는 설정이 없어도 문제 없음
            if (mode == ConfigCommonTcg.ShuffleMode.None || mode == ConfigCommonTcg.ShuffleMode.PureRandom || mode == ConfigCommonTcg.ShuffleMode.SeededReplay)
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

        /// <summary>툴/코드에서 강제 알림</summary>
        public void RaiseChanged() => Changed?.Invoke();

        // ──────────────────────────────────────────────────────────────────────────────
        // Deck Rules
        // ──────────────────────────────────────────────────────────────────────────────
        [Header("덱 규칙 (Deck Rules)")]
        [Tooltip("게임을 시작할 수 있는 덱 최소 카드 개수")]
        public int minDeckCardCount = 1;

        [Tooltip("게임을 시작할 수 있는 덱 최대 카드 개수")]
        [Min(0)]
        public int maxDeckCardCount = 5;

        // ──────────────────────────────────────────────────────────────────────────────
        // Hand & Draw
        // ──────────────────────────────────────────────────────────────────────────────
        [Header("핸드 & 드로우 (Hand & Draw)")]
        [Tooltip("게임 시작시 받는 카드 개수")]
        [Min(0)]
        public int startingHandCardCount = 3;

        [Tooltip("턴이 지날때 마다 드로우하는 카드 개수")]
        [Min(0)]
        public int cardsDrawnPerTurn = 1;

        // ──────────────────────────────────────────────────────────────────────────────
        // Mana
        // ──────────────────────────────────────────────────────────────────────────────
        [Header("마나")]
        [Tooltip("대결 시작 마나 수치")]
        public int countManaBattleStart = 1;

        [Tooltip("대결 시 얻을 수 있는 최대 마나 수치")]
        public int countMaxManaInBattle = 10;

        [Tooltip("턴이 종료될 때 증가하는 마나 수치")]
        public int countManaAfterTurn = 1;

        // ──────────────────────────────────────────────────────────────────────────────
        // Turn Limits
        // ──────────────────────────────────────────────────────────────────────────────
        [Header("턴 제한 (Turn Limits)")]
        [Tooltip("턴 제한 시간(초)")]
        [Min(0)]
        public int turnTimeLimitSeconds = 60;

        [Tooltip("최대 턴 수")]
        [Min(0)]
        public int maxTurns = 10;
        
        // ──────────────────────────────────────────────────────────────────────────────
        // UI 연출
        // ──────────────────────────────────────────────────────────────────────────────
        [Header("UI 연출")]
        [Tooltip("한개의 Command 종료 후 대기 시간")]
        [Min(0)]
        public float timeWaitAfterCommand = 1.0f;

        // ──────────────────────────────────────────────────────────────────────────────
        // Shuffle
        // ──────────────────────────────────────────────────────────────────────────────
        [Header("카드 섞기")]
        [Tooltip("플레이어 섞기 모드")]
        public ConfigCommonTcg.ShuffleMode playerShuffleMode = ConfigCommonTcg.ShuffleMode.PureRandom;

        [Tooltip("플레이어 섞기 모드 설정 파일 (모드에 맞는 ScriptableObject를 지정)")]
        public ScriptableObject playerShuffleSettings;

        [Tooltip("적 덱 프리셋")]
        public EnemyDeckPreset enemyDeckPreset;
        [Tooltip("적 섞기 모드")]
        public ConfigCommonTcg.ShuffleMode enemyShuffleMode = ConfigCommonTcg.ShuffleMode.PureRandom;

        [Tooltip("적 섞기 모드 설정 파일 (모드에 맞는 ScriptableObject를 지정)")]
        public ScriptableObject enemyShuffleSettings;

        /// <summary>
        /// 플레이어/적에 따라 셔플 정책을 반환합니다.
        /// </summary>
        public (ConfigCommonTcg.ShuffleMode mode, ScriptableObject settings) GetShufflePolicy(ConfigCommonTcg.TcgPlayerSide side)
        {
            return side switch
            {
                ConfigCommonTcg.TcgPlayerSide.Player => (playerShuffleMode, playerShuffleSettings),
                ConfigCommonTcg.TcgPlayerSide.Enemy => (enemyShuffleMode, enemyShuffleSettings),
                _ => (ConfigCommonTcg.ShuffleMode.PureRandom, null)
            };
        }
        // ──────────────────────────────────────────────────────────────────────────────
        // 연출용. todo 분리 해야 함
        // ──────────────────────────────────────────────────────────────────────────────
        [Header("Move To Target")]
        [Tooltip("Player 영역이 하단일 경우, - 값으로 입력해야 합니다.")]
        public Vector3 moveToTargetLeftDownOffset;
        
        [Header("Hand To Grave")]
        public float handToGraveFadeOutDelayTime;
        public Easing.EaseType handToGraveFadeOutEasing;
        public float handToGraveFadeOutDuration;
        public readonly bool handToGraveUseBurnDissolve = false;
        
        [Header("Hand To Field")]
        public Easing.EaseType handToFieldFadeInEasing;
        public float handToFieldFadeInDuration;

        [Header("Attack Unit")] 
        [Tooltip("Player 영역이 하단일 경우, - 값으로 입력해야 합니다.")]
        public float attackUnitBackDistance;
        public Easing.EaseType attackUnitBackEasing;
        public float attackUnitBackDuration;
        public Easing.EaseType attackUnitHitEasing;
        public float attackUnitHitDuration;
        [Tooltip("공격자가 받는 데미지, 타겟이 받는 데미지 연출 사이의 간격 시간")]
        public float attackUnitShowDamageDiffDuration;
        
        public float GetAttackUnitBackDistance(ConfigCommonTcg.TcgPlayerSide side)
        {
            return side == ConfigCommonTcg.TcgPlayerSide.Player ? attackUnitBackDistance : attackUnitBackDistance * -1;
        }
        public Vector3 GetMoveToTargetLeftDownOffset(ConfigCommonTcg.TcgPlayerSide side)
        {
            return side == ConfigCommonTcg.TcgPlayerSide.Player ? moveToTargetLeftDownOffset : moveToTargetLeftDownOffset * -1;
        }
#if UNITY_EDITOR
        // ──────────────────────────────────────────────────────────────────────────────
        // 테스트용
        // ──────────────────────────────────────────────────────────────────────────────
        [Header("테스트용")]
        public int testSeed;
        public bool showDeckInfo;
        public bool showShuffleInfo;
        public bool showCardUid;

        [ContextMenu("Rebuild Cache & Raise Changed")]
        private void RebuildCacheAndNotify()
        {
            // 필요 시 내부 캐시 재빌드 로직 추가 지점
            Changed?.Invoke();
        }
#endif
    }
}
