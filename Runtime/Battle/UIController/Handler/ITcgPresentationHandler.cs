using System.Collections;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 프리젠테이션(연출) 스텝을 실행하는 핸들러의 공통 인터페이스.
    /// </summary>
    /// <remarks>
    /// 각 구현체는 특정 <see cref="TcgPresentationConstants.TcgPresentationStepType"/>에 대응하며,
    /// 해당 스텝의 UI 연출을 코루틴 형태로 수행한다.
    /// </remarks>
    public interface ITcgPresentationHandler
    {
        /// <summary>
        /// 이 핸들러가 처리할 프리젠테이션 스텝 타입.
        /// </summary>
        /// <remarks>
        /// 스텝 디스패처는 이 값을 기준으로 적절한 핸들러를 선택한다.
        /// </remarks>
        TcgPresentationConstants.TcgPresentationStepType Type { get; }

        /// <summary>
        /// 주어진 프리젠테이션 스텝에 대한 연출을 실행한다.
        /// </summary>
        /// <param name="ctx">UI 윈도우, 세션 상태 및 설정을 포함한 프리젠테이션 컨텍스트.</param>
        /// <param name="step">실행할 프리젠테이션 스텝 데이터.</param>
        /// <returns>연출 진행을 제어하는 코루틴 이터레이터.</returns>
        /// <remarks>
        /// 구현체는 필요한 UI 요소를 찾지 못한 경우 안전하게 조기 종료할 수 있다.
        /// 코루틴 종료 시점은 다음 스텝 실행 타이밍에 직접적인 영향을 줄 수 있다.
        /// </remarks>
        IEnumerator Play(TcgPresentationContext ctx, TcgPresentationStep step);
    }
}