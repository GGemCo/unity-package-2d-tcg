using GGemCo2DCore;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GGemCo2DTcg
{
    public class UIIconMyDeck : UIIcon, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header(UIWindowConstants.TitleHeaderIndividual)] 
        [Tooltip("덱 이름")]
        public TextMeshProUGUI textName;

        [Tooltip("덱 삭제 버튼")] 
        public Button buttonDelete;

        private UIWindowMyDeck _windowMyDeck;
        private PopupManager _popupManager;

        protected override void Awake()
        {
            base.Awake();
            SetDrag(false);
            buttonDelete?.onClick.AddListener(OnClickDelete);
        }

        protected override void Start()
        {
            base.Start();
            _popupManager = SceneGame.Instance.popupManager;
            _windowMyDeck = SceneGame.Instance.uIWindowManager.GetUIWindowByUid<UIWindowMyDeck>(UIWindowConstants.WindowUid.TcgMyDeck);
        }

        public override bool ChangeInfoByUid(int deckIndex, int iconCount = 0, int iconLevel = 0, bool iconIsLearn = false, int remainCoolTime = 0)
        {
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
        /// 아이콘 이미지 업데이트 하기
        /// </summary>
        protected override void UpdateIconImage()
        {
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            ShowOverImage(true);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // GcLogger.Log($"UIElementMyDeck OnPointerClick");
            if (!PossibleClick) return;
            if (IsLock()) return;
            if(eventData.button == PointerEventData.InputButton.Left)
            {
                if (!window) return;
                window.SetSelectedIcon(index);
            }
            else if(eventData.button == PointerEventData.InputButton.Middle)
            {
            }
            else if(eventData.button == PointerEventData.InputButton.Right)
            {
                if (uid <= 0 || GetCount() <= 0) return;
                window.OnRightClick(this);
            }
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            // GcLogger.Log("OnPointerExit "+eventData);
            ShowOverImage(false);
        }

        private void OnClickDelete()
        {
            if (!_popupManager) return;
            var popupMetadata = new PopupMetadata
            {
                PopupType = PopupManager.Type.Default,
                Title = "덱 삭제하기",
                Message = "정말로 삭제하시겠습니까?",
                MessageColor = Color.red,
                OnConfirm = OnDelete,
                ShowCancelButton = true
            };
            _popupManager.ShowPopup(popupMetadata);
        }

        private void OnDelete()
        {
            _windowMyDeck?.RemoveDeck(index);
        }
    }
}