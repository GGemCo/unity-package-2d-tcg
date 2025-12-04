namespace GGemCo2DTcg
{
    public class ConfigCommonTcg
    {
        /// <summary>
        /// 덱 셔플 동작 모드.
        /// </summary>
        public enum ShuffleMode
        {
            /// <summary>
            /// 완전 랜덤 셔플 (Fisher–Yates).
            /// </summary>
            PureRandom,

            /// <summary>
            /// 코스트 등 가중치를 고려한 셔플.
            /// 초기 손패 쪽에 저코스트가 조금 더 잘 배치되도록 조정하는 등의 용도.
            /// </summary>
            Weighted,

            /// <summary>
            /// 고정 시드를 사용하여 결과를 재현 가능한 셔플.
            /// 리플레이, PVP 검증 등에 사용.
            /// </summary>
            SeededReplay
        }
    }
}