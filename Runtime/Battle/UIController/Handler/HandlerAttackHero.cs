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
            var attackerSide = step.Side;
            var attackerIndex = step.FromIndex;
            var defenderIndex = step.ToIndex;

            int attackerHp = step.ValueA;
            int targetHp   = step.ValueB;
            int attackerDamage = step.ValueC;
            int targetDamage   = step.ValueD;

            var attackerFieldWindow = ctx.GetFieldWindow(attackerSide);
            var targetHandWindow = ctx.GetHandWindow(attackerSide == ConfigCommonTcg.TcgPlayerSide.Player
                ? ConfigCommonTcg.TcgPlayerSide.Enemy
                : ConfigCommonTcg.TcgPlayerSide.Player);

            if (attackerFieldWindow == null || targetHandWindow == null) yield break;

            var attackerSlot = attackerFieldWindow.GetSlotByIndex(attackerIndex);
            var attackerIcon = attackerFieldWindow.GetIconByIndex(attackerIndex);
            var defenderIcon = targetHandWindow.GetIconByIndex(defenderIndex);

            // UI가 아직 생성/동기화되지 않은 프레임일 수 있으므로 짧게 대기 후 종료
            if (attackerIcon == null || defenderIcon == null || attackerSlot == null)
            {
                yield return new WaitForSecondsRealtime(0.05f);
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

            // 이동 중 정렬/가림 방지를 위해 UI 루트로 일시 이동(월드 좌표 유지)
            attackerIcon.transform.SetParent(ctx.UIRoot);

            yield return TcgUiTween.MoveTo(attackerIcon.transform, targetPos - new Vector3(20, 20), attackerFieldWindow.timeToMove);

            // 이동 연출 종료 후 원래 부모로 복귀 및 로컬 위치 리셋
            attackerIcon.transform.SetParent(attackerSlot.transform);
            attackerIcon.transform.localPosition = Vector3.zero;

            // 체력/데미지 표시 갱신(UIIconCard일 때만 업데이트)
            if (defenderIcon is UIIconCard defenderCardIcon)
                defenderCardIcon.UpdateHealth(targetHp, targetDamage);

            if (targetHp <= 0)
            {
                
            }
        }

        /// <summary>
        /// 아이콘에 <see cref="CanvasGroup"/>이 존재할 경우, 지정 시간 동안 알파를 0으로 페이드아웃합니다.
        /// </summary>
        /// <param name="icon">페이드아웃 대상 컴포넌트(아이콘).</param>
        /// <param name="duration">페이드아웃 시간(초).</param>
        private static IEnumerator FadeOutIfPossible(Component icon, float duration)
        {
            var cg = icon.GetComponent<CanvasGroup>();
            if (cg == null) yield break;
            yield return TcgUiTween.FadeTo(cg, 0f, duration);
        }
    }
}
