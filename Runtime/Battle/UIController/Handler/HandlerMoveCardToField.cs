using System.Collections;
using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 손패의 카드를 필드(보드) 슬롯으로 이동시키는 소환 연출 핸들러.
    /// </summary>
    public sealed class HandlerMoveCardToField : ITcgPresentationHandler
    {
        /// <summary>
        /// 이 핸들러가 처리하는 프레젠테이션 스텝 타입.
        /// </summary>
        public TcgPresentationConstants.TcgPresentationStepType Type => TcgPresentationConstants.TcgPresentationStepType.MoveCardToField;

        /// <summary>
        /// 손패 → 보드 이동 연출을 실행합니다.
        /// </summary>
        /// <param name="ctx">연출 실행 컨텍스트.</param>
        /// <param name="step">출발(손패) 인덱스, 도착(보드) 인덱스 및 보드 상태값 등이 포함된 스텝.</param>
        /// <returns>코루틴 이터레이터.</returns>
        public IEnumerator Play(TcgPresentationContext ctx, TcgPresentationStep step)
        {
            var attackerSide  = step.Side;
            var attackerZone = step.FromZone;
            var attackerIndex = step.FromIndex;
            
            var targetZone = step.ToZone;
            var targetIndex = step.ToIndex;
            
            var handWindow = ctx.GetUIWindow(attackerZone);
            var fieldWindow = ctx.GetUIWindow(targetZone);
            if (handWindow == null || fieldWindow == null) yield break;

            // 핸드에서는 지워주기
            var slot = handWindow.GetSlotByIndex(step.FromIndex);
            if (slot)
            {
                slot.gameObject.SetActive(false);
            }

            // 이미 데이터는 변경되었으므로, Field에서 정보를 가져온다.
            int toIndex = step.ToIndex;
            var casterDataSide = ctx.Session.GetSideState(step.Side);
            var fromCard = casterDataSide.GetBattleDataCardInFieldByIndex(toIndex);
            var uiIcon = fieldWindow.SetIconCount(toIndex, fromCard.Uid, 1);
            if (!uiIcon) {
                yield return new WaitForSeconds(0.05f);
                yield break;
            }
            var uiIconCard = uiIcon.GetComponent<UIIconCard>();
            if (uiIconCard != null)
            {
                uiIconCard.UpdateAttack(fromCard.Attack);
                uiIconCard.UpdateHealth(fromCard.Health);
            }
            var targetSlot = fieldWindow.GetSlotByIndex(toIndex);
            if (targetSlot != null)
            {
                var defaultFadeOption = UiFadeUtility.FadeOptions.Default;
                defaultFadeOption.easeType = ctx.Settings.handToFieldFadeInEasing;
                yield return UiFadeUtility.FadeIn(fieldWindow, targetSlot.gameObject, ctx.Settings.handToFieldFadeInDuration, defaultFadeOption);
            }
            
            yield return new WaitForSeconds(0.05f);
        }
    }
}