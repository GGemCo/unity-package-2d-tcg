using System.Collections;
using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// Ability 기반 회복(힐) 연출(팝업 텍스트/회복 이펙트)을 처리하는 프리젠테이션 핸들러.
    /// </summary>
    public sealed class HandlerAbilityHeal : HandlerBase, ITcgPresentationHandler
    {
        /// <summary>
        /// 이 핸들러가 처리하는 프리젠테이션 스텝 타입.
        /// </summary>
        public TcgPresentationConstants.TcgPresentationStepType Type =>
            TcgPresentationConstants.TcgPresentationStepType.HealPopup;

        /// <summary>
        /// 회복(힐) 연출을 재생한다.
        /// </summary>
        /// <param name="ctx">UI 조회 및 연출 실행에 필요한 프리젠테이션 컨텍스트.</param>
        /// <param name="step">공격자/대상 위치 정보와 회복 페이로드를 포함하는 스텝.</param>
        /// <returns>코루틴 이터레이터.</returns>
        /// <remarks>
        /// 공격자가 없는(예: Permanent/환경 효과 등) 회복도 있을 수 있으므로 공격자 아이콘/슬롯은 선택적으로 처리한다.
        /// 페이로드가 회복 타입이 아니거나, 회복 값이 0 이하이면 연출을 수행하지 않는다.
        /// </remarks>
        public IEnumerator Play(TcgPresentationContext ctx, TcgPresentationStep step)
        {
            // 공격자/대상 위치 정보
            var attackerSide  = step.Side;
            var attackerZone = step.FromZone;
            var attackerIndex = step.FromIndex;
            var defenderZone = step.ToZone;
            var defenderIndex = step.ToIndex;

            var attackerWindow = ctx.GetUIWindow(attackerZone);
            var defenderWindow = ctx.GetUIWindow(defenderZone);
            if (attackerWindow == null || defenderWindow == null) yield break;

            // 공격자는 없을 수 있으므로(예: Permanent/환경 효과 등) 인덱스가 유효할 때만 조회한다.
            UISlot attackerSlot = attackerIndex >= 0 ? attackerWindow.GetSlotByIndex(attackerIndex) : null;
            UIIcon attackerIcon = attackerIndex >= 0 ? attackerWindow.GetIconByIndex(attackerIndex) : null;

            // 공격자 아이콘이 존재하는 경우에만 분리 연출(루트 이동/슬롯 숨김)을 수행한다.
            if (attackerIcon != null && attackerSlot != null)
            {
                var iconTr = attackerIcon.transform;

                // 이동/연출 중 캔버스 정렬(소팅/레이아웃) 문제를 피하기 위해 UI 루트로 일시 이동
                iconTr.SetParent(ctx.UIRoot, worldPositionStays: true);

                // 슬롯은 숨기고 아이콘만 분리하여 연출한다.
                yield return UiFadeUtility.FadeOutImmediately(attackerWindow, attackerSlot.gameObject);
            }

            UISlot defenderSlot = defenderWindow.GetSlotByIndex(defenderIndex);
            UIIcon defenderIcon = defenderWindow.GetIconByIndex(defenderIndex);

            // 대상이 없으면 연출을 수행할 수 없으므로 안전하게 종료한다.
            // NOTE: 현재 조건은 "둘 다 null"일 때만 종료한다(아이콘만 있거나 슬롯만 있는 경우는 진행).
            if (defenderIcon == null && defenderSlot == null)
            {
                yield return new WaitForSeconds(0.05f);
                yield break;
            }

            // 회복 페이로드가 아니면 처리하지 않는다.
            if (step.Payload is not TcgAbilityPayloadHeal payload) yield break;

            // 0 이하 회복은 연출하지 않는다(무효/흡수 등은 별도 스텝을 전제).
            if (payload.HealValue <= 0) yield break;

            // 회복 수치를 팝업으로 표시한다(양수 표기).
            ShowDamageText(defenderIcon, payload.HealValue);

            // 회복 이펙트를 대상 아이콘에 표시한다.
            ShowEffect(defenderIcon, EffectUidHeal);
        }
    }
}
