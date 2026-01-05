namespace GGemCo2DTcg
{
    /// <summary>
    /// 마나 획득 Ability 연출에 필요한 페이로드입니다.
    /// </summary>
    public sealed class TcgAbilityPayloadGainMana
    {
        public int ManaValue { get; }

        public TcgAbilityPayloadGainMana(int manaValue)
        {
            ManaValue = manaValue;
        }
    }
}
