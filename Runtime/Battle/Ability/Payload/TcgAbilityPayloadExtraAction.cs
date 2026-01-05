namespace GGemCo2DTcg
{
    /// <summary>
    /// 추가 행동 Ability 연출에 필요한 페이로드입니다.
    /// </summary>
    public sealed class TcgAbilityPayloadExtraAction
    {
        public int ExtraActionCount { get; }

        public TcgAbilityPayloadExtraAction(int extraActionCount)
        {
            ExtraActionCount = extraActionCount;
        }
    }
}
