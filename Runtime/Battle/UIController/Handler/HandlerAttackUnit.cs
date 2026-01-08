using System.Collections;
using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 유닛이 상대 유닛을 공격하는 연출을 재생하는 프리젠테이션 핸들러.
    /// </summary>
    /// <remarks>
    /// 공격자 아이콘을 대상 근처로 이동시키며 다음 단계를 순차 재생한다.
    /// 1) 대상보다 조금 왼쪽 아래로 즉시 이동(스냅)
    /// 2) 뒤로 천천히 이동(백스윙)
    /// 3) 빠르게 전진하며 타격(히트)
    /// 이후 공격자/대상 HP UI를 갱신하고, 데미지 팝업 및 히트 이펙트를 표시한다.
    /// </remarks>
    public sealed class HandlerAttackUnit : HandlerBase, ITcgPresentationHandler
    {
        /// <summary>
        /// 이 핸들러가 처리하는 프리젠테이션 스텝 타입.
        /// </summary>
        public TcgPresentationConstants.TcgPresentationStepType Type =>
            TcgPresentationConstants.TcgPresentationStepType.AttackUnit;

        /// <summary>
        /// 공격(유닛 → 유닛) 연출을 재생한다.
        /// </summary>
        /// <param name="ctx">세션/윈도우 조회 및 컷신 설정을 제공하는 프리젠테이션 컨텍스트.</param>
        /// <param name="step">공격자/대상(존/인덱스)과 공격 페이로드를 포함하는 스텝.</param>
        /// <returns>코루틴 이터레이터.</returns>
        /// <remarks>
        /// <paramref name="step"/>의 <c>Payload</c>는 <see cref="TcgBattleUIControllerPayloadAttackUnit"/> 이어야 한다.
        /// 필요한 UI 오브젝트를 찾지 못하면 안전하게 조기 종료한다.
        /// </remarks>
        public IEnumerator Play(TcgPresentationContext ctx, TcgPresentationStep step)
        {
            var attackerSide = step.Side;
            var attackerZone = step.FromZone;
            var attackerIndex = step.FromIndex;

            var defenderZone = step.ToZone;
            var defenderIndex = step.ToIndex;

            // NOTE: 현재 구현에서는 데이터 사이드 참조를 생성하지만 실제 연출 로직에는 사용하지 않는다.
            var attackerDataSide = ctx.Session.GetSideState(attackerSide);
            var defenderDataSide = ctx.Session.GetOpponentState(attackerSide);

            var payloadAttackUnit = step.Payload as TcgBattleUIControllerPayloadAttackUnit;
            if (payloadAttackUnit == null)
            {
                GcLogger.LogError(
                    $"{nameof(step.Payload)} 에 {nameof(TcgBattleUIControllerPayloadAttackUnit)} 값이 없습니다.");
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

            // 이동 중 캔버스 정렬(소팅/레이아웃) 문제를 피하기 위해 UI 루트로 일시 이동
            iconTr.SetParent(ctx.UIRoot, worldPositionStays: true);

            // 공격자 슬롯은 숨기고 아이콘만 분리하여 연출한다.
            yield return UiFadeUtility.FadeOutImmediately(attackerWindow, attackerSlot.gameObject);

            // 1) 대상보다 조금 왼쪽 아래로 스냅 이동(공격자 사이드에 따라 오프셋이 달라질 수 있음)
            var snapPos = targetPos + ctx.UICutsceneSettings.GetMoveToTargetLeftDownOffset(attackerSide);
            iconTr.position = snapPos;

            // 2) 백스윙(뒤로 천천히 이동)
            var backPos = snapPos + new Vector3(0, ctx.UICutsceneSettings.GetAttackUnitBackDistance(attackerSide), 0);

            var defaultMoveOption = MoveOptions.Default;
            defaultMoveOption.easeType = ctx.UICutsceneSettings.attackUnitBackEasing;
            yield return UiMoveTransform.MoveTo(attackerWindow, iconTr, backPos,
                ctx.UICutsceneSettings.attackUnitBackDuration, defaultMoveOption);

            // 3) 히트(빠르게 전진하며 타격)
            defaultMoveOption = MoveOptions.Default;
            defaultMoveOption.easeType = ctx.UICutsceneSettings.attackUnitHitEasing;
            yield return UiMoveTransform.MoveTo(attackerWindow, iconTr, snapPos,
                ctx.UICutsceneSettings.attackUnitHitDuration, defaultMoveOption);

            // 공격자/대상 체력 UI 업데이트
            if (attackerIcon is UIIconCard attackerCardIcon)
                attackerCardIcon.UpdateHealth(attackerHp);

            if (defenderIcon is UIIconCard defenderCardIcon)
                defenderCardIcon.UpdateHealth(targetHp);

            // 대상 데미지 표시(규약: 피해는 음수로 표기)
            if (damageToTarget > 0)
            {
                ShowDamageText(defenderIcon, damageToTarget * -1);
                ShowEffect(defenderIcon, EffectUidHit);
            }

            // 양측 데미지 표시 타이밍을 분리해 “맞는 느낌”을 강조한다.
            yield return new WaitForSeconds(ctx.UICutsceneSettings.attackUnitShowDamageDiffDuration);

            // 공격자 데미지 표시(반격/상호 피해)
            if (damageToAttacker > 0)
            {
                ShowDamageText(attackerIcon, damageToAttacker * -1);
                ShowEffect(attackerIcon, EffectUidHit);
            }
        }
    }
}
