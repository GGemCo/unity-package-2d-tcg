namespace GGemCo2DTcg
{
    /// <summary>
    /// TCG 어빌리티 효과 중 버프(Buff) 정보를 전달하기 위한 페이로드 클래스입니다.
    /// </summary>
    /// <remarks>
    /// 주로 어빌리티 처리 로직에서
    /// “어떤 타입의 어빌리티가 얼마만큼의 수치를 변경하는지”를 묶어서 전달하는 용도로 사용됩니다.
    /// </remarks>
    public class TcgAbilityPayloadBuff
    {
        /// <summary>
        /// 적용할 어빌리티 타입입니다.
        /// </summary>
        public TcgAbilityConstants.TcgAbilityType Type { get; set; }

        /// <summary>
        /// 버프로 적용될 수치 값입니다.
        /// </summary>
        /// <remarks>
        /// 양수/음수 여부에 따라 강화 또는 약화 효과로 해석될 수 있습니다.
        /// </remarks>
        public int BuffValue { get; }

        /// <summary>
        /// 버프 어빌리티 페이로드를 생성합니다.
        /// </summary>
        /// <param name="type">적용할 어빌리티 타입입니다.</param>
        /// <param name="buffValue">버프로 적용할 수치 값입니다.</param>
        public TcgAbilityPayloadBuff(
            TcgAbilityConstants.TcgAbilityType type,
            int buffValue)
        {
            Type = type;
            BuffValue = buffValue;
        }
    }
}