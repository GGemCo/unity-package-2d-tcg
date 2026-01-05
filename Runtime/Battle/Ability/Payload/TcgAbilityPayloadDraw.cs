namespace GGemCo2DTcg
{
    /// <summary>
    /// 드로우(카드 획득) Ability 연출에 필요한 페이로드입니다.
    /// </summary>
    public sealed class TcgAbilityPayloadDraw
    {
        public int DrawCount { get; }

        public TcgAbilityPayloadDraw(int drawCount)
        {
            DrawCount = drawCount;
        }
    }
}
