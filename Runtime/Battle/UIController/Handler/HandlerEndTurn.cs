using System.Collections;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 턴이 종료 연출 핸들러.
    /// </summary>
    public sealed class HandlerEndTurn : ITcgPresentationHandler
    {
        /// <summary>
        /// 이 핸들러가 처리하는 프레젠테이션 스텝 타입.
        /// </summary>
        public TcgPresentationConstants.TcgPresentationStepType Type => TcgPresentationConstants.TcgPresentationStepType.EndTurn;

        /// <summary>
        /// 플레이어 턴일 때, 메시지를 보여주는 연출을 실행합니다.
        /// </summary>
        /// <param name="ctx">연출 실행 컨텍스트.</param>
        /// <param name="step">출발(손패) 인덱스, 도착(보드) 인덱스 및 보드 상태값 등이 포함된 스텝.</param>
        /// <returns>코루틴 이터레이터.</returns>
        public IEnumerator Play(TcgPresentationContext ctx, TcgPresentationStep step)
        {
            var uiWindowTcgBattleHud = ctx.BattleHud;
            if (uiWindowTcgBattleHud == null) yield break;
            
            yield return uiWindowTcgBattleHud.ShowEndTurnText();
            yield return new WaitForSeconds(0.05f);
        }
    }
}