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
    }
}