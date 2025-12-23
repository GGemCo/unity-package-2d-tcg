using System.Collections;
using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 유닛이 상대 유닛을 공격하는 연출을 재생하는 핸들러.
    /// </summary>
    /// <remarks>
    /// 1) 대상보다 조금 왼쪽 아래로 즉시 이동
    /// 2) 뒤로 천천히 이동
    /// 3) 빠르게 대상 위치로 타격 이동
    /// HP/데미지 UI 갱신 및 사망 시 페이드아웃을 수행합니다.
    /// </remarks>
    public sealed class HandlerAttackUnit : ITcgPresentationHandler
    {
        public TcgPresentationStepType Type => TcgPresentationStepType.AttackUnit;

        public IEnumerator Play(TcgPresentationContext ctx, TcgPresentationStep step)
        {
            var attackerSide  = step.Side;
            var attackerIndex = step.FromIndex;
            var defenderIndex = step.ToIndex;

            int attackerHp     = step.ValueA;
            int targetHp       = step.ValueB;
            int attackerDamage = step.ValueC;
            int targetDamage   = step.ValueD;

            var attackerFieldWindow = ctx.GetFieldWindow(attackerSide);
            var defenderField = ctx.GetFieldWindow(attackerSide == ConfigCommonTcg.TcgPlayerSide.Player
                ? ConfigCommonTcg.TcgPlayerSide.Enemy
                : ConfigCommonTcg.TcgPlayerSide.Player);

            if (attackerFieldWindow == null || defenderField == null) yield break;

            var attackerSlot = attackerFieldWindow.GetSlotByIndex(attackerIndex);
            var attackerIcon = attackerFieldWindow.GetIconByIndex(attackerIndex);
            
            var defenderSlot = defenderField.GetSlotByIndex(defenderIndex);
            var defenderIcon = defenderField.GetIconByIndex(defenderIndex);

            if (attackerIcon == null || defenderIcon == null || attackerSlot == null)
            {
                yield return new WaitForSeconds(0.05f);
                yield break;
            }

            Vector3 targetPos = defenderIcon.transform.position;

            var iconTr = attackerIcon.transform;

            // 이동 중 캔버스 정렬 문제를 피하기 위해 UI 루트로 일시 이동
            iconTr.SetParent(ctx.UIRoot, worldPositionStays: true);
            
            // 슬롯은 안보이게 
            yield return FadeOutIfPossible(attackerSlot, 0);
            
            // ---- 1) 대상보다 조금 왼쪽 아래로 "바로" 이동 ----
            var snapPos = targetPos + attackerFieldWindow.leftDownOffset;
            iconTr.position = snapPos;

            // ---- 2) 뒤로 천천히 이동했다가 ----
            var backPos = snapPos + new Vector3(0, attackerFieldWindow.backDistance, 0);

            yield return TcgUiTween.MoveTo(iconTr, backPos, attackerFieldWindow.backDuration, attackerFieldWindow.backEasing);

            // ---- 3) 빠른 속도로 상대 카드를 치는 듯한 느낌 ----
            yield return TcgUiTween.MoveTo(iconTr, snapPos, attackerFieldWindow.hitDuration, attackerFieldWindow.hitEasing);

            yield return new WaitForSeconds(0.1f);
            // 원복
            yield return FadeInIfPossible(attackerSlot, 0);
            iconTr.SetParent(attackerSlot.transform, worldPositionStays: false);
            iconTr.localPosition = Vector3.zero;

            // 체력/데미지 표시
            if (attackerIcon is UIIconCard attackerCardIcon)
                attackerCardIcon.UpdateHealth(attackerHp, attackerDamage);

            bool isFadeOut = false;
            if (attackerHp <= 0)
            {
                yield return new WaitForSeconds(attackerFieldWindow.fadeOutDelayTime);
                yield return FadeOutIfPossible(attackerSlot, attackerFieldWindow.fadeOutDuration, attackerFieldWindow.fadeOutEasing);
                // attackerSlot.gameObject.SetActive(false);
                isFadeOut = true;
            }

            if (defenderIcon is UIIconCard defenderCardIcon)
                defenderCardIcon.UpdateHealth(targetHp, targetDamage);

            if (targetHp <= 0)
            {
                yield return new WaitForSeconds(attackerFieldWindow.fadeOutDelayTime);
                yield return FadeOutIfPossible(defenderSlot, attackerFieldWindow.fadeOutDuration, attackerFieldWindow.fadeOutEasing);
                // defenderSlot.gameObject.SetActive(false);
                isFadeOut = true;
            }
            // 사망한 카드가 없으면, 조금 텀을 주기 위해서 fade out 시간만큼 대기한다.
            if (!isFadeOut)
            {
                yield return new WaitForSeconds(attackerFieldWindow.fadeOutDuration);
            }
        }

        private static IEnumerator FadeOutIfPossible(Component icon, float duration, Easing.EaseType easeType = Easing.EaseType.Linear)
        {
            var cg = icon.GetComponent<CanvasGroup>();
            if (cg == null)
            {
                // CanvasGroup이 없으면 안전하게 즉시 비활성화로 처리
                icon.gameObject.SetActive(false);
                yield return new WaitForSeconds(0.05f);
                yield break;
            }
            yield return TcgUiTween.FadeTo(cg, 1f, 0f, duration, easeType);
        }

        private static IEnumerator FadeInIfPossible(Component icon, float duration, Easing.EaseType easeType = Easing.EaseType.Linear)
        {
            var cg = icon.GetComponent<CanvasGroup>();
            if (cg == null)
            {
                // CanvasGroup이 없으면 안전하게 즉시 활성화로 처리
                GcLogger.LogError($"슬로 프리팹의 UISlot.UseCanvasGroup을 활성화 해주세요.");
                icon.gameObject.SetActive(true);
                yield return new WaitForSeconds(0.05f);
                yield break;
            }
            yield return TcgUiTween.FadeTo(cg, 0f, 1f, duration, easeType);
        }
    }
}
