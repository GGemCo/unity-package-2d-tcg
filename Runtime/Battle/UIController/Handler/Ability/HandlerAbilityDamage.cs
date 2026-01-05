using System.Collections;
using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// Ability 기반 피해(데미지) 팝업/이펙트 연출 핸들러.
    /// </summary>
    public sealed class HandlerAbilityDamage : HandlerBase, ITcgPresentationHandler
    {
        public TcgPresentationConstants.TcgPresentationStepType Type =>
            TcgPresentationConstants.TcgPresentationStepType.AbilityDamage;

        public IEnumerator Play(TcgPresentationContext ctx, TcgPresentationStep step)
        {
            var attackerSide  = step.Side;
            var attackerZone = step.FromZone;
            var attackerIndex = step.FromIndex;
            var defenderZone = step.ToZone;
            var defenderIndex = step.ToIndex;

            var attackerHandWindow = ctx.GetUIWindow(attackerZone);
            var defenderFieldWindow = ctx.GetUIWindow(defenderZone);
            if (attackerHandWindow == null || defenderFieldWindow == null) yield break;

            UISlot attackerSlot = attackerIndex >= 0 ? attackerHandWindow.GetSlotByIndex(attackerIndex) : null;
            UIIcon attackerIcon = attackerIndex >= 0 ? attackerHandWindow.GetIconByIndex(attackerIndex) : null;
            
            UISlot defenderSlot = defenderFieldWindow.GetSlotByIndex(defenderIndex);
            UIIcon defenderIcon = defenderFieldWindow.GetIconByIndex(defenderIndex);

            // 공격자는 없을 수 있다. Permanent 타입
            if (attackerIcon != null && attackerSlot != null)
            {
                Vector3 targetPos = defenderIcon.transform.position;

                var iconTr = attackerIcon.transform;

                // 이동 중 캔버스 정렬 문제를 피하기 위해 UI 루트로 일시 이동
                iconTr.SetParent(ctx.UIRoot, worldPositionStays: true);
            
                // 슬롯은 안보이게 
                yield return UiFadeUtility.FadeOutImmediately(attackerHandWindow, attackerSlot.gameObject);
            
                // ---- 1) 대상보다 조금 왼쪽 아래로 "바로" 이동 ----
                var snapPos = targetPos + ctx.Settings.moveToTargetLeftDownOffset;

                // ---- 2) 뒤로 천천히 이동했다가 ----
                var backPos = snapPos + new Vector3(0, ctx.Settings.attackUnitBackDistance, 0);

                var defaultMoveOption = MoveOptions.Default;
                defaultMoveOption.easeType = ctx.Settings.attackUnitBackEasing;
                yield return UiMoveTransform.MoveTo(attackerHandWindow, iconTr, backPos,
                    ctx.Settings.attackUnitBackDuration, defaultMoveOption);

                // ---- 3) 빠른 속도로 상대 카드를 치는 듯한 느낌 ----
                defaultMoveOption = MoveOptions.Default;
                defaultMoveOption.easeType = ctx.Settings.attackUnitHitEasing;
                yield return UiMoveTransform.MoveTo(attackerHandWindow, iconTr, snapPos,
                    ctx.Settings.attackUnitHitDuration, defaultMoveOption);
            }

            // 타겟이 없으면 넘어가기
            if (defenderIcon == null && defenderSlot == null)
            {
                yield return new WaitForSeconds(0.05f);
                yield break;
            }

            if (step.Payload is not TcgAbilityPayloadDamage payload) yield break;
            if (payload.DamageValue <= 0) yield break;
            
            ShowDamageText(defenderIcon, payload.DamageValue * -1);
            ShowEffect(defenderIcon, EffectUidHit);
        }
    }
}
