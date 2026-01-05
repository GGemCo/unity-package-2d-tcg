using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    public sealed class StruckTableTcgCardPermanent : ITcgCardAbilityRow
    {
        public int uid { get; set; }
        public TcgAbilityConstants.TcgAbilityType abilityType { get; set; }
        public TcgAbilityConstants.TcgAbilityTriggerType tcgAbilityTriggerType { get; set; }
        public TcgAbilityConstants.TcgAbilityTargetType tcgAbilityTargetType { get; set; }
        public int paramA { get; set; }
        public int paramB { get; set; }
        public int paramC { get; set; }

        public int intervalTurn;
        public int maxStacks;

        // -------------------------
        // Lifetime (Optional)
        // -------------------------
        // Permanent 카드가 "언제까지 유지되는지"를 정의합니다.
        // - 테이블 컬럼이 없으면 기본값(Indefinite)으로 동작하도록 설계합니다.
        public ConfigCommonTcg.TcgPermanentLifetimeType lifetimeType;

        /// <summary>
        /// lifetimeType 별 파라미터A
        /// - Durability: 내구도(사용/발동 가능 횟수)
        /// - DurationTurns: 남은 턴 수
        /// - TriggerCount: 남은 트리거(발동) 횟수
        /// </summary>
        public int lifetimeParamA;

        /// <summary>
        /// lifetimeType 별 파라미터B
        /// - DurationTurns: 감소 기준 트리거(예: OnTurnEnd)
        /// </summary>
        public int lifetimeParamB;
    }

    public sealed class TableTcgCardPermanent : DefaultTable<StruckTableTcgCardPermanent>
    {
        public override string Key => ConfigAddressableTableTcg.TcgCardPermanent;

        protected override StruckTableTcgCardPermanent BuildRow(Dictionary<string, string> data)
        {
            return new StruckTableTcgCardPermanent
            {
                uid = MathHelper.ParseInt(data["Uid"]),
                abilityType = EnumHelper.ConvertEnum<TcgAbilityConstants.TcgAbilityType>(data["AbilityType"]),
                tcgAbilityTriggerType = EnumHelper.ConvertEnum<TcgAbilityConstants.TcgAbilityTriggerType>(data["TriggerType"]),
                tcgAbilityTargetType = EnumHelper.ConvertEnum<TcgAbilityConstants.TcgAbilityTargetType>(data["TargetType"]),
                paramA = MathHelper.ParseInt(data["ParamA"]),
                paramB = MathHelper.ParseInt(data["ParamB"]),
                paramC = MathHelper.ParseInt(data["ParamC"]),
                intervalTurn = data.TryGetValue("IntervalTurn", out var interval) ? MathHelper.ParseInt(interval) : 1,
                maxStacks = data.TryGetValue("MaxStacks", out var ms) ? MathHelper.ParseInt(ms) : 0,
                lifetimeType = data.TryGetValue("LifetimeType", out var lt)
                    ? EnumHelper.ConvertEnum<ConfigCommonTcg.TcgPermanentLifetimeType>(lt)
                    : ConfigCommonTcg.TcgPermanentLifetimeType.Indefinite,
                lifetimeParamA = data.TryGetValue("LifetimeParamA", out var lpa) ? MathHelper.ParseInt(lpa) : 0,
                lifetimeParamB = data.TryGetValue("LifetimeParamB", out var lpb) ? MathHelper.ParseInt(lpb) : 0,
            };
        }
    }
}