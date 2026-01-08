using System.Collections;
using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// Ability 기반 피해(데미지) 연출(팝업 텍스트/히트 이펙트 및 간단한 타격 모션)을 처리하는 프리젠테이션 핸들러.
    /// </summary>
    public sealed class HandlerAbilityDamage : HandlerBase, ITcgPresentationHandler
    {
        /// <summary>
        /// 이 핸들러가 처리하는 프리젠테이션 스텝 타입.
        /// </summary>
        public TcgPresentationConstants.TcgPresentationStepType Type =>
            TcgPresentationConstants.TcgPresentationStepType.AbilityDamage;

        /// <summary>
        /// 피해(데미지) 연출을 재생한다.
        /// </summary>
        /// <param name="ctx">UI 조회/연출 설정(UICutsceneSettings) 및 루트 트랜스폼을 제공하는 컨텍스트.</param>
        /// <param name="step">공격자/대상 위치 정보와 피해 페이로드를 포함하는 스텝.</param>
        /// <returns>코루틴 이터레이터.</returns>
        /// <remarks>
        /// 공격자가 없는(예: Permanent/환경 효과 등) 피해도 있을 수 있으므로 공격자 아이콘/슬롯은 선택적으로 처리한다.
        /// 페이로드가 피해 타입이 아니거나, 피해 값이 0 이하이면 연출을 수행하지 않는다.
        /// </remarks>
        public IEnumerator Play(TcgPresentationContext ctx, TcgPresentationStep step)
        {
            // 공격자/대상 위치 정보
            var attackerSide  = step.Side;
            var attackerZone = step.FromZone;
            var attackerIndex = step.FromIndex;
            var defenderZone = step.ToZone;
            var defenderIndex = step.ToIndex;

            var attackerHandWindow = ctx.GetUIWindow(attackerZone);
            var defenderFieldWindow = ctx.GetUIWindow(defenderZone);
            if (attackerHandWindow == null || defenderFieldWindow == null) yield break;

            // 공격자는 없을 수 있으므로(예: Permanent/환경 효과 등) 인덱스가 유효할 때만 조회한다.
            UISlot attackerSlot = attackerIndex >= 0 ? attackerHandWindow.GetSlotByIndex(attackerIndex) : null;
            UIIcon attackerIcon = attackerIndex >= 0 ? attackerHandWindow.GetIconByIndex(attackerIndex) : null;

            UISlot defenderSlot = defenderFieldWindow.GetSlotByIndex(defenderIndex);
            UIIcon defenderIcon = defenderFieldWindow.GetIconByIndex(defenderIndex);

            // 공격자 아이콘이 존재하는 경우에만 타격 모션(접근/후퇴/히트)을 수행한다.
            if (attackerIcon != null && attackerSlot != null)
            {
                Vector3 targetPos = defenderIcon.transform.position;
                var iconTr = attackerIcon.transform;

                // 이동 중 캔버스 정렬(소팅/레이아웃) 문제를 피하기 위해 UI 루트로 일시 이동
                iconTr.SetParent(ctx.UIRoot, worldPositionStays: true);

                // 슬롯은 숨기고 아이콘만 분리하여 연출한다.
                yield return UiFadeUtility.FadeOutImmediately(attackerHandWindow, attackerSlot.gameObject);

                // 1) 대상보다 조금 왼쪽 아래로 "즉시" 이동(타격 연출의 시작 위치)
                var snapPos = targetPos + ctx.UICutsceneSettings.moveToTargetLeftDownOffset;

                // 2) 살짝 뒤로 물러나는 모션
                var backPos = snapPos + new Vector3(0, ctx.UICutsceneSettings.attackUnitBackDistance, 0);

                var defaultMoveOption = MoveOptions.Default;
                defaultMoveOption.easeType = ctx.UICutsceneSettings.attackUnitBackEasing;
                yield return UiMoveTransform.MoveTo(attackerHandWindow, iconTr, backPos,
                    ctx.UICutsceneSettings.attackUnitBackDuration, defaultMoveOption);

                // 3) 빠르게 전진하며 타격하는 느낌의 모션
                defaultMoveOption = MoveOptions.Default;
                defaultMoveOption.easeType = ctx.UICutsceneSettings.attackUnitHitEasing;
                yield return UiMoveTransform.MoveTo(attackerHandWindow, iconTr, snapPos,
                    ctx.UICutsceneSettings.attackUnitHitDuration, defaultMoveOption);
            }

            // 대상이 없으면 연출을 수행할 수 없으므로 안전하게 종료한다.
            // NOTE: 현재 조건은 "둘 다 null"일 때만 종료한다(아이콘만 있거나 슬롯만 있는 경우는 진행).
            if (defenderIcon == null && defenderSlot == null)
            {
                yield return new WaitForSeconds(0.05f);
                yield break;
            }

            // 피해 페이로드가 아니면 처리하지 않는다.
            if (step.Payload is not TcgAbilityPayloadDamage payload) yield break;

            // 0 이하 피해는 연출하지 않는다(회복/무효 처리 등은 별도 스텝에서 처리하는 것을 전제).
            if (payload.DamageValue <= 0) yield break;

            // 표시 규약: 데미지 텍스트는 음수로 표기(예: -10)
            ShowDamageText(defenderIcon, payload.DamageValue * -1);
            ShowEffect(defenderIcon, EffectUidHit);
        }
    }
}
