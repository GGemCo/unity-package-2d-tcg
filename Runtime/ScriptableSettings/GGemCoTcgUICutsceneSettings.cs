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
        fileName = ConfigScriptableObjectTcg.TcgUICutsceneSettings.FileName,
        menuName = ConfigScriptableObjectTcg.TcgUICutsceneSettings.MenuName,
        order    = ConfigScriptableObjectTcg.TcgUICutsceneSettings.Ordering)]
    public class GGemCoTcgUICutsceneSettings : ScriptableObject, ISettingsChangeNotifier
    {
        // 런타임/에디터 공용 알림 (직렬화되지 않음)
        public event Action Changed;

#if UNITY_EDITOR
        // 인스펙터 값 변경 시 자동 검증 및 알림
        private void OnValidate()
        {
            Changed?.Invoke();
        }

#endif

        /// <summary>툴/코드에서 강제 알림</summary>
        public void RaiseChanged() => Changed?.Invoke();

        // ──────────────────────────────────────────────────────────────────────────────
        // 연출용.
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
    }
}
