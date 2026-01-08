namespace GGemCo2DTcg
{
    /// <summary>
    /// 적 필드의 "영웅(Hero)" 카드 아이콘을 표현하는 UI 컴포넌트입니다.
    /// <see cref="UIIconFieldEnemy"/>를 확장하여 영웅 전용 UI 규칙(보더/마나 표시 등)을 적용합니다.
    /// </summary>
    public class UIIconFieldEnemyHero : UIIconFieldEnemy
    {
        /// <summary>
        /// Unity 생명주기: 오브젝트 초기화 시 호출됩니다.
        /// 영웅 전용 보더 리소스 키 프리픽스를 설정합니다.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            borderKeyPrefix = $"{ConfigAddressableKeyTcg.Card.ImageBorderHand}_";
        }

        /// <summary>
        /// 카드 UID를 기준으로 아이콘 표시 정보를 갱신합니다.
        /// 기본 갱신 로직을 수행한 뒤, 영웅 카드에 대해 마나(코스트) UI를 숨깁니다.
        /// </summary>
        /// <param name="cardUid">표시할 카드의 고유 UID입니다.</param>
        /// <param name="iconCount">수량(기본 구현과의 호환용)입니다.</param>
        /// <param name="iconLevel">레벨(기본 구현과의 호환용)입니다.</param>
        /// <param name="iconIsLearn">학습/해금 여부(기본 구현과의 호환용)입니다.</param>
        /// <param name="remainCoolTime">남은 쿨타임(기본 구현과의 호환용)입니다.</param>
        /// <returns>갱신 성공 시 true, 실패 시 false입니다.</returns>
        public override bool ChangeInfoByUid(
            int cardUid,
            int iconCount = 0,
            int iconLevel = 0,
            bool iconIsLearn = false,
            int remainCoolTime = 0)
        {
            if (!base.ChangeInfoByUid(cardUid, iconCount, iconLevel, iconIsLearn, remainCoolTime))
                return false;

            // 영웅은 마나(코스트) 이미지가 표시되지 않도록 처리합니다.
            if (imageMana != null)
            {
                imageMana.gameObject.SetActive(false);
            }

            return true;
        }
    }
}