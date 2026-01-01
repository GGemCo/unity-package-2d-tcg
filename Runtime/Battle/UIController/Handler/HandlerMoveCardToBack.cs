using System.Collections;
using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 필드에 있는 카드 원래 자리로 되돌리기
    /// </summary>
    public sealed class HandlerMoveCardToBack : ITcgPresentationHandler
    {
        public TcgPresentationConstants.TcgPresentationStepType Type => TcgPresentationConstants.TcgPresentationStepType.MoveCardToBack;

        public IEnumerator Play(TcgPresentationContext ctx, TcgPresentationStep step)
        {
            var attackerSide  = step.Side;
            var attackerZone = step.FromZone;
            var attackerIndex = step.FromIndex;
            var attackerWindow = ctx.GetUIWindow(attackerZone);
            
            UISlot attackerSlot = attackerWindow.GetSlotByIndex(attackerIndex);
            UIIcon attackerIcon = attackerWindow.GetIconByIndex(attackerIndex);
            
            if (attackerIcon == null || attackerSlot == null)
            {
                yield return new WaitForSeconds(0.05f);
                yield break;
            }
            
            yield return new WaitForSeconds(ctx.Settings.handToGraveFadeOutDelayTime);
            
            var iconTr = attackerIcon.transform;
            iconTr.SetParent(attackerSlot.transform, worldPositionStays: false);
            iconTr.localPosition = Vector3.zero;
            
            // 슬롯 보이게 
            yield return UiFadeUtility.FadeInImmediately(attackerWindow, attackerSlot.gameObject);
        }
    }
}
