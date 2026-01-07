using System.Collections;
using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 유닛이 상대 유닛을 공격하는 연출을 재생하는 핸들러.
    /// </summary>
    /// <remarks>
    /// 1) 대상보다 조금 왼쪽 아래로 즉시 이동
    /// 2) 뒤로 천천히 이동
    /// 3) 빠르게 대상 위치로 타격 이동
    /// HP/데미지 UI 갱신 및 사망 시 페이드아웃을 수행합니다.
    /// </remarks>
    public sealed class HandlerAttackUnit : HandlerBase, ITcgPresentationHandler
    {
        public TcgPresentationConstants.TcgPresentationStepType Type => TcgPresentationConstants.TcgPresentationStepType.AttackUnit;

        public IEnumerator Play(TcgPresentationContext ctx, TcgPresentationStep step)
        {
            var attackerSide  = step.Side;
            var attackerZone = step.FromZone;
            var attackerIndex = step.FromIndex;
            
            var defenderZone = step.ToZone;
            var defenderIndex = step.ToIndex;

            var attackerDataSide = ctx.Session.GetSideState(attackerSide);
            var defenderDataSide = ctx.Session.GetOpponentState(attackerSide);
            var payloadAttackUnit = step.Payload as TcgBattleUIControllerPayloadAttackUnit;
            if (payloadAttackUnit == null)
            {
                GcLogger.LogError($"{nameof(step.Payload)} 에 {nameof(TcgBattleUIControllerPayloadAttackUnit)} 값이 없습니다.");
                yield break;
            }

            int attackerHp = payloadAttackUnit.AttackerHealth;
            int damageToAttacker = payloadAttackUnit.DamageToAttacker;
            
            int targetHp = payloadAttackUnit.TargetHealth;
            int damageToTarget = payloadAttackUnit.DamageToTarget;

            var attackerWindow = ctx.GetUIWindow(attackerZone);
            var defenderWindow = ctx.GetUIWindow(defenderZone);

            if (attackerWindow == null || defenderWindow == null) yield break;

            var attackerSlot = attackerWindow.GetSlotByIndex(attackerIndex);
            var attackerIcon = attackerWindow.GetIconByIndex(attackerIndex);
            
            var defenderSlot = defenderWindow.GetSlotByIndex(defenderIndex);
            var defenderIcon = defenderWindow.GetIconByIndex(defenderIndex);

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
            
            // ---- 1) 대상보다 조금 왼쪽 아래로 "바로" 이동 ----
            var snapPos = targetPos + ctx.UICutsceneSettings.GetMoveToTargetLeftDownOffset(attackerSide);
            iconTr.position = snapPos;

            // ---- 2) 뒤로 천천히 이동했다가 ----
            var backPos = snapPos + new Vector3(0, ctx.UICutsceneSettings.GetAttackUnitBackDistance(attackerSide), 0);

            var defaultMoveOption = MoveOptions.Default;
            defaultMoveOption.easeType = ctx.UICutsceneSettings.attackUnitBackEasing;
            yield return UiMoveTransform.MoveTo(attackerWindow, iconTr, backPos,
                ctx.UICutsceneSettings.attackUnitBackDuration, defaultMoveOption);

            // ---- 3) 빠른 속도로 상대 카드를 치는 듯한 느낌 ----
            defaultMoveOption = MoveOptions.Default;
            defaultMoveOption.easeType = ctx.UICutsceneSettings.attackUnitHitEasing;
            yield return UiMoveTransform.MoveTo(attackerWindow, iconTr, snapPos,
                ctx.UICutsceneSettings.attackUnitHitDuration, defaultMoveOption);

            // 체력 업데이트
            if (attackerIcon is UIIconCard attackerCardIcon)
                attackerCardIcon.UpdateHealth(attackerHp);

            if (defenderIcon is UIIconCard defenderCardIcon)
                defenderCardIcon.UpdateHealth(targetHp);
            
            // 데미지 표시
            if (damageToTarget > 0)
            {
                ShowDamageText(defenderIcon, damageToTarget * -1);
                ShowEffect(defenderIcon, EffectUidHit);
            }
            
            yield return new WaitForSeconds(ctx.UICutsceneSettings.attackUnitShowDamageDiffDuration);

            if (damageToAttacker > 0)
            {
                ShowDamageText(attackerIcon, damageToAttacker * -1);
                ShowEffect(attackerIcon, EffectUidHit);
            }
        }
    }
}
