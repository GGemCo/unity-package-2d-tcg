using System.Collections;
using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 손패의 카드를 필드(보드) 슬롯으로 이동시키는 소환 연출 핸들러.
    /// </summary>
    public sealed class HandlerSummonFromHandToBoard : ITcgPresentationHandler
    {
        /// <summary>
        /// 이 핸들러가 처리하는 프레젠테이션 스텝 타입.
        /// </summary>
        public TcgPresentationStepType Type => TcgPresentationStepType.MoveCardHandToBoard;

        /// <summary>
        /// 손패 → 보드 이동 연출을 실행합니다.
        /// </summary>
        /// <param name="ctx">연출 실행 컨텍스트.</param>
        /// <param name="step">출발(손패) 인덱스, 도착(보드) 인덱스 및 보드 상태값 등이 포함된 스텝.</param>
        /// <returns>코루틴 이터레이터.</returns>
        public IEnumerator Play(TcgPresentationContext ctx, TcgPresentationStep step)
        {
            var handWindow = ctx.GetHandWindow(step.Side);
            var fieldWindow = ctx.GetFieldWindow(step.Side);
            if (handWindow == null || fieldWindow == null) yield break;

            // Hand UI: 0번은 영웅, 실제 손패는 1번부터(그래서 +1 오프셋)
            int fronIndexByWindow = step.FromIndex + 1;
            var slot = handWindow.GetSlotByIndex(fronIndexByWindow);
            slot.gameObject.SetActive(false);
            var icon = handWindow.GetIconByIndex(fronIndexByWindow);
            
            int toIndex = step.ToIndex;
            var destSlot = fieldWindow.GetSlotByIndex(toIndex);

            if (icon == null || destSlot == null)
            {
                // UI가 아직 준비되지 않은 경우를 고려한 짧은 대기
                yield return new WaitForSecondsRealtime(0.05f);
                yield break;
            }

            var grid = fieldWindow.containerIcon;
            var childCount = step.ValueA; // 그리드 좌표 계산에 필요한 현재 아이콘 개수(스텝 값으로 전달)
            if (!GridLayoutPositionUtility.TryGetCellTransformPosition(grid, step.ToIndex, childCount, out var pos))
            {
                // 좌표 계산 실패 시 안전 fallback (slot position)
                pos = destSlot.transform.position;
            }
            // 이동 중 캔버스 정렬 문제를 피하기 위해 UI 루트로 일시 이동
            icon.transform.SetParent(ctx.UIRoot, worldPositionStays: true);

            yield return TcgUiTween.MoveTo(icon.transform, pos, fieldWindow.timeToMove);
            
            // 이미 데이터는 변경되었으므로, Board에서 정보를 가져온다.
            var fromCard = step.Attacker.Board.GetFieldDataByIndex(toIndex);
            var uiIcon = fieldWindow.SetIconCount(toIndex, fromCard.Uid, 1);
            if (!uiIcon) {
                yield return new WaitForSecondsRealtime(0.05f);
                yield break;
            }
            var uiIconCard = uiIcon.GetComponent<UIIconCard>();
            if (uiIconCard != null)
            {
                uiIconCard.UpdateAttack(fromCard.Attack);
                uiIconCard.UpdateHealth(fromCard.Health);
            }
            
            // 원복
            icon.transform.SetParent(slot.transform, worldPositionStays: false);
            icon.transform.localPosition = Vector3.zero;
            yield return new WaitForSecondsRealtime(0.05f);
        }
    }
}