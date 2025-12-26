using System.Collections;
using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 유닛이 상대 영웅을 공격하는 연출을 재생하는 핸들러.
    /// </summary>
    /// <remarks>
    /// 공격자 아이콘을 타겟 위치로 이동시킨 후 원위치로 복귀하고,
    /// 영웅 HP/데미지 UI를 갱신하며 사망 시 페이드아웃을 하고, 전투 종료 수행합니다.
    /// </remarks>
    public sealed class HandlerAttackHero : ITcgPresentationHandler
    {
        /// <summary>
        /// 이 핸들러가 처리하는 프레젠테이션 스텝 타입.
        /// </summary>
        public TcgPresentationStepType Type => TcgPresentationStepType.AttackHero;

        /// <summary>
        /// 공격(유닛 → 영웅) 연출을 실행합니다.
        /// </summary>
        /// <param name="ctx">연출 실행에 필요한 세션/윈도우 컨텍스트.</param>
        /// <param name="step">공격자/대상 인덱스 및 HP/데미지 값이 담긴 스텝.</param>
        /// <returns>코루틴 이터레이터.</returns>
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
            var targetHandWindow = ctx.GetHandWindow(attackerSide == ConfigCommonTcg.TcgPlayerSide.Player
                ? ConfigCommonTcg.TcgPlayerSide.Enemy
                : ConfigCommonTcg.TcgPlayerSide.Player);

            if (attackerFieldWindow == null || targetHandWindow == null) yield break;

            var attackerSlot = attackerFieldWindow.GetSlotByIndex(attackerIndex);
            var attackerIcon = attackerFieldWindow.GetIconByIndex(attackerIndex);
            
            var defenderSlot = targetHandWindow.GetSlotByIndex(defenderIndex);
            var defenderIcon = targetHandWindow.GetIconByIndex(defenderIndex);

            // UI가 아직 생성/동기화되지 않은 프레임일 수 있으므로 짧게 대기 후 종료
            if (attackerIcon == null || defenderIcon == null || attackerSlot == null)
            {
                yield return new WaitForSeconds(0.05f);
                yield break;
            }

            // 타겟 위치 계산(그리드 좌표 계산 실패 시 아이콘/트랜스폼 위치로 폴백)
            var iconHero = targetHandWindow.GetIconByIndex(0);
            if (iconHero == null)
            {
                GcLogger.LogError($"0번째 슬롯에 영웅 아이콘이 없습니다.");
                yield break;
            }
            Vector3 targetPos = iconHero.transform.position;

            var iconTr = attackerIcon.transform;

            // 이동 중 캔버스 정렬 문제를 피하기 위해 UI 루트로 일시 이동
            iconTr.SetParent(ctx.UIRoot, worldPositionStays: true);

            // ---- 1) 대상보다 조금 왼쪽 아래로 "바로" 이동 ----
            var snapPos = targetPos + attackerFieldWindow.leftDownOffset;
            iconTr.position = snapPos;

            // ---- 2) 뒤로 천천히 이동했다가 ----
            var backPos = snapPos + new Vector3(0, attackerFieldWindow.backDistance, 0);

            var defaultMoveOption = MoveOptions.Default;
            defaultMoveOption.easeType = attackerFieldWindow.backEasing;
            yield return UiMoveTransform.MoveTo(attackerFieldWindow, iconTr, backPos,
                attackerFieldWindow.backDuration, defaultMoveOption);

            // ---- 3) 빠른 속도로 상대 카드를 치는 듯한 느낌 ----
            defaultMoveOption = MoveOptions.Default;
            defaultMoveOption.easeType = attackerFieldWindow.hitEasing;
            yield return UiMoveTransform.MoveTo(attackerFieldWindow, iconTr, snapPos,
                attackerFieldWindow.hitDuration, defaultMoveOption);

            // 잠시 대기 후 원복
            yield return new WaitForSeconds(0.1f);
            iconTr.SetParent(attackerSlot.transform, worldPositionStays: false);
            iconTr.localPosition = Vector3.zero;

            // 체력/데미지 표시
            if (defenderIcon is UIIconCard defenderCardIcon)
                defenderCardIcon.UpdateHealth(targetHp, targetDamage);

            // 사망 페이드 아웃
            if (targetHp <= 0)
            {
                var defaultFadeOption = UiFadeUtility.FadeOptions.Default;
                defaultFadeOption.easeType = attackerFieldWindow.fadeOutEasing;
                yield return UiFadeUtility.FadeOut(attackerFieldWindow, defenderSlot.gameObject, attackerFieldWindow.fadeOutDuration, defaultFadeOption);
                
                yield return new WaitForSeconds(1.0f);
            }
        }
    }
}
