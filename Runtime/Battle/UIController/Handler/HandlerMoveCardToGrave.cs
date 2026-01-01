using System.Collections;
using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 사용한 자리에서 fade out
    /// </summary>
    public sealed class HandlerMoveCardToGrave : ITcgPresentationHandler
    {
        public TcgPresentationConstants.TcgPresentationStepType Type => TcgPresentationConstants.TcgPresentationStepType.MoveCardToGrave;

        public IEnumerator Play(TcgPresentationContext ctx, TcgPresentationStep step)
        {
            var attackerSide  = step.Side;
            var attackerZone = step.FromZone;
            var attackerIndex = step.FromIndex;
            
            var attackerWindow = ctx.GetUIWindow(attackerZone);
            if (attackerWindow == null) yield break;

            UISlot attackerSlot = attackerWindow.GetSlotByIndex(attackerIndex);
            UIIcon attackerIcon = attackerWindow.GetIconByIndex(attackerIndex);
            
            if (attackerIcon == null || attackerSlot == null)
            {
                yield return new WaitForSeconds(0.05f);
                yield break;
            }
            
            yield return new WaitForSeconds(ctx.Settings.handToGraveFadeOutDelayTime);

            if (ctx.Settings.handToGraveUseBurnDissolve)
            {
                var uiBurnDissolvePlayer = attackerIcon.GetComponent<UiBurnDissolvePlayer>();
                if (uiBurnDissolvePlayer != null)
                {
                    yield return uiBurnDissolvePlayer.CoPlay();
                }
            }
            else
            {
                var defaultFadeOption = UiFadeUtility.FadeOptions.Default;
                defaultFadeOption.easeType = ctx.Settings.handToGraveFadeOutEasing;
                defaultFadeOption.startAlpha = 1f;
                yield return UiFadeUtility.FadeOut(attackerWindow, attackerIcon.gameObject, ctx.Settings.handToGraveFadeOutDuration, defaultFadeOption);
            }

            var iconTr = attackerIcon.transform;
            iconTr.SetParent(attackerSlot.transform, worldPositionStays: false);
            iconTr.localPosition = Vector3.zero;
            yield return UiFadeUtility.FadeOutImmediately(attackerWindow, attackerSlot.gameObject);
        }
    }
}
