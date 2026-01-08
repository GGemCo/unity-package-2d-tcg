namespace GGemCo2DTcg
{
    /// <summary>
    /// 필드(Field)에 배치되는 플레이어 영웅(Hero) 카드 아이콘을 표현하는 UI 컴포넌트입니다.
    /// UIIconFieldPlayer를 상속하며, 영웅 전용 UI 규칙(테두리, 마나 아이콘 비활성화)을 적용합니다.
    /// </summary>
    public class UIIconFieldPlayerHero : UIIconFieldPlayer
    {
        /// <summary>
        /// 컴포넌트 초기화 시 호출됩니다.
        /// 부모 초기화 이후, 영웅 카드에 맞는 핸드(Hand) 테두리 이미지 키 접두사를 설정합니다.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            borderKeyPrefix = $"{ConfigAddressableKeyTcg.Card.ImageBorderHand}_";
        }

        /// <summary>
        /// 카드 UID를 기준으로 영웅 카드 아이콘 정보를 변경합니다.
        /// </summary>
        /// <param name="cardUid">변경할 카드의 고유 UID</param>
        /// <param name="iconCount">아이콘에 표시할 카드 수량</param>
        /// <param name="iconLevel">카드 레벨</param>
        /// <param name="iconIsLearn">카드 학습 여부</param>
        /// <param name="remainCoolTime">남은 쿨타임</param>
        /// <returns>
        /// 카드 정보 변경에 성공하면 true, 실패하면 false를 반환합니다.
        /// </returns>
        /// <remarks>
        /// 영웅 카드는 마나 비용 개념을 사용하지 않으므로,
        /// 카드 정보 변경 이후 마나 이미지 UI를 비활성화합니다.
        /// </remarks>
        public override bool ChangeInfoByUid(
            int cardUid,
            int iconCount = 0,
            int iconLevel = 0,
            bool iconIsLearn = false,
            int remainCoolTime = 0)
        {
            if (!base.ChangeInfoByUid(cardUid, iconCount, iconLevel, iconIsLearn, remainCoolTime))
                return false;

            // 영웅 카드는 마나 UI를 표시하지 않음
            if (imageMana != null)
            {
                imageMana.gameObject.SetActive(false);
            }

            return true;
        }
    }
}