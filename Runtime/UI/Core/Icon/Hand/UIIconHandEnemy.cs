using GGemCo2DCore;
using UnityEngine.EventSystems;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 적(Enemy) 손패(Hand)에 표시되는 카드 아이콘 UI 컴포넌트입니다.
    /// 카드 정보 변경과 포인터 이벤트 처리를 담당하며,
    /// 카드 정보 팝업(UIWindowTcgCardInfo) 및 선택/오버 UI 상태와 연동됩니다.
    /// </summary>
    public class UIIconHandEnemy : UIIconCard, IPointerEnterHandler, IPointerExitHandler
    {
        /// <summary>
        /// 카드 상세 정보를 표시하는 TCG 카드 정보 UI 윈도우입니다.
        /// </summary>
        private UIWindowTcgCardInfo _windowTcgCardInfo;

        /// <summary>
        /// 첫 프레임 시작 시 호출됩니다.
        /// 카드 정보 표시를 위한 UIWindowTcgCardInfo 인스턴스를 캐싱합니다.
        /// </summary>
        protected override void Start()
        {
            base.Start();
            _windowTcgCardInfo = SceneGame.Instance.uIWindowManager
                .GetUIWindowByUid<UIWindowTcgCardInfo>(UIWindowConstants.WindowUid.TcgCardInfo);
        }

        /// <summary>
        /// 오브젝트가 비활성화될 때 호출됩니다.
        /// 선택/오버 상태 UI 및 카드 정보 UI를 정리하여
        /// UI 상태가 남지 않도록 처리합니다.
        /// </summary>
        private void OnDisable()
        {
            if (IsSelected())
            {
                SceneGame.Instance.uIWindowManager.ShowSelectIconImage(false);
            }

            SceneGame.Instance.uIWindowManager.ShowOverIconImage(false);
            _windowTcgCardInfo?.Show(false);
        }

        /// <summary>
        /// 카드 UID를 기준으로 적 손패 카드 아이콘 정보를 변경합니다.
        /// </summary>
        /// <param name="cardUid">변경할 카드의 고유 UID</param>
        /// <param name="iconCount">아이콘에 표시할 카드 수량</param>
        /// <param name="iconLevel">카드 레벨</param>
        /// <param name="iconIsLearn">카드 학습 여부</param>
        /// <param name="remainCoolTime">남은 쿨타임</param>
        /// <returns>
        /// 카드 테이블에 UID가 존재하면 true, 존재하지 않으면 false를 반환합니다.
        /// </returns>
        public override bool ChangeInfoByUid(
            int cardUid,
            int iconCount = 0,
            int iconLevel = 0,
            bool iconIsLearn = false,
            int remainCoolTime = 0)
        {
            var info = tableTcgCard.GetDataByUid(cardUid);
            if (info == null)
            {
                GcLogger.LogError($"tcg_card 테이블에 없는 카드 입니다. uid: {cardUid}");
                return false;
            }

            base.ChangeInfoByUid(cardUid, iconCount, iconLevel, iconIsLearn, remainCoolTime);
            return true;
        }

        /// <summary>
        /// 마우스 포인터가 카드 아이콘 영역에 진입했을 때 호출됩니다.
        /// </summary>
        /// <param name="eventData">포인터 이벤트 데이터</param>
        /// <remarks>
        /// 현재는 동작이 구현되어 있지 않으며,
        /// 추후 카드 정보 미리보기 또는 오버 UI 표시 로직이 추가될 수 있습니다.
        /// </remarks>
        public void OnPointerEnter(PointerEventData eventData)
        {
        }

        /// <summary>
        /// 마우스 포인터가 카드 아이콘 영역에서 벗어났을 때 호출됩니다.
        /// </summary>
        /// <param name="eventData">포인터 이벤트 데이터</param>
        /// <remarks>
        /// 포인터 진입 시 처리와 쌍을 이루는 UI 정리 로직이
        /// 향후 구현될 수 있습니다.
        /// </remarks>
        public void OnPointerExit(PointerEventData eventData)
        {
        }

        /// <summary>
        /// 카드 아이콘을 클릭했을 때 호출됩니다.
        /// </summary>
        /// <param name="eventData">포인터 클릭 이벤트 데이터</param>
        /// <remarks>
        /// 덱이 선택된 상태라면 클릭 시 해당 카드를 즉시 덱에 포함시키는 용도로 사용됩니다.
        /// 실제 동작 로직은 아직 구현되어 있지 않습니다.
        /// </remarks>
        public override void OnPointerClick(PointerEventData eventData)
        {
        }
    }
}
