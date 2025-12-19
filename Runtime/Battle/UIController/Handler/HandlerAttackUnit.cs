using System.Collections;
using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 유닛이 상대 유닛을 공격하는 연출을 재생하는 핸들러.
    /// </summary>
    /// <remarks>
    /// 공격자 아이콘을 대상 위치로 이동 → 복귀,
    /// HP/데미지 UI 갱신 및 사망 시 페이드아웃을 수행합니다.
    /// </remarks>
    public sealed class HandlerAttackUnit : ITcgPresentationHandler
    {
        /// <summary>
        /// 이 핸들러가 처리하는 프레젠테이션 스텝 타입.
        /// </summary>
        public TcgPresentationStepType Type => TcgPresentationStepType.AttackUnit;

        /// <summary>
        /// 공격(유닛 ↔ 유닛) 연출을 실행합니다.
        /// </summary>
        /// <param name="ctx">연출 실행에 필요한 세션/윈도우 컨텍스트.</param>
        /// <param name="step">공격자/대상 인덱스 및 HP/데미지 값이 담긴 스텝.</param>
        /// <returns>코루틴 이터레이터.</returns>
        public IEnumerator Play(TcgPresentationContext ctx, TcgPresentationStep step)
        {
            var attackerSide = step.Side;
            var attackerIndex = step.FromIndex;
            var defenderIndex = step.ToIndex;

            int attackerHp = step.ValueA;
            int targetHp   = step.ValueB;
            int attackerDamage = step.ValueC;
            int targetDamage   = step.ValueD;

            var attackerField = ctx.GetFieldWindow(attackerSide);
            var defenderField = ctx.GetFieldWindow(attackerSide == ConfigCommonTcg.TcgPlayerSide.Player
                ? ConfigCommonTcg.TcgPlayerSide.Enemy
                : ConfigCommonTcg.TcgPlayerSide.Player);

            if (attackerField == null || defenderField == null) yield break;

            var attackerSlot = attackerField.GetSlotByIndex(attackerIndex);
            var attackerIcon = attackerField.GetIconByIndex(attackerIndex);
            var defenderIcon = defenderField.GetIconByIndex(defenderIndex);

            // UI가 아직 준비되지 않은 경우를 고려해 짧게 대기 후 종료
            if (attackerIcon == null || defenderIcon == null || attackerSlot == null)
            {
                yield return new WaitForSecondsRealtime(0.05f);
                yield break;
            }

            // 타겟 위치 계산(그리드 좌표 계산 실패 시 아이콘 위치로 폴백)
            var defenderGrid = defenderField.containerIcon;
            int defenderChildCount = defenderField.GetActiveIconCount();
            Vector3 targetPos;
            if (!GridLayoutPositionUtility.TryGetCellTransformPosition(defenderGrid, defenderIndex, defenderChildCount, out targetPos))
                targetPos = defenderIcon.transform.position;

            // 이동 중 캔버스 정렬 문제를 피하기 위해 UI 루트로 일시 이동
            attackerIcon.transform.SetParent(ctx.UIRoot);

            yield return TcgUiTween.MoveTo(attackerIcon.transform, targetPos - new Vector3(20, 20), attackerField.timeToMove);

            // 원복
            attackerIcon.transform.SetParent(attackerSlot.transform);
            attackerIcon.transform.localPosition = Vector3.zero;

            // 체력/데미지 표시
            if (attackerIcon is UIIconCard attackerCardIcon)
                attackerCardIcon.UpdateHealth(attackerHp, attackerDamage);

            // 사망 FadeOut
            if (attackerHp <= 0)
                yield return FadeOutIfPossible(attackerIcon, attackerField.timeToFadeOut);
            
            if (defenderIcon is UIIconCard defenderCardIcon)
                defenderCardIcon.UpdateHealth(targetHp, targetDamage);

            if (targetHp <= 0)
                yield return FadeOutIfPossible(defenderIcon, defenderField.timeToFadeOut);
        }

        /// <summary>
        /// <see cref="CanvasGroup"/>이 존재할 때만 페이드아웃을 수행합니다.
        /// </summary>
        /// <param name="icon">대상 아이콘.</param>
        /// <param name="duration">페이드아웃 시간(초).</param>
        private static IEnumerator FadeOutIfPossible(Component icon, float duration)
        {
            var cg = icon.GetComponent<CanvasGroup>();
            if (cg == null) yield break;
            yield return TcgUiTween.FadeTo(cg, 0f, duration);
        }
    }
}
