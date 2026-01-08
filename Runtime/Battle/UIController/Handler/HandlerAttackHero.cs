using System.Collections;
using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 유닛이 상대 영웅(히어로 슬롯)을 공격하는 연출을 재생하는 프리젠테이션 핸들러.
    /// </summary>
    /// <remarks>
    /// 공격자 아이콘을 대상 위치로 이동(스냅 → 백스윙 → 히트)한 뒤 원위치로 복귀시킨다.
    /// 이후 대상 영웅의 HP UI를 갱신하고, 데미지 팝업/히트 이펙트를 표시하며,
    /// 대상 HP가 0 이하이면 영웅 슬롯을 페이드아웃한다.
    /// </remarks>
    public sealed class HandlerAttackHero : HandlerBase, ITcgPresentationHandler
    {
        /// <summary>
        /// 이 핸들러가 처리하는 프리젠테이션 스텝 타입.
        /// </summary>
        public TcgPresentationConstants.TcgPresentationStepType Type =>
            TcgPresentationConstants.TcgPresentationStepType.AttackHero;

        /// <summary>
        /// 공격(유닛 → 영웅) 연출을 재생한다.
        /// </summary>
        /// <param name="ctx">세션/윈도우 조회 및 컷신 설정을 제공하는 프리젠테이션 컨텍스트.</param>
        /// <param name="step">공격자 정보(사이드/존/인덱스)와 공격 페이로드를 포함하는 스텝.</param>
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
            var defenderIndex = ConfigCommonTcg.IndexHeroSlot;

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

            // 1) 대상보다 조금 왼쪽 아래로 스냅 이동(타격 연출 시작 위치)
            var snapPos = targetPos + ctx.UICutsceneSettings.moveToTargetLeftDownOffset;
            iconTr.position = snapPos;

            // 2) 뒤로 천천히 물러나는 백스윙
            var backPos = snapPos + new Vector3(0, ctx.UICutsceneSettings.attackUnitBackDistance, 0);

            var defaultMoveOption = MoveOptions.Default;
            defaultMoveOption.easeType = ctx.UICutsceneSettings.attackUnitBackEasing;
            yield return UiMoveTransform.MoveTo(attackerWindow, iconTr, backPos,
                ctx.UICutsceneSettings.attackUnitBackDuration, defaultMoveOption);

            // 3) 빠르게 전진하며 히트
            defaultMoveOption = MoveOptions.Default;
            defaultMoveOption.easeType = ctx.UICutsceneSettings.attackUnitHitEasing;
            yield return UiMoveTransform.MoveTo(attackerWindow, iconTr, snapPos,
                ctx.UICutsceneSettings.attackUnitHitDuration, defaultMoveOption);

            // 잠시 대기 후 원복(공격자 슬롯으로 되돌림)
            yield return new WaitForSeconds(0.1f);
            iconTr.SetParent(attackerSlot.transform, worldPositionStays: false);
            iconTr.localPosition = Vector3.zero;

            // 대상(영웅) 체력 UI 업데이트
            if (defenderIcon is UIIconCard defenderCardIcon)
                defenderCardIcon.UpdateHealth(targetHp);

            // 데미지 표시(규약: 피해는 음수로 표기)
            if (damageToTarget > 0)
            {
                ShowDamageText(defenderIcon, damageToTarget * -1);
                ShowEffect(defenderIcon, EffectUidHit);
            }

            // 대상 사망 시 페이드아웃
            if (targetHp <= 0)
            {
                yield return new WaitForSeconds(ctx.UICutsceneSettings.handToGraveFadeOutDelayTime);

                var defaultFadeOption = UiFadeUtility.FadeOptions.Default;
                defaultFadeOption.easeType = ctx.UICutsceneSettings.handToGraveFadeOutEasing;
                yield return UiFadeUtility.FadeOut(
                    attackerWindow,
                    defenderSlot.gameObject,
                    ctx.UICutsceneSettings.handToGraveFadeOutDuration,
                    defaultFadeOption);
            }
        }
    }
}
