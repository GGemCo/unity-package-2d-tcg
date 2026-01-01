using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    public sealed class StruckTableTcgCardEvent : ITcgCardAbilityRow
    {
        public int uid { get; set; }
        public TcgAbilityConstants.TcgAbilityType abilityType { get; set; }
        public TcgAbilityConstants.TcgAbilityTriggerType tcgAbilityTriggerType { get; set; }
        public TcgAbilityConstants.TcgAbilityTargetType tcgAbilityTargetType { get; set; }
        public int paramA { get; set; }
        public int paramB { get; set; }
        public int paramC { get; set; }

        /// <summary>
        /// 트리거 발동 후 카드를 소모(묘지로 이동)할지 여부.
        /// </summary>
        public bool consumeOnTrigger;
    }

    public sealed class TableTcgCardEvent : DefaultTable<StruckTableTcgCardEvent>
    {
        public override string Key => ConfigAddressableTableTcg.TcgCardEvent;

        protected override StruckTableTcgCardEvent BuildRow(Dictionary<string, string> data)
        {
            return new StruckTableTcgCardEvent
            {
                uid = MathHelper.ParseInt(data["Uid"]),
                abilityType = EnumHelper.ConvertEnum<TcgAbilityConstants.TcgAbilityType>(data["AbilityType"]),
                tcgAbilityTriggerType = EnumHelper.ConvertEnum<TcgAbilityConstants.TcgAbilityTriggerType>(data["TriggerType"]),
                tcgAbilityTargetType = EnumHelper.ConvertEnum<TcgAbilityConstants.TcgAbilityTargetType>(data["TargetType"]),
                paramA = MathHelper.ParseInt(data["ParamA"]),
                paramB = MathHelper.ParseInt(data["ParamB"]),
                paramC = MathHelper.ParseInt(data["ParamC"]),
                consumeOnTrigger = ConvertBoolean(data["ConsumeOnTrigger"])
            };
        }
    }
}