using System.Collections;
using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 연출 중 분리/이동된 필드 카드 아이콘을 원래 슬롯(부모) 위치로 되돌리는 프리젠테이션 핸들러.
    /// </summary>
    /// <remarks>
    /// 공격/능력 연출 과정에서 아이콘을 UI 루트로 옮기고 슬롯을 숨기는 경우가 있으므로,
    /// 해당 아이콘을 슬롯 자식으로 복원하고 슬롯을 다시 표시한다.
    /// </remarks>
    public sealed class HandlerMoveCardToBack : ITcgPresentationHandler
    {
        /// <summary>
        /// 이 핸들러가 처리하는 프리젠테이션 스텝 타입.
        /// </summary>
        public TcgPresentationConstants.TcgPresentationStepType Type =>
            TcgPresentationConstants.TcgPresentationStepType.MoveCardToBack;

        /// <summary>
        /// 아이콘을 원래 슬롯 위치로 복원하고 슬롯 표시 상태를 되돌린다.
        /// </summary>
        /// <param name="ctx">UI 윈도우 조회 및 컷신 설정을 제공하는 프리젠테이션 컨텍스트.</param>
        /// <param name="step">복원 대상(존/인덱스)을 포함하는 스텝.</param>
        /// <returns>코루틴 이터레이터.</returns>
        /// <remarks>
        /// 필요한 UI 오브젝트(윈도우/슬롯/아이콘)를 찾지 못하면 안전하게 조기 종료한다.
        /// </remarks>
        public IEnumerator Play(TcgPresentationContext ctx, TcgPresentationStep step)
        {
            var attackerSide = step.Side;
            var attackerZone = step.FromZone;
            var attackerIndex = step.FromIndex;

            var attackerWindow = ctx.GetUIWindow(attackerZone);
            if (attackerWindow == null) yield break;

            UISlot attackerSlot = attackerWindow.GetSlotByIndex(attackerIndex);
            UIIcon attackerIcon = attackerWindow.GetIconByIndex(attackerIndex);

            // UI가 아직 생성/동기화되지 않은 프레임일 수 있으므로 짧게 대기 후 종료
            if (attackerIcon == null || attackerSlot == null)
            {
                yield return new WaitForSeconds(0.05f);
                yield break;
            }

            // 다른 연출(공격/전환 등)의 마무리 타이밍에 맞춰 약간 지연 후 복원한다.
            yield return new WaitForSeconds(ctx.UICutsceneSettings.handToGraveFadeOutDelayTime);

            // 아이콘을 슬롯 자식으로 되돌리고 슬롯 기준 정렬 위치로 복원한다.
            var iconTr = attackerIcon.transform;
            iconTr.SetParent(attackerSlot.transform, worldPositionStays: false);
            iconTr.localPosition = Vector3.zero;

            // 숨겨두었던 슬롯을 즉시 표시한다.
            yield return UiFadeUtility.FadeInImmediately(attackerWindow, attackerSlot.gameObject);
        }
    }
}
