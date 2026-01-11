using System;
using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// TCG UI 컷신/연출에 필요한 전역 설정을 보관하는 ScriptableObject입니다.
    /// </summary>
    /// <remarks>
    /// 포함 항목:
    /// - 타겟으로 이동 시 오프셋
    /// - Hand → Grave 페이드 아웃 연출(딜레이/이징/지속시간/디졸브 여부)
    /// - Hand → Field 페이드 인 연출(이징/지속시간)
    /// - 유닛 공격 연출(백스텝/히트/데미지 표시 간격)
    /// - 설정 변경 알림(런타임/에디터)
    /// </remarks>
    [CreateAssetMenu(
        fileName = ConfigScriptableObjectTcg.TcgUICutsceneSettings.FileName,
        menuName = ConfigScriptableObjectTcg.TcgUICutsceneSettings.MenuName,
        order    = ConfigScriptableObjectTcg.TcgUICutsceneSettings.Ordering)]
    public class GGemCoTcgUICutsceneSettings : ScriptableObject, ISettingsChangeNotifier
    {
        /// <summary>
        /// 설정 값이 변경되었음을 알리는 이벤트입니다. (직렬화되지 않음)
        /// </summary>
        public event Action Changed;

#if UNITY_EDITOR
        /// <summary>
        /// 에디터에서 인스펙터 값이 변경될 때 호출되어 변경 알림을 발생시킵니다.
        /// </summary>
        private void OnValidate()
        {
            Changed?.Invoke();
        }
#endif

        /// <summary>
        /// 코드/툴에서 강제로 <see cref="Changed"/> 알림을 발생시킵니다.
        /// </summary>
        public void RaiseChanged() => Changed?.Invoke();

        // ──────────────────────────────────────────────────────────────────────────────
        // UI 연출
        // ──────────────────────────────────────────────────────────────────────────────
        /// <summary>
        /// UI 연출 관련 타이밍 설정입니다.
        /// </summary>
        [Header("UI 연출")]
        [Tooltip("한개의 Command 종료 후 대기 시간")]
        [Min(0)]
        public float timeWaitAfterCommand = 0.0f;

        // ──────────────────────────────────────────────────────────────────────────────
        // 연출용
        // ──────────────────────────────────────────────────────────────────────────────

        /// <summary>
        /// 카드/오브젝트를 타겟 방향으로 이동시킬 때 사용하는 오프셋입니다.
        /// </summary>
        /// <remarks>
        /// Player 영역이 하단인 UI 레이아웃에서는 음수 방향 값을 사용하도록 설계되었습니다.
        /// </remarks>
        [Header("Move To Target")]
        [Tooltip("Player 영역이 하단일 경우, - 값으로 입력해야 합니다.")]
        public Vector3 moveToTargetLeftDownOffset;

        /// <summary>
        /// Hand → Grave 연출(페이드 아웃) 관련 설정입니다.
        /// </summary>
        [Header("Hand To Grave")]
        public float handToGraveFadeOutDelayTime;

        /// <summary>
        /// Hand → Grave 페이드 아웃 이징 타입입니다.
        /// </summary>
        public Easing.EaseType handToGraveFadeOutEasing;

        /// <summary>
        /// Hand → Grave 페이드 아웃 지속 시간(초)입니다.
        /// </summary>
        public float handToGraveFadeOutDuration;

        /// <summary>
        /// Hand → Grave 이동 시 Burn Dissolve 효과 사용 여부입니다.
        /// </summary>
        /// <remarks>
        /// readonly로 선언되어 있으므로 인스펙터에서 변경되지 않으며, 코드 상 기본값은 false입니다.
        /// </remarks>
        public readonly bool handToGraveUseBurnDissolve = false;

        /// <summary>
        /// Hand → Field 연출(페이드 인) 관련 설정입니다.
        /// </summary>
        [Header("Hand To Field")]
        public Easing.EaseType handToFieldFadeInEasing;

        /// <summary>
        /// Hand → Field 페이드 인 지속 시간(초)입니다.
        /// </summary>
        public float handToFieldFadeInDuration;

        /// <summary>
        /// 유닛 공격(Attack Unit) 연출 관련 설정입니다.
        /// </summary>
        [Header("Attack Unit")]
        [Tooltip("Player 영역이 하단일 경우, - 값으로 입력해야 합니다.")]
        public float attackUnitBackDistance;

        /// <summary>
        /// 공격 전 “뒤로 젖히기(백스텝)” 이징 타입입니다.
        /// </summary>
        public Easing.EaseType attackUnitBackEasing;

        /// <summary>
        /// 공격 전 “뒤로 젖히기(백스텝)” 지속 시간(초)입니다.
        /// </summary>
        public float attackUnitBackDuration;

        /// <summary>
        /// 피격(Hit) 연출 이징 타입입니다.
        /// </summary>
        public Easing.EaseType attackUnitHitEasing;

        /// <summary>
        /// 피격(Hit) 연출 지속 시간(초)입니다.
        /// </summary>
        public float attackUnitHitDuration;

        /// <summary>
        /// 공격자가 받는 데미지 연출과 타겟이 받는 데미지 연출 사이의 간격 시간(초)입니다.
        /// </summary>
        [Tooltip("공격자가 받는 데미지, 타겟이 받는 데미지 연출 사이의 간격 시간")]
        public float attackUnitShowDamageDiffDuration;

        /// <summary>
        /// 진영에 따라 공격 유닛 백스텝 거리의 방향을 보정하여 반환합니다.
        /// </summary>
        /// <param name="side">플레이어/적 진영 구분입니다.</param>
        /// <returns>
        /// Player 진영이면 <see cref="attackUnitBackDistance"/>를 그대로,
        /// Enemy 진영이면 방향을 반전한 값을 반환합니다.
        /// </returns>
        public float GetAttackUnitBackDistance(ConfigCommonTcg.TcgPlayerSide side)
        {
            return side == ConfigCommonTcg.TcgPlayerSide.Player ? attackUnitBackDistance : attackUnitBackDistance * -1;
        }

        /// <summary>
        /// 진영에 따라 타겟 이동 오프셋의 방향을 보정하여 반환합니다.
        /// </summary>
        /// <param name="side">플레이어/적 진영 구분입니다.</param>
        /// <returns>
        /// Player 진영이면 <see cref="moveToTargetLeftDownOffset"/>을 그대로,
        /// Enemy 진영이면 벡터 방향을 반전한 값을 반환합니다.
        /// </returns>
        public Vector3 GetMoveToTargetLeftDownOffset(ConfigCommonTcg.TcgPlayerSide side)
        {
            return side == ConfigCommonTcg.TcgPlayerSide.Player ? moveToTargetLeftDownOffset : moveToTargetLeftDownOffset * -1;
        }
    }
}
