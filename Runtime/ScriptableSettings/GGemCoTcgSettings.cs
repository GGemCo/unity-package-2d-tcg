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

            Changed?.Invoke();
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

        [Tooltip("가중치. (0은 제외)")] 
        public int[] costWeights;
        
        [Header("마나")]
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
        
#if UNITY_EDITOR
        // ──────────────────────────────────────────────────────────────────────────────
        // 테스트용
        // ──────────────────────────────────────────────────────────────────────────────
        [Header("테스트용")]
        public int testSeed;
        public EnemyDeckPreset testDeckPreset;
        public bool testMemoryProfile;
        
        [ContextMenu("Rebuild Cache & Raise Changed")]
        private void RebuildCacheAndNotify()
        {
            // 필요 시 내부 캐시 재빌드 로직 추가 지점
            Changed?.Invoke();
        }
#endif
    }
}
