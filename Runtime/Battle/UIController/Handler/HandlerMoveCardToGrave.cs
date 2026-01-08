using System.Collections;
using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 카드가 사용/소멸되어 묘지로 이동하는 시점에, 현재 위치에서 페이드아웃(또는 번 디졸브) 연출을 재생하는 프리젠테이션 핸들러.
    /// </summary>
    /// <remarks>
    /// 이 핸들러는 실제로 UI 상에서 다음을 수행한다.
    /// - 설정된 지연 시간 후 아이콘을 페이드아웃하거나 BurnDissolve 연출을 재생
    /// - 아이콘 트랜스폼을 원래 슬롯 자식으로 복원
    /// - 슬롯을 즉시 숨김 처리하여 "빈 슬롯" 상태로 만든다
    /// </remarks>
    public sealed class HandlerMoveCardToGrave : ITcgPresentationHandler
    {
        /// <summary>
        /// 이 핸들러가 처리하는 프리젠테이션 스텝 타입.
        /// </summary>
        public TcgPresentationConstants.TcgPresentationStepType Type =>
            TcgPresentationConstants.TcgPresentationStepType.MoveCardToGrave;

        /// <summary>
        /// 묘지 이동(소멸) 연출을 재생한다.
        /// </summary>
        /// <param name="ctx">UI 윈도우 조회 및 컷신 설정을 제공하는 프리젠테이션 컨텍스트.</param>
        /// <param name="step">출발 존/인덱스(페이드아웃할 카드 위치)를 포함하는 스텝.</param>
        /// <returns>코루틴 이터레이터.</returns>
        /// <remarks>
        /// 필요한 UI 오브젝트(윈도우/슬롯/아이콘)를 찾지 못하면 안전하게 조기 종료한다.
        /// 실제 카드 데이터의 "묘지 이동"은 별도 로직에서 이미 처리되었음을 전제로 한다.
        /// </remarks>
        public IEnumerator Play(TcgPresentationContext ctx, TcgPresentationStep step)
        {
            var attackerSide = step.Side;
            var attackerZone = step.FromZone;
            var attackerIndex = step.FromIndex;

            var attackerWindow = ctx.GetUIWindow(attackerZone);
            if (attackerWindow == null) yield break;

            UISlot attackerSlot = attackerWindow.GetSlotByIndex(attackerIndex);
            UIIcon attackerIcon = attackerWindow.GetIconByIndex(attackerIndex);

            // UI가 아직 생성/동기화되지 않은 프레임일 수 있으므로 짧게 대기 후 종료
            if (attackerIcon == null || attackerSlot == null)
            {
                yield return new WaitForSeconds(0.05f);
                yield break;
            }

            // 다른 연출 템포에 맞춘 딜레이 후 소멸 연출을 시작한다.
            yield return new WaitForSeconds(ctx.UICutsceneSettings.handToGraveFadeOutDelayTime);

            // 설정에 따라 BurnDissolve 또는 일반 페이드아웃을 사용한다.
            if (ctx.UICutsceneSettings.handToGraveUseBurnDissolve)
            {
                var uiBurnDissolvePlayer = attackerIcon.GetComponent<UiBurnDissolvePlayer>();
                if (uiBurnDissolvePlayer != null)
                {
                    yield return uiBurnDissolvePlayer.CoPlay();
                }
            }
            else
            {
                var defaultFadeOption = UiFadeUtility.FadeOptions.Default;
                defaultFadeOption.easeType = ctx.UICutsceneSettings.handToGraveFadeOutEasing;
                defaultFadeOption.startAlpha = 1f;

                yield return UiFadeUtility.FadeOut(
                    attackerWindow,
                    attackerIcon.gameObject,
                    ctx.UICutsceneSettings.handToGraveFadeOutDuration,
                    defaultFadeOption);
            }

            // 아이콘을 슬롯 자식으로 복원(다음 스텝에서 재사용/정렬되도록 원위치로 되돌림)
            var iconTr = attackerIcon.transform;
            iconTr.SetParent(attackerSlot.transform, worldPositionStays: false);
            iconTr.localPosition = Vector3.zero;

            // 슬롯은 즉시 숨김 처리하여 빈 자리로 만든다.
            yield return UiFadeUtility.FadeOutImmediately(attackerWindow, attackerSlot.gameObject);
        }
    }
}
