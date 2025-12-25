using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 어빌리티(Ability) 정의 테이블.
    /// - 카드/영속물/이벤트 등에서 AbilityId를 참조하여 실행 로직과 파라미터를 재사용합니다.
    /// - 실제 실행 로직은 <see cref="TcgAbilityRunner"/>의 핸들러(<see cref="ITcgAbilityHandler"/>)로 위임합니다.
    /// </summary>
    public sealed class StruckTableTcgAbility
    {
        // DefaultTable 의 Key 필드 규약(uid)을 유지하기 위한 alias.
        public int uid;
        public int abilityId;
        public TcgAbilityConstants.TcgAbilityType abilityType;
        public string name;
        public TcgAbilityConstants.TcgAbilityTriggerType tcgAbilityTriggerType;
        public TcgAbilityConstants.TcgAbilityTargetType tcgAbilityTargetType;
        public int paramA;
        public int paramB;
        public int paramC;
        public string description;
    }

    public sealed class TableTcgAbility : DefaultTable<StruckTableTcgAbility>
    {
        public override string Key => ConfigAddressableTableTcg.TcgAbility;

        protected override StruckTableTcgAbility BuildRow(Dictionary<string, string> data)
        {
            return new StruckTableTcgAbility
            {
                uid = MathHelper.ParseInt(data["Uid"]),
                abilityId = MathHelper.ParseInt(data["Uid"]),
                abilityType = EnumHelper.ConvertEnum<TcgAbilityConstants.TcgAbilityType>(data["AbilityType"]),
                name = data["Name"],
                tcgAbilityTriggerType = EnumHelper.ConvertEnum<TcgAbilityConstants.TcgAbilityTriggerType>(data["TriggerType"]),
                tcgAbilityTargetType = EnumHelper.ConvertEnum<TcgAbilityConstants.TcgAbilityTargetType>(data["TargetType"]),
                paramA = MathHelper.ParseInt(data["ParamA"]),
                paramB = MathHelper.ParseInt(data["ParamB"]),
                paramC = MathHelper.ParseInt(data["ParamC"]),
                description = data.TryGetValue("Description", out var desc) ? desc : string.Empty,
            };
        }
    }
}
