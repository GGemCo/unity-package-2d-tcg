using System.Collections;
using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// Ability 기반 회복(힐) 팝업/이펙트 연출 핸들러.
    /// </summary>
    public sealed class HandlerAbilityHeal : HandlerBase, ITcgPresentationHandler
    {
        public TcgPresentationConstants.TcgPresentationStepType Type =>
            TcgPresentationConstants.TcgPresentationStepType.HealPopup;

        public IEnumerator Play(TcgPresentationContext ctx, TcgPresentationStep step)
        {
            var attackerSide  = step.Side;
            var attackerZone = step.FromZone;
            var attackerIndex = step.FromIndex;
            var defenderZone = step.ToZone;
            var defenderIndex = step.ToIndex;

            var attackerWindow = ctx.GetUIWindow(attackerZone);
            var defenderWindow = ctx.GetUIWindow(defenderZone);
            if (attackerWindow == null || defenderWindow == null) yield break;

            UISlot attackerSlot = attackerWindow.GetSlotByIndex(attackerIndex);
            UIIcon attackerIcon = attackerWindow.GetIconByIndex(attackerIndex);
            
            UISlot defenderSlot = defenderWindow.GetSlotByIndex(defenderIndex);
            UIIcon defenderIcon = defenderWindow.GetIconByIndex(defenderIndex);
            
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
            yield return UiFadeUtility.FadeOutImmediately(attackerWindow, attackerSlot.gameObject);
            
            if (step.Payload is not TcgAbilityPayloadHeal payload) yield break;
            if (payload.HealValue <= 0) yield break;
            
            ShowDamageText(attackerIcon, payload.HealValue);
            ShowEffect(attackerIcon, EffectUidHeal);
        }
    }
}
