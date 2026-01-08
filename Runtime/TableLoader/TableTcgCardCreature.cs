using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// Creature 타입 카드의 상세 테이블 행(row) 데이터입니다.
    /// 공격력, 체력, 키워드 등 전투와 직접적으로 관련된 스탯을 포함합니다.
    /// </summary>
    public class StruckTableTcgCardCreature
    {
        /// <summary>카드의 고유 식별자(Uid)입니다.</summary>
        public int uid;

        /// <summary>Creature 카드의 공격력(Attack)입니다.</summary>
        public int attack;

        /// <summary>Creature 카드의 체력(Health)입니다.</summary>
        public int health;

        /// <summary>
        /// Creature 카드에 부여된 키워드 목록입니다.
        /// 능력 테이블/시스템과의 연계를 위해 문자열 형태로 유지됩니다.
        /// </summary>
        public string keywords;
    }

    /// <summary>
    /// Creature 타입 카드의 상세 테이블을 로드하는 테이블 클래스입니다.
    /// <see cref="StruckTableTcgCard"/>와 Uid를 기준으로 결합되어 사용됩니다.
    /// </summary>
    public class TableTcgCardCreature : DefaultTable<StruckTableTcgCardCreature>
    {
        /// <summary>
        /// Addressables(또는 테이블 로더)에서 Creature 카드 테이블을 식별하기 위한 키입니다.
        /// </summary>
        public override string Key => ConfigAddressableTableTcg.TcgCardCreature;

        /// <summary>
        /// 테이블 한 행의 원시(Dictionary) 데이터를 <see cref="StruckTableTcgCardCreature"/>로 변환합니다.
        /// Creature 카드의 전투 스탯과 키워드를 파싱합니다.
        /// </summary>
        /// <param name="data">컬럼명-문자열 값 형태의 원시 행 데이터입니다.</param>
        /// <returns>변환된 Creature 카드 상세 행 데이터입니다.</returns>
        /// <exception cref="KeyNotFoundException">
        /// 필수 컬럼(Uid, Attack, Health, Keywords)이 누락된 경우 발생할 수 있습니다.
        /// </exception>
        protected override StruckTableTcgCardCreature BuildRow(Dictionary<string, string> data)
        {
            return new StruckTableTcgCardCreature
            {
                uid = MathHelper.ParseInt(data["Uid"]),
                attack = MathHelper.ParseInt(data["Attack"]),
                health = MathHelper.ParseInt(data["Health"]),
                keywords = data["Keywords"],
            };
        }
    }
}