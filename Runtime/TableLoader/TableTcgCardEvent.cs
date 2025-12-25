using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    public sealed class StruckTableTcgCardEvent
    {
        public int uid;
        public int abilityUid;
        public TcgAbilityConstants.TcgAbilityTriggerType tcgAbilityTriggerType;

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
                abilityUid = MathHelper.ParseInt(data["AbilityUid"]),
                tcgAbilityTriggerType = EnumHelper.ConvertEnum<TcgAbilityConstants.TcgAbilityTriggerType>(data["TriggerType"]),
                consumeOnTrigger = data.TryGetValue("ConsumeOnTrigger", out var v) && MathHelper.ParseInt(v) != 0
            };
        }
    }
}
