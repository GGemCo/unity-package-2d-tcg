namespace GGemCo2DTcg
{
    /// <summary>
    /// 카드 시스템 전반에서 사용되는 공통 상수 정의 클래스입니다.
    /// 카드 타입, 희귀도 등 카드의 정적 분류 정보를 포함합니다.
    /// </summary>
    public static class CardConstants
    {
        /// <summary>
        /// 카드의 상위 타입을 정의합니다.
        /// Creature, Spell, Equipment, Permanent, Event, Hero 등으로 구분됩니다.
        /// </summary>
        public enum Type
        {
            /// <summary>
            /// 타입을 구분하지 않음을 의미합니다.
            /// 카드 검색 또는 필터링 시 모든 타입을 허용할 때 사용됩니다.
            /// </summary>
            Any,

            /// <summary>
            /// 크리처 카드입니다.
            /// 전장에 소환되어 공격 및 방어 등 전투에 직접 참여하는 유닛을 의미합니다.
            /// </summary>
            Creature,

            /// <summary>
            /// 스펠 카드입니다.
            /// 사용 즉시 발동되며, 일회성 효과를 수행하는 마법 또는 기술 카드입니다.
            /// </summary>
            Spell,

            /// <summary>
            /// 장비 카드입니다.
            /// 무기, 방어구 등으로 영웅 또는 크리처에 부착되어 능력치를 강화합니다.
            /// </summary>
            Equipment,

            /// <summary>
            /// 영속 카드입니다.
            /// 건물, 필드 효과 등 전장에 지속적으로 영향을 미치는 오브젝트를 의미합니다.
            /// </summary>
            Permanent,

            /// <summary>
            /// 이벤트 카드입니다.
            /// 트랩, 특수 트리거 등 특정 조건에서 발동되는 일회성 효과를 나타냅니다.
            /// </summary>
            Event,

            /// <summary>
            /// 영웅 카드입니다.
            /// 플레이어를 대표하는 핵심 유닛으로, 고유 능력과 장비 슬롯 등을 가질 수 있습니다.
            /// </summary>
            Hero
        }

        /// <summary>
        /// 카드의 희귀도를 정의합니다.
        /// 하스스톤(Hearthstone), MTG 스타일의 희귀도 체계를 기준으로 확장 가능합니다.
        /// </summary>
        public enum Grade
        {
            /// <summary>
            /// 희귀도가 정의되지 않은 상태를 의미합니다.
            /// 내부 테스트 또는 특수 카드에 사용될 수 있습니다.
            /// </summary>
            None,

            /// <summary>
            /// 일반(Common) 카드입니다.
            /// 가장 기본적인 카드로, 높은 획득 확률을 가집니다.
            /// </summary>
            Common,

            /// <summary>
            /// 매직(Magic) 카드입니다.
            /// 일반 카드보다 강력하거나 특수한 효과를 가집니다.
            /// </summary>
            Magic,

            /// <summary>
            /// 에픽(Epic) 카드입니다.
            /// 게임 플레이에 큰 영향을 주는 강력한 효과를 가진 희귀 카드입니다.
            /// </summary>
            Epic,

            /// <summary>
            /// 전설(Legendary) 카드입니다.
            /// 매우 희귀하며, 고유하거나 게임의 흐름을 바꾸는 핵심 카드입니다.
            /// </summary>
            Legendary
        }
    }
}
