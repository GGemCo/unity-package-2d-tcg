using System.Collections;
using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 손패에서 사용된 카드를 "소모(Grave)" 처리하는 기본 연출 핸들러.
    /// - 현재 패키지에는 별도의 Grave UI가 없으므로,
    ///   손패 슬롯을 즉시 비활성화하는 최소 연출만 제공합니다.
    /// </summary>
    public sealed class HandlerMoveCardHandToGrave : ITcgPresentationHandler
    {
        public TcgPresentationStepType Type => TcgPresentationStepType.MoveCardHandToGrave;

        public IEnumerator Play(TcgPresentationContext ctx, TcgPresentationStep step)
        {
            var handWindow = ctx.GetHandWindow(step.Side);
            if (handWindow == null) yield break;

            // Hand UI: 0번은 영웅, 실제 손패는 1번부터(그래서 +1 오프셋)
            var slot = handWindow.GetSlotByIndex(step.FromIndex + 1);
            if (slot)
            {
                slot.gameObject.SetActive(false);
            }

            yield return null;
        }
    }
}
