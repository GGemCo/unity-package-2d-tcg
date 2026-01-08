using System.Collections.Generic;
using GGemCo2DCore;
using UnityEngine;
using Button = UnityEngine.UI.Button;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 나의 덱 리스트 윈도우
    /// </summary>
    public class UIWindowTcgMyDeck : UIWindow
    {
        [Header(UIWindowConstants.TitleHeaderIndividual)]
        [Tooltip("새로운 덱 만들기 버튼")]
        public Button buttonCreateNew;

        private MyDeckData _myDeckData;
        private PlayerDataTcg _playerDataTcg;

        private UIWindowInputField _windowInputField;
        private UIWindowTcgMyDeckCard _windowTcgMyDeckCard;
        private string _titleInputField;
        // 디폴트로 설정한 덱 아이콘
        private UIIconMyDeck _defaultIconMyDeck;
        
        protected override void Awake()
        {
            uid = UIWindowConstants.WindowUid.TcgMyDeck;
            if (TableLoaderManager.Instance == null) return;
            
            base.Awake();
            
            IconPoolManager.SetSetIconHandler(new SetIconHandlerMyDeck());
            DragDropHandler.SetStrategy(new DragDropStrategyMyDeck());
            
            buttonCreateNew?.onClick.AddListener(OnClickCreateNew);
        }

        private void OnDestroy()
        {
            buttonCreateNew?.onClick.RemoveAllListeners();
        }

        protected override void Start()
        {
            base.Start();
            if (SceneGame != null && TcgPackageManager.Instance.saveDataManagerTcg != null)
            {
                _myDeckData = TcgPackageManager.Instance.saveDataManagerTcg.MyDeck;
                _playerDataTcg = TcgPackageManager.Instance.saveDataManagerTcg.PlayerTcg;
            }
            _windowInputField = SceneGame.uIWindowManager.GetUIWindowByUid<UIWindowInputField>(UIWindowConstants.WindowUid.InputField);
            _windowTcgMyDeckCard = SceneGame.uIWindowManager.GetUIWindowByUid<UIWindowTcgMyDeckCard>(UIWindowConstants.WindowUid.TcgMyDeckCard);
            _titleInputField = LocalizationManagerTcg.Instance.GetUIWindowMyDeckByKey("Button_Create_New");
        }

        public override void OnShow(bool show)
        {
            if (SceneGame == null || TableLoaderManager.Instance == null) return;
            if (show)
            {
                LoadMyDeckData();
            }
            RemoveSelectedIcon();
        }
        private void LoadMyDeckData()
        {
            var saveData = _myDeckData.GetAllData();
            for (int i = 0; i < maxCountIcon; i++)
            {
                var data = saveData.GetValueOrDefault(i);
                var uiSlot = GetSlotByIndex(i);
                if (data == null)
                {
                    uiSlot?.gameObject.SetActive(false);
                    continue;
                }
                uiSlot?.gameObject.SetActive(true);
                var uiIcon = GetIconByIndex(i);
                if (!uiIcon) continue;
                uiIcon.Initialize(this, uid, data.index, data.index, iconSize, slotSize);
                uiIcon.ChangeInfoByUid(data.index, 1, 1);
                uiIcon.SetDrag(false);
            }
            if (_playerDataTcg != null)
                SetDefaultDeck(_playerDataTcg.defaultDeckIndex);
        }

        private void OnClickCreateNew()
        {
            if (_myDeckData.GetCurrentCount() >= maxCountIcon)
            {
                SceneGame.systemMessageManager.ShowMessageWarning("Tcg_System_MaxDeckCount", maxCountIcon);
                return;
            }
            _windowInputField.UpdateInfo(_titleInputField, OnCreateNew);
        }

        private void OnCreateNew(string deckName)
        {
            // GcLogger.Log($"OnCreateNew {deckName}");
            var index = _myDeckData.AddNewDeck(deckName);
            if (index < 0) return;
            LoadMyDeckData();
        }
        /// <summary>
        /// 덱 Element 선택했을때
        /// </summary>
        /// <param name="icon"></param>
        protected override void OnSelectedIcon(UIIcon icon)
        {
            _windowTcgMyDeckCard?.UpdateCardInfo(icon.index);
        }

        public void AddCardToDeck(int deckIndex, int cardUid)
        {
            // GcLogger.Log($"AddCardToDeck {deckIndex}");
            var result = _myDeckData.AddCardToDeck(deckIndex, cardUid);
            if (!result) return;
        }
        
        public void RemoveDeck(int index)
        {
            bool result = _myDeckData.RemoveDeck(index);
            if (!result) return;
            // 순서 중요. Detach 를 하고, _defaultIconMyDeck 값이 있으면 null 처리
            DetachIcon(index);
            if (_defaultIconMyDeck && _defaultIconMyDeck.index == index)
            {
                result = _playerDataTcg.SetDefaultDeckIndex(-1);
                if (result)
                    _defaultIconMyDeck = null;
            }
            LoadMyDeckData();
        }

        public void SetDefaultDeck(int index)
        {
            if (index < 0) return;
            if (_defaultIconMyDeck && _defaultIconMyDeck.index == index) return;
            
            UIIcon uiIcon = GetIconByIndex(index);
            if (GcLogger.IsNull(uiIcon, nameof(uiIcon))) return;
            UIIconMyDeck uiIconMyDeck = uiIcon as UIIconMyDeck;
            if (GcLogger.IsNull(uiIconMyDeck, nameof(uiIconMyDeck))) return;
            if (_defaultIconMyDeck != null) _defaultIconMyDeck.SetDefault(false);
            uiIconMyDeck.SetDefault(true);
            _defaultIconMyDeck = uiIconMyDeck;
        }
    }
}