using System.Collections;
using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 손패의 카드를 필드(보드) 슬롯으로 이동시키는 소환 연출 핸들러.
    /// </summary>
    public sealed class HandlerMoveCardHandToBoard : ITcgPresentationHandler
    {
        /// <summary>
        /// 이 핸들러가 처리하는 프레젠테이션 스텝 타입.
        /// </summary>
        public TcgPresentationStepType Type => TcgPresentationStepType.MoveCardHandToBoard;

        /// <summary>
        /// 손패 → 보드 이동 연출을 실행합니다.
        /// </summary>
        /// <param name="ctx">연출 실행 컨텍스트.</param>
        /// <param name="step">출발(손패) 인덱스, 도착(보드) 인덱스 및 보드 상태값 등이 포함된 스텝.</param>
        /// <returns>코루틴 이터레이터.</returns>
        public IEnumerator Play(TcgPresentationContext ctx, TcgPresentationStep step)
        {
            var handWindow = ctx.GetHandWindow(step.Side);
            var fieldWindow = ctx.GetFieldWindow(step.Side);
            if (handWindow == null || fieldWindow == null) yield break;

            // 핸드에서는 지워주기
            // Hand UI: 0번은 영웅, 실제 손패는 1번부터(그래서 +1 오프셋)
            var slot = handWindow.GetSlotByIndex(step.FromIndex + 1);
            if (slot)
            {
                slot.gameObject.SetActive(false);
            }

            // 이미 데이터는 변경되었으므로, Board에서 정보를 가져온다.
            int toIndex = step.ToIndex;
            var fromCard = step.Attacker.Board.GetByIndex(toIndex);
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

            var defaultFadeOption = UiFadeUtility.FadeOptions.Default;
            defaultFadeOption.easeType = fieldWindow.fadeInEasing;
            yield return UiFadeUtility.FadeIn(fieldWindow, targetSlot.gameObject, fieldWindow.fadeInDuration, defaultFadeOption);
                
            yield return new WaitForSeconds(0.05f);
        }
    }
}