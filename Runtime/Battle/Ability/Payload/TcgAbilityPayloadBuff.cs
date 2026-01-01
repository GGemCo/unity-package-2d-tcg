namespace GGemCo2DTcg
{
    public class TcgAbilityPayloadBuff
    {
        public TcgAbilityConstants.TcgAbilityType Type { get; set; }
        public int BuffValue { get; }

        public TcgAbilityPayloadBuff(TcgAbilityConstants.TcgAbilityType type, int buffValue)
        {
            Type = type;
            BuffValue = buffValue;
        }
    }
}