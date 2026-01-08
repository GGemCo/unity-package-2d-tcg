using GGemCo2DCore;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GGemCo2DTcg
{
    /// <summary>
    /// "내 덱(My Deck)" 목록에서 덱 하나를 표시하는 UI 아이콘 컴포넌트입니다.
    /// - 덱 이름 표시
    /// - 선택(좌클릭) / 컨텍스트 메뉴(우클릭)
    /// - 삭제 버튼 클릭 시 확인 팝업 표시 및 삭제 처리
    /// </summary>
    public class UIIconMyDeck : UIIcon, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header(UIWindowConstants.TitleHeaderIndividual)]
        [Tooltip("덱 이름")]
        public TextMeshProUGUI textName;

        [Tooltip("덱 삭제 버튼")]
        public Button buttonDelete;

        private UIWindowTcgMyDeck _windowTcgMyDeck;
        private PopupManager _popupManager;

        /// <summary>
        /// Unity 생명주기: 오브젝트 초기화 시 호출됩니다.
        /// 드래그를 비활성화하고, 삭제 버튼 클릭 이벤트를 연결합니다.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            SetDrag(false);
            buttonDelete?.onClick.AddListener(OnClickDelete);
        }

        /// <summary>
        /// Unity 생명주기: 첫 프레임 시작 시 호출됩니다.
        /// 팝업 매니저와 덱 목록 윈도우 참조를 캐싱합니다.
        /// </summary>
        protected override void Start()
        {
            base.Start();
            _popupManager = SceneGame.Instance.popupManager;
            _windowTcgMyDeck = SceneGame.Instance.uIWindowManager
                .GetUIWindowByUid<UIWindowTcgMyDeck>(UIWindowConstants.WindowUid.TcgMyDeck);
        }

        /// <summary>
        /// 덱 인덱스를 UID로 사용하여 아이콘 표시 정보를 갱신합니다.
        /// - <paramref name="deckIndex"/>를 <see cref="UIIcon.uid"/>에 저장하여 덱을 식별합니다.
        /// - 저장된 덱 정보로부터 덱 이름을 표시합니다.
        /// </summary>
        /// <param name="deckIndex">표시할 덱의 인덱스(이 아이콘의 UID로 사용)입니다.</param>
        /// <param name="iconCount">아이콘에 표시할 수량(기본 구현과의 호환용)입니다.</param>
        /// <param name="iconLevel">아이콘 레벨(기본 구현과의 호환용)입니다.</param>
        /// <param name="iconIsLearn">학습/해금 여부(기본 구현과의 호환용)입니다.</param>
        /// <param name="remainCoolTime">남은 쿨타임(기본 구현과의 호환용)입니다.</param>
        /// <returns>정보 갱신 성공 시 true를 반환합니다.</returns>
        public override bool ChangeInfoByUid(
            int deckIndex,
            int iconCount = 0,
            int iconLevel = 0,
            bool iconIsLearn = false,
            int remainCoolTime = 0)
        {
            // 덱 index 번호를 고유번호(uid)로 사용
            uid = deckIndex;
            SetCount(iconCount);

            var data = TcgPackageManager.Instance.saveDataManagerTcg.MyDeck.GetDeckInfoByIndex(deckIndex);
            if (textName != null)
            {
                textName.text = data.deckName;
            }

            UpdateInfo();
            return true;
        }

        /// <summary>
        /// 아이콘 이미지 업데이트 훅입니다.
        /// 덱 아이콘은 별도의 이미지 변경이 없으므로 비워둡니다.
        /// </summary>
        protected override void UpdateIconImage()
        {
        }

        /// <summary>
        /// 마우스 포인터가 아이콘 위로 올라오면 오버(하이라이트) 표시를 켭니다.
        /// </summary>
        /// <param name="eventData">포인터 이벤트 데이터입니다.</param>
        public void OnPointerEnter(PointerEventData eventData)
        {
            ShowOverImage(true);
        }

        /// <summary>
        /// 마우스 클릭 입력을 처리합니다.
        /// - 좌클릭: 해당 아이콘을 선택 상태로 설정
        /// - 우클릭: 컨텍스트 동작(예: 상세/관리 메뉴)을 호출
        /// </summary>
        /// <param name="eventData">포인터 이벤트 데이터입니다.</param>
        public void OnPointerClick(PointerEventData eventData)
        {
            // GcLogger.Log($"UIElementMyDeck OnPointerClick");
            if (!PossibleClick) return;
            if (IsLock()) return;

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (!window) return;
                window.SetSelectedIcon(index);
            }
            else if (eventData.button == PointerEventData.InputButton.Middle)
            {
                // 미사용: 필요 시 중클릭 액션을 추가할 수 있습니다.
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                // 유효한 덱(UID)이며 수량이 있을 때만 우클릭 동작을 허용합니다.
                if (uid <= 0 || GetCount() <= 0) return;
                window.OnRightClick(this);
            }
        }

        /// <summary>
        /// 마우스 포인터가 아이콘을 벗어나면 오버(하이라이트) 표시를 끕니다.
        /// </summary>
        /// <param name="eventData">포인터 이벤트 데이터입니다.</param>
        public void OnPointerExit(PointerEventData eventData)
        {
            // GcLogger.Log("OnPointerExit "+eventData);
            ShowOverImage(false);
        }

        /// <summary>
        /// 삭제 버튼 클릭 시 호출됩니다.
        /// 확인 팝업을 띄우고, 확인 시 실제 삭제(<see cref="OnDelete"/>)를 수행합니다.
        /// </summary>
        private void OnClickDelete()
        {
            if (!_popupManager) return;

            var popupMetadata = new PopupMetadata
            {
                PopupType = PopupManager.Type.Default,
                Title = "Tcg_Popup_DeleteDeck",
                Message = "Tcg_Popup_ConfirmDeleteDeckWarning",
                MessageColor = Color.red,
                OnConfirm = OnDelete,
                ShowCancelButton = true
            };

            _popupManager.ShowPopup(popupMetadata);
        }

        /// <summary>
        /// 삭제 확인 팝업에서 "확인"을 눌렀을 때 호출됩니다.
        /// 현재 아이콘 인덱스를 기준으로 덱 삭제를 요청합니다.
        /// </summary>
        private void OnDelete()
        {
            _windowTcgMyDeck?.RemoveDeck(index);
        }

        /// <summary>
        /// 기본 덱 표시 여부에 따라 아이콘 색상을 변경합니다.
        /// </summary>
        /// <param name="isDefault">기본 덱이면 true입니다.</param>
        public void SetDefault(bool isDefault)
        {
            ImageIcon.color = isDefault ? Color.yellow : Color.white;
        }
    }
}
