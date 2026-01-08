using System.Collections;
using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 특정 대상(타겟) 위치로 카드 아이콘을 즉시(스냅) 이동시키는 프리젠테이션 핸들러.
    /// </summary>
    /// <remarks>
    /// 공격/능력 연출에서 "타겟팅" 느낌을 주기 위해 공격자 아이콘을 대상 근처로 이동시킨다.
    /// 현재 구현은 다음만 수행한다.
    /// - 공격자 아이콘을 UI 루트로 분리
    /// - 공격자 슬롯을 숨김
    /// - 대상 아이콘 기준 오프셋 위치로 스냅 이동
    ///
    /// 아이콘 원복(슬롯으로 되돌리기) 또는 추가 모션/이펙트는 별도 스텝(예: MoveCardToBack 등)에서 처리하는 것을 전제로 한다.
    /// </remarks>
    public class HandlerMoveCardToTarget : HandlerBase, ITcgPresentationHandler
    {
        /// <summary>
        /// 이 핸들러가 처리하는 프리젠테이션 스텝 타입.
        /// </summary>
        public TcgPresentationConstants.TcgPresentationStepType Type =>
            TcgPresentationConstants.TcgPresentationStepType.MoveCardToTarget;

        /// <summary>
        /// 공격자 아이콘을 대상 위치 근처로 스냅 이동시키는 연출을 재생한다.
        /// </summary>
        /// <param name="ctx">UI 윈도우 조회 및 컷신 설정을 제공하는 프리젠테이션 컨텍스트.</param>
        /// <param name="step">출발(존/인덱스)과 대상(존/인덱스)을 포함하는 스텝.</param>
        /// <returns>코루틴 이터레이터.</returns>
        /// <remarks>
        /// 필요한 UI 오브젝트(윈도우/슬롯/아이콘)를 찾지 못하면 안전하게 조기 종료한다.
        /// </remarks>
        public IEnumerator Play(TcgPresentationContext ctx, TcgPresentationStep step)
        {
            var attackerSide = step.Side;
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

            // UI가 아직 생성/동기화되지 않은 프레임일 수 있으므로 짧게 대기 후 종료
            if (attackerSlot == null || attackerIcon == null || defenderSlot == null || defenderIcon == null)
            {
                yield return new WaitForSeconds(0.05f);
                yield break;
            }

            var iconTr = attackerIcon.transform;

            // 이동 중 캔버스 정렬(소팅/레이아웃) 문제를 피하기 위해 UI 루트로 일시 이동
            iconTr.SetParent(ctx.UIRoot, worldPositionStays: true);

            // 공격자 슬롯은 숨기고 아이콘만 분리하여 연출한다.
            yield return UiFadeUtility.FadeOutImmediately(attackerWindow, attackerSlot.gameObject);

            // 대상보다 조금 왼쪽 아래로 스냅 이동(타겟팅 포지션)
            var snapPos = defenderIcon.transform.position + ctx.UICutsceneSettings.moveToTargetLeftDownOffset;
            iconTr.position = snapPos;
        }
    }
}
