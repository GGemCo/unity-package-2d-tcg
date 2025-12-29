using System.Collections;

namespace GGemCo2DTcg
{
    public class HandlerPlaySpellCast : ITcgPresentationHandler
    {
        /// <summary>
        /// 이 핸들러가 처리하는 프레젠테이션 스텝 타입.
        /// </summary>
        public TcgPresentationStepType Type => TcgPresentationStepType.PlaySpellCast;
        
        /// <summary>
        /// 손패 스펠 타입 사용 연출을 실행합니다.
        /// </summary>
        /// <param name="ctx">연출 실행 컨텍스트.</param>
        /// <param name="step">출발(손패) 인덱스, 도착(보드) 인덱스 및 보드 상태값 등이 포함된 스텝.</param>
        /// <returns>코루틴 이터레이터.</returns>
        public IEnumerator Play(TcgPresentationContext ctx, TcgPresentationStep step)
        {
            var handWindow = ctx.GetHandWindow(step.Side);
            var fieldWindow = ctx.GetFieldWindow(step.Side);
            if (handWindow == null || fieldWindow == null) yield break;
            
            // 핸드에서는 지워주기
            var slot = handWindow.GetSlotByIndex(step.FromIndex);
            if (slot)
            {
                slot.gameObject.SetActive(false);
            }
        }
    }
}