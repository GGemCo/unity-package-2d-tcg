using System.Collections;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 프레젠테이션(연출) 스텝을 실행하는 핸들러 인터페이스.
    /// </summary>
    public interface ITcgPresentationHandler
    {
        /// <summary>
        /// 이 핸들러가 처리할 스텝 타입.
        /// </summary>
        TcgPresentationStepType Type { get; }

        /// <summary>
        /// 주어진 스텝에 대한 연출 코루틴을 실행합니다.
        /// </summary>
        /// <param name="ctx">연출 실행에 필요한 UI/세션 컨텍스트.</param>
        /// <param name="step">실행할 프레젠테이션 스텝.</param>
        /// <returns>코루틴 이터레이터.</returns>
        IEnumerator Play(TcgPresentationContext ctx, TcgPresentationStep step);
    }
}