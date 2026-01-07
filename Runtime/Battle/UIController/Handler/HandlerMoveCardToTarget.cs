using System.Collections;
using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    public class HandlerMoveCardToTarget : HandlerBase, ITcgPresentationHandler
    {
        /// <summary>
        /// 이 핸들러가 처리하는 프레젠테이션 스텝 타입.
        /// </summary>
        public TcgPresentationConstants.TcgPresentationStepType Type => TcgPresentationConstants.TcgPresentationStepType.MoveCardToTarget;
        
        public IEnumerator Play(TcgPresentationContext ctx, TcgPresentationStep step)
        {
            var attackerSide  = step.Side;
            var attackerZone = step.FromZone;
            var attackerIndex = step.FromIndex;
            var defenderZone = step.ToZone;
            var defenderIndex = step.ToIndex;

            var attackerWindow = ctx.GetUIWindow(attackerZone);
            if (attackerWindow == null) yield break;
            
            var defenderWindow = ctx.GetUIWindow(defenderZone);
            if (defenderWindow == null) yield break;

            UISlot attackerSlot = attackerWindow.GetSlotByIndex(attackerIndex);
            UIIcon attackerIcon = attackerWindow.GetIconByIndex(attackerIndex);
            
            UISlot defenderSlot = defenderWindow.GetSlotByIndex(defenderIndex);
            UIIcon defenderIcon = defenderWindow.GetIconByIndex(defenderIndex);
            
            if (attackerSlot == null || attackerIcon == null || defenderSlot == null || defenderIcon == null)
            {
                yield return new WaitForSeconds(0.05f);
                yield break;
            }

            var iconTr = attackerIcon.transform;

            // 이동 중 캔버스 정렬 문제를 피하기 위해 UI 루트로 일시 이동
            iconTr.SetParent(ctx.UIRoot, worldPositionStays: true);
            
            // 슬롯은 안보이게 
            yield return UiFadeUtility.FadeOutImmediately(attackerWindow, attackerSlot.gameObject);
            
            // 대상보다 조금 왼쪽 아래로 "바로" 이동 
            var snapPos = defenderIcon.transform.position + ctx.UICutsceneSettings.moveToTargetLeftDownOffset;
            iconTr.position = snapPos;
        }
    }
}