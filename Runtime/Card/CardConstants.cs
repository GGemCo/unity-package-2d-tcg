namespace GGemCo2DTcg
{
    public static class CardConstants
    {
        /// <summary>
        /// 카드의 상위 타입.
        /// Creature, Spell, Equipment, Permanent, Event 등
        /// </summary>
        public enum Type
        {
            /// <summary>
            /// 타입을 구분하지 않음.
            /// 필터에서 사용 시, 어떤 타입이든 허용됩니다.
            /// </summary>
            Any,

            /// <summary>
            /// 크리처 카드.
            /// 전장에 소환되어 전투에 참여하는 유닛.
            /// </summary>
            Creature,

            /// <summary>
            /// 스펠 카드.
            /// 즉시 발동되거나 일회성 효과를 가지는 카드.
            /// </summary>
            Spell,

            /// <summary>
            /// 장비/무기/방어구 등 영웅 혹은 크리처에 부착되는 카드.
            /// </summary>
            Equipment,

            /// <summary>
            /// 지속형 영속물(건물, 필드 등)을 표현하는 카드.
            /// </summary>
            Permanent,

            /// <summary>
            /// 일회성 이벤트, 트랩 등 특수 효과 카드.
            /// </summary>
            Event,
            Hero
        }

        /// <summary>
        /// 카드 희귀도. 하스스톤/MTG식으로 확장 가능.
        /// </summary>
        public enum Grade
        {
            None,
            Common,
            Magic,
            Epic,
            Legendary
        }

        /// <summary>
        /// Creature가 보유할 수 있는 고유 특성(패시브) 키워드 집합.
        /// - 모든 키워드는 공통 규칙으로 처리되며 카드 개별 능력과는 별도로 동작합니다.
        /// - 이펙트(Effect)와 달리 고정적인 성질을 나타냅니다.
        /// todo 정리 필요. ConfigCommonTcg.cs 에도 있음
        /// </summary>
        public enum KeywordType
        {
            /// <summary>
            /// 은신 상태: 적이 이 카드를 명시적으로 선택할 수 없습니다.
            /// 공격 또는 특정 행동 시 은신이 해제될 수 있습니다.
            /// </summary>
            Stealth,

            /// <summary>
            /// 재생: 자신의 턴 종료 시 일정량의 체력을 자동 회복합니다.
            /// 회복량은 카드 개별 데이터 또는 규칙 엔진에서 결정됩니다.
            /// </summary>
            Regenerate,

            /// <summary>
            /// 화상: 공격에 성공하면 적에게 지속 피해(DoT)를 부여합니다.
            /// 화상 데미지는 턴 종료 시 처리됩니다.
            /// </summary>
            Burn,

            /// <summary>
            /// 관통 공격: 공격 시 상대의 방어력을 일부 무시하고 직접 피해를 전달합니다.
            /// 방어력 무시 수치는 공통 규칙 또는 카드 능력에서 정의됩니다.
            /// </summary>
            Pierce,

            /// <summary>
            /// 기습: 전투 시작 또는 은신 상태에서 첫 공격 시 추가 피해를 제공합니다.
            /// 한 번 발동 후에는 일반 공격 규칙을 따릅니다.
            /// </summary>
            Ambush,

            /// <summary>
            /// 중갑: 받는 물리 피해를 일정 비율 감소시킵니다.
            /// Stone Golem과 같은 탱커형 Creature에 적합합니다.
            /// </summary>
            Fortified,

            /// <summary>
            /// 경직 무시: 밀침, 속박, 기절과 같은 방해 효과에 대한 저항을 가집니다.
            /// 특정 제어 효과를 무시하거나 지속 시간을 단축합니다.
            /// </summary>
            Unstoppable,

            /// <summary>
            /// 가속: 소환된 턴에도 즉시 공격하거나 행동할 수 있습니다.
            /// 일반적인 소환 후 대기 규칙을 무시합니다.
            /// </summary>
            Haste,

            /// <summary>
            /// 성장 보호막: 한 턴마다 방어력이 증가하거나 보호막을 생성합니다.
            /// Verdant Forest Guardian과 같은 자연계 Creature에서 자주 사용됩니다.
            /// </summary>
            ShieldGrowth,

            /// <summary>
            /// 자연 강화: 아군 자연(Nature) 속성 카드와 상호작용 시 강화됩니다.
            /// 속성별 시너지 시스템에서 사용됩니다.
            /// </summary>
            NatureBond,
            
            /// <summary>
            /// 번개 충격: 공격 시 추가 번개 피해가 적용되거나,
            /// 주변 적에게 연쇄 번개 효과(Chain Lightning)를 발생시킵니다.
            /// </summary>
            LightningSurge,

            /// <summary>
            /// 비행: 비행 유닛으로서 일부 지상 공격이나 지상 대상 전용 능력의 영향을 받지 않습니다.
            /// 전장 충돌 규칙에서 특별한 우선순위를 갖습니다.
            /// </summary>
            Flying,

            /// <summary>
            /// 과충전: 공격력이 일시적으로 증가하나,
            /// 일정 조건에서 반동 피해(자가 피해)를 받을 수 있습니다.
            /// 고위험·고화력 특성을 가진 비행/마법 생물의 전형적인 능력입니다.
            /// </summary>
            Overcharge
        }
    }
}