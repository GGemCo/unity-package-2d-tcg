using System.Collections;
using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 손패(Hand)의 카드를 필드(Field/Board) 슬롯에 표시하도록 갱신하고 페이드인 연출을 재생하는 프리젠테이션 핸들러.
    /// </summary>
    /// <remarks>
    /// 실제 카드 데이터는 스텝 실행 전에 이미 필드로 이동된 상태를 전제로 하며,
    /// 이 핸들러는 UI 측에서 다음을 수행한다.
    /// - 손패 슬롯 비활성화(제거된 것처럼 보이게 처리)
    /// - 필드 슬롯에 아이콘을 생성/갱신하고 스탯(공/체)을 반영
    /// - 필드 슬롯을 페이드인하여 소환 느낌을 연출
    /// </remarks>
    public sealed class HandlerMoveCardToField : ITcgPresentationHandler
    {
        /// <summary>
        /// 이 핸들러가 처리하는 프리젠테이션 스텝 타입.
        /// </summary>
        public TcgPresentationConstants.TcgPresentationStepType Type =>
            TcgPresentationConstants.TcgPresentationStepType.MoveCardToField;

        /// <summary>
        /// 손패 → 필드 이동(소환) UI 연출을 재생한다.
        /// </summary>
        /// <param name="ctx">세션, UI 윈도우 및 컷신 설정을 제공하는 프리젠테이션 컨텍스트.</param>
        /// <param name="step">출발 존/인덱스(손패)와 도착 존/인덱스(필드)를 포함하는 스텝.</param>
        /// <returns>코루틴 이터레이터.</returns>
        /// <remarks>
        /// 필요한 UI 윈도우를 찾지 못하면 안전하게 조기 종료한다.
        /// 필드에 표시할 카드 정보는 세션의 필드 데이터에서 다시 조회한다(스텝 시점에 데이터가 이미 반영되었음을 전제).
        /// </remarks>
        public IEnumerator Play(TcgPresentationContext ctx, TcgPresentationStep step)
        {
            var attackerSide = step.Side;
            var attackerZone = step.FromZone;
            var attackerIndex = step.FromIndex;

            var targetZone = step.ToZone;
            var targetIndex = step.ToIndex;

            var handWindow = ctx.GetUIWindow(attackerZone);
            var fieldWindow = ctx.GetUIWindow(targetZone);
            if (handWindow == null || fieldWindow == null) yield break;

            // 손패 슬롯은 제거된 것처럼 보이도록 비활성화한다.
            var slot = handWindow.GetSlotByIndex(step.FromIndex);
            if (slot)
            {
                slot.gameObject.SetActive(false);
            }

            // NOTE: 데이터는 이미 변경되었으므로, 필드 데이터에서 카드 정보를 가져와 UI를 갱신한다.
            int toIndex = step.ToIndex;
            var casterDataSide = ctx.Session.GetSideState(step.Side);
            var fromCard = casterDataSide.GetBattleDataCardInFieldByIndex(toIndex);

            // 필드 슬롯에 아이콘을 생성/갱신한다(카드 UID 기준).
            var uiIcon = fieldWindow.SetIconCount(toIndex, fromCard.Uid, 1);
            if (!uiIcon)
            {
                // UI가 아직 생성/동기화되지 않은 프레임일 수 있으므로 짧게 대기 후 종료
                yield return new WaitForSeconds(0.05f);
                yield break;
            }

            // 스탯(공/체) 표시를 최신 값으로 반영한다.
            var uiIconCard = uiIcon.GetComponent<UIIconCard>();
            if (uiIconCard != null)
            {
                uiIconCard.UpdateAttack(fromCard.Attack);
                uiIconCard.UpdateHealth(fromCard.Health);
            }

            // 필드 슬롯 자체를 페이드인하여 소환 느낌을 준다.
            var targetSlot = fieldWindow.GetSlotByIndex(toIndex);
            if (targetSlot != null)
            {
                var defaultFadeOption = UiFadeUtility.FadeOptions.Default;
                defaultFadeOption.easeType = ctx.UICutsceneSettings.handToFieldFadeInEasing;
                yield return UiFadeUtility.FadeIn(
                    fieldWindow,
                    targetSlot.gameObject,
                    ctx.UICutsceneSettings.handToFieldFadeInDuration,
                    defaultFadeOption);
            }

            // 다음 연출과의 템포를 맞추기 위한 짧은 대기
            yield return new WaitForSeconds(0.05f);
        }
    }
}
