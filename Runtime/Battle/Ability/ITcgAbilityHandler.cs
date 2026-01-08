namespace GGemCo2DTcg
{
    /// <summary>
    /// 단일 Ability의 실행 로직을 담당하는 핸들러 인터페이스입니다.
    /// </summary>
    /// <remarks>
    /// - 각 Ability 타입(데미지, 힐, 드로우 등)은 이 인터페이스를 구현한
    ///   개별 핸들러 클래스로 분리됩니다.
    /// - <c>AbilityRunner</c> 내부에서 핸들러를 등록·관리하며,
    ///   실행 시점에 적절한 핸들러를 선택해 호출합니다.
    /// - 도메인 로직 전용 인터페이스로, UI 연출과는 직접적으로 연결되지 않습니다.
    /// </remarks>
    public interface ITcgAbilityHandler
    {
        /// <summary>
        /// Ability 실행 컨텍스트를 기반으로 실제 능력 효과를 처리합니다.
        /// </summary>
        /// <param name="context">
        /// Ability 실행에 필요한 모든 정보(시전자, 대상, 파라미터, 결과 수집 등)를
        /// 포함하는 컨텍스트 객체입니다.
        /// </param>
        void Execute(TcgAbilityContext context);
    }
}