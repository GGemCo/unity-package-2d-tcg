using System.Collections;
using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 유닛이 상대 영웅을 공격하는 연출을 재생하는 핸들러.
    /// </summary>
    /// <remarks>
    /// 공격자 아이콘을 타겟 위치로 이동시킨 후 원위치로 복귀하고,
    /// 영웅 HP/데미지 UI를 갱신하며 사망 시 페이드아웃을 하고, 전투 종료 수행합니다.
    /// </remarks>
    public sealed class HandlerAttackHero : HandlerBase, ITcgPresentationHandler
    {
        /// <summary>
        /// 이 핸들러가 처리하는 프레젠테이션 스텝 타입.
        /// </summary>
        public TcgPresentationConstants.TcgPresentationStepType Type => TcgPresentationConstants.TcgPresentationStepType.AttackHero;

        /// <summary>
        /// 공격(유닛 → 영웅) 연출을 실행합니다.
        /// </summary>
        /// <param name="ctx">연출 실행에 필요한 세션/윈도우 컨텍스트.</param>
        /// <param name="step">공격자/대상 인덱스 및 HP/데미지 값이 담긴 스텝.</param>
        /// <returns>코루틴 이터레이터.</returns>
        public IEnumerator Play(TcgPresentationContext ctx, TcgPresentationStep step)
        {
            var attackerSide  = step.Side;
            var attackerZone = step.FromZone;
            var attackerIndex = step.FromIndex;
            
            var defenderZone = step.ToZone;
            var defenderIndex = ConfigCommonTcg.IndexHeroSlot;

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

            // UI가 아직 생성/동기화되지 않은 프레임일 수 있으므로 짧게 대기 후 종료
            if (attackerIcon == null || defenderIcon == null || attackerSlot == null)
            {
                yield return new WaitForSeconds(0.05f);
                yield break;
            }

            Vector3 targetPos = defenderIcon.transform.position;

            var iconTr = attackerIcon.transform;

            // 이동 중 캔버스 정렬 문제를 피하기 위해 UI 루트로 일시 이동
            iconTr.SetParent(ctx.UIRoot, worldPositionStays: true);

            // ---- 1) 대상보다 조금 왼쪽 아래로 "바로" 이동 ----
            var snapPos = targetPos + ctx.Settings.moveToTargetLeftDownOffset;
            iconTr.position = snapPos;

            // ---- 2) 뒤로 천천히 이동했다가 ----
            var backPos = snapPos + new Vector3(0, ctx.Settings.attackUnitBackDistance, 0);

            var defaultMoveOption = MoveOptions.Default;
            defaultMoveOption.easeType = ctx.Settings.attackUnitBackEasing;
            yield return UiMoveTransform.MoveTo(attackerWindow, iconTr, backPos,
                ctx.Settings.attackUnitBackDuration, defaultMoveOption);

            // ---- 3) 빠른 속도로 상대 카드를 치는 듯한 느낌 ----
            defaultMoveOption = MoveOptions.Default;
            defaultMoveOption.easeType = ctx.Settings.attackUnitHitEasing;
            yield return UiMoveTransform.MoveTo(attackerWindow, iconTr, snapPos,
                ctx.Settings.attackUnitHitDuration, defaultMoveOption);

            // 잠시 대기 후 원복
            yield return new WaitForSeconds(0.1f);
            iconTr.SetParent(attackerSlot.transform, worldPositionStays: false);
            iconTr.localPosition = Vector3.zero;

            // 체력 업데이트
            if (defenderIcon is UIIconCard defenderCardIcon)
                defenderCardIcon.UpdateHealth(targetHp);

            // 데미지 표시
            if (damageToTarget > 0)
            {
                ShowDamageText(defenderIcon, damageToTarget * -1);
                ShowEffect(defenderIcon, EffectUidHit);
            }

            // 사망 페이드 아웃
            if (targetHp <= 0)
            {
                yield return new WaitForSeconds(ctx.Settings.handToGraveFadeOutDelayTime);
                
                var defaultFadeOption = UiFadeUtility.FadeOptions.Default;
                defaultFadeOption.easeType = ctx.Settings.handToGraveFadeOutEasing;
                yield return UiFadeUtility.FadeOut(attackerWindow, defenderSlot.gameObject, ctx.Settings.handToGraveFadeOutDuration, defaultFadeOption);
            }
        }
    }
}
