namespace GGemCo2DTcg
{
    /// <summary>
    /// 이펙트 하나를 처리하는 핸들러 인터페이스.
    /// - EffectRunner 내부에서 등록하여 사용합니다.
    /// </summary>
    public interface ITcgEffectHandler
    {
        void Execute(TcgEffectContext context);
    }
}