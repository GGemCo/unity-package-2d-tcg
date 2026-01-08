using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// Permanent 타입 카드의 능력(Ability) 및 지속(Lifetime) 정보를 담는 상세 테이블 행(row) 데이터입니다.
    /// <see cref="ITcgCardAbilityRow"/> 계약에 따라 기본 능력 정의를 제공하며,
    /// 추가로 스택, 발동 주기, 수명 규칙을 포함합니다.
    /// </summary>
    public sealed class StruckTableTcgCardPermanent : ITcgCardAbilityRow
    {
        /// <summary>
        /// 카드의 고유 식별자(Uid)입니다.
        /// 기본 카드 테이블(<c>TableTcgCard</c>)과 결합할 때 사용됩니다.
        /// </summary>
        public int Uid { get; set; }

        /// <summary>Permanent 카드가 가지는 능력의 종류입니다.</summary>
        public TcgAbilityConstants.TcgAbilityType AbilityType { get; set; }

        /// <summary>능력이 발동되는 시점(Trigger)입니다.</summary>
        public TcgAbilityConstants.TcgAbilityTriggerType TcgAbilityTriggerType { get; set; }

        /// <summary>능력이 적용될 대상(Target)의 종류입니다.</summary>
        public TcgAbilityConstants.TcgAbilityTargetType TcgAbilityTargetType { get; set; }

        /// <summary>
        /// 능력 파라미터 A입니다.
        /// TODO: AbilityType에 따라 의미(예: 수치/효과량/턴 수 등)가 달라질 수 있습니다.
        /// </summary>
        public int ParamA { get; set; }

        /// <summary>
        /// 능력 파라미터 B입니다.
        /// TODO: AbilityType에 따라 의미(예: 보조 수치/조건 값 등)가 달라질 수 있습니다.
        /// </summary>
        public int ParamB { get; set; }

        /// <summary>
        /// 능력 파라미터 C입니다.
        /// TODO: AbilityType에 따라 의미(예: 추가 옵션/플래그 등)가 달라질 수 있습니다.
        /// </summary>
        public int ParamC { get; set; }

        /// <summary>
        /// 능력이 발동되는 턴 간격입니다.
        /// 예: 1이면 매 트리거마다, 2이면 2턴마다 발동됩니다.
        /// </summary>
        public int intervalTurn;

        /// <summary>
        /// 동일한 Permanent 효과가 중첩될 수 있는 최대 스택 수입니다.
        /// 0이면 스택 제한이 없거나 스택 개념을 사용하지 않음을 의미할 수 있습니다.
        /// </summary>
        public int maxStacks;

        // -------------------------
        // Lifetime (Optional)
        // -------------------------

        /// <summary>
        /// Permanent 카드가 언제까지 유지되는지를 정의하는 수명(Lifetime) 타입입니다.
        /// 테이블 컬럼이 없으면 <see cref="ConfigCommonTcg.TcgPermanentLifetimeType.Indefinite"/>로 동작합니다.
        /// </summary>
        public ConfigCommonTcg.TcgPermanentLifetimeType lifetimeType;

        /// <summary>
        /// lifetimeType 별 파라미터 A입니다.
        /// - Durability: 내구도(사용/발동 가능 횟수)
        /// - DurationTurns: 남은 턴 수
        /// - TriggerCount: 남은 트리거(발동) 횟수
        /// </summary>
        public int lifetimeParamA;

        /// <summary>
        /// lifetimeType 별 파라미터 B입니다.
        /// - DurationTurns: 감소 기준 트리거(예: OnTurnEnd)
        /// </summary>
        public int lifetimeParamB;
    }

    /// <summary>
    /// Permanent 타입 카드의 능력 및 수명 정보를 로드하는 테이블 클래스입니다.
    /// 기본 카드 테이블과 Uid를 기준으로 결합되어 지속 효과 로직에 사용됩니다.
    /// </summary>
    public sealed class TableTcgCardPermanent : DefaultTable<StruckTableTcgCardPermanent>
    {
        /// <summary>
        /// Addressables(또는 테이블 로더)에서 Permanent 카드 테이블을 식별하기 위한 키입니다.
        /// </summary>
        public override string Key => ConfigAddressableTableTcg.TcgCardPermanent;

        /// <summary>
        /// 테이블 한 행의 원시(Dictionary) 데이터를 <see cref="StruckTableTcgCardPermanent"/>로 변환합니다.
        /// 필수 컬럼은 강제 파싱하며, 선택 컬럼은 기본값을 사용합니다.
        /// </summary>
        /// <param name="data">컬럼명-문자열 값 형태의 원시 행 데이터입니다.</param>
        /// <returns>변환된 Permanent 카드 상세 행 데이터입니다.</returns>
        /// <exception cref="KeyNotFoundException">
        /// 필수 컬럼(Uid, AbilityType, TriggerType, TargetType, ParamA, ParamB, ParamC)이 누락된 경우 발생할 수 있습니다.
        /// </exception>
        protected override StruckTableTcgCardPermanent BuildRow(Dictionary<string, string> data)
        {
            return new StruckTableTcgCardPermanent
            {
                Uid = MathHelper.ParseInt(data["Uid"]),
                AbilityType = EnumHelper.ConvertEnum<TcgAbilityConstants.TcgAbilityType>(data["AbilityType"]),
                TcgAbilityTriggerType = EnumHelper.ConvertEnum<TcgAbilityConstants.TcgAbilityTriggerType>(data["TriggerType"]),
                TcgAbilityTargetType = EnumHelper.ConvertEnum<TcgAbilityConstants.TcgAbilityTargetType>(data["TargetType"]),
                ParamA = MathHelper.ParseInt(data["ParamA"]),
                ParamB = MathHelper.ParseInt(data["ParamB"]),
                ParamC = MathHelper.ParseInt(data["ParamC"]),

                intervalTurn = data.TryGetValue("IntervalTurn", out var interval)
                    ? MathHelper.ParseInt(interval)
                    : 1,

                maxStacks = data.TryGetValue("MaxStacks", out var ms)
                    ? MathHelper.ParseInt(ms)
                    : 0,

                lifetimeType = data.TryGetValue("LifetimeType", out var lt)
                    ? EnumHelper.ConvertEnum<ConfigCommonTcg.TcgPermanentLifetimeType>(lt)
                    : ConfigCommonTcg.TcgPermanentLifetimeType.Indefinite,

                lifetimeParamA = data.TryGetValue("LifetimeParamA", out var lpa)
                    ? MathHelper.ParseInt(lpa)
                    : 0,

                lifetimeParamB = data.TryGetValue("LifetimeParamB", out var lpb)
                    ? MathHelper.ParseInt(lpb)
                    : 0,
            };
        }
    }
}
