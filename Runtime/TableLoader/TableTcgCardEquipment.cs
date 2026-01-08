using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// Equipment 타입 카드의 능력(Ability) 정의를 담는 상세 테이블 행(row) 데이터입니다.
    /// <see cref="ITcgCardAbilityRow"/> 계약에 따라 능력 타입/트리거/타겟 및 파라미터들을 제공합니다.
    /// </summary>
    public sealed class StruckTableTcgCardEquipment : ITcgCardAbilityRow
    {
        /// <summary>
        /// 카드의 고유 식별자(Uid)입니다.
        /// 기본 카드 테이블(<c>TableTcgCard</c>)과 결합할 때 키로 사용됩니다.
        /// </summary>
        public int Uid { get; set; }

        /// <summary>
        /// 장비 카드가 가지는 능력의 종류입니다.
        /// </summary>
        public TcgAbilityConstants.TcgAbilityType AbilityType { get; set; }

        /// <summary>
        /// 능력이 발동되는 시점(Trigger)입니다.
        /// </summary>
        public TcgAbilityConstants.TcgAbilityTriggerType TcgAbilityTriggerType { get; set; }

        /// <summary>
        /// 능력이 적용될 대상(Target)의 종류입니다.
        /// </summary>
        public TcgAbilityConstants.TcgAbilityTargetType TcgAbilityTargetType { get; set; }

        /// <summary>
        /// 능력 파라미터 A입니다.
        /// TODO: AbilityType에 따라 의미(예: 수치/횟수/턴 수 등)가 달라질 수 있습니다.
        /// </summary>
        public int ParamA { get; set; }

        /// <summary>
        /// 능력 파라미터 B입니다.
        /// TODO: AbilityType에 따라 의미(예: 보조 수치/조건 값 등)가 달라질 수 있습니다.
        /// </summary>
        public int ParamB { get; set; }

        /// <summary>
        /// 능력 파라미터 C입니다.
        /// TODO: AbilityType에 따라 의미(예: 추가 옵션/확률/플래그 등)가 달라질 수 있습니다.
        /// </summary>
        public int ParamC { get; set; }
    }

    /// <summary>
    /// Equipment 타입 카드의 능력(Ability) 상세 테이블을 로드하는 테이블 클래스입니다.
    /// 로드된 행 데이터는 Uid를 기준으로 기본 카드 테이블과 결합되어 사용됩니다.
    /// </summary>
    public sealed class TableTcgCardEquipment : DefaultTable<StruckTableTcgCardEquipment>
    {
        /// <summary>
        /// Addressables(또는 테이블 로더)에서 장비 카드 테이블을 식별하기 위한 키입니다.
        /// </summary>
        public override string Key => ConfigAddressableTableTcg.TcgCardEquipment;

        /// <summary>
        /// 테이블 한 행의 원시(Dictionary) 데이터를 <see cref="StruckTableTcgCardEquipment"/>로 변환합니다.
        /// 능력 타입/트리거/타겟과 파라미터(A/B/C)를 파싱합니다.
        /// </summary>
        /// <param name="data">컬럼명-문자열 값 형태의 원시 행 데이터입니다.</param>
        /// <returns>변환된 장비 카드 능력 행 데이터입니다.</returns>
        /// <exception cref="KeyNotFoundException">
        /// 필수 컬럼(Uid, AbilityType, TriggerType, TargetType, ParamA, ParamB, ParamC)이 누락된 경우 발생할 수 있습니다.
        /// </exception>
        protected override StruckTableTcgCardEquipment BuildRow(Dictionary<string, string> data)
        {
            return new StruckTableTcgCardEquipment
            {
                Uid = MathHelper.ParseInt(data["Uid"]),
                AbilityType = EnumHelper.ConvertEnum<TcgAbilityConstants.TcgAbilityType>(data["AbilityType"]),
                TcgAbilityTriggerType = EnumHelper.ConvertEnum<TcgAbilityConstants.TcgAbilityTriggerType>(data["TriggerType"]),
                TcgAbilityTargetType = EnumHelper.ConvertEnum<TcgAbilityConstants.TcgAbilityTargetType>(data["TargetType"]),
                ParamA = MathHelper.ParseInt(data["ParamA"]),
                ParamB = MathHelper.ParseInt(data["ParamB"]),
                ParamC = MathHelper.ParseInt(data["ParamC"]),
            };
        }
    }
}
