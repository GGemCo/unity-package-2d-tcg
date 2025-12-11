namespace GGemCo2DTcg
{
    /// <summary>
    /// 능력 하나를 처리하는 핸들러 인터페이스.
    /// - AbilityRunner 내부에서 등록하여 사용합니다.
    /// </summary>
    public interface ITcgAbilityHandler
    {
        void Execute(TcgAbilityContext context);
    }
}