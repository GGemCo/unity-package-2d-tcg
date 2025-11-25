using GGemCo2DCore;
using TMPro;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 나의 덱 리스트 중에서 덱을 선택했을 때, 해당 덱에 포함된 카드 리스트를 표시하는 UI 윈도우
    /// </summary>
    public class UIWindowMyDeckCard : UIWindow
    {
        // 나의 덱 리스트에서 카드를 빼는 키를 마우스 좌클릭으로 할지, 우클릭으로 할지
        public enum RemoveCardButtonType
        {
            Left,
            Right,
        }
        [Header(UIWindowConstants.TitleHeaderIndividual)]
        [Tooltip("덱 이름")]
        public TMP_Text textName;

        [Tooltip("덱에 포함된 카드 개수")]
        public TMP_Text textCardCount;
        
        [Tooltip("RemoveCardButtonType")]
        public RemoveCardButtonType removeCardButtonType = RemoveCardButtonType.Left;

        private int _deckIndex;
        
        public MyDeckData myDeckData;
        
        private PopupManager _popupManager;
        private UIWindowMyDeck _windowMyDeck;
        private UIWindowCardInfo _uiWindowCardInfo;
        private TableTcgCard _tableTcgCard;
        
        protected override void Awake()
        {
            if (TableLoaderManager.Instance == null)
            {
                return;
            }
            if (iconPrefab == null)
            {
                GcLogger.LogError($"{nameof(UIWindowMyDeckCard)}: iconPrefab 가 지정되지 않았습니다.");
                return;
            }
            if (containerIcon == null)
            {
                GcLogger.LogError($"{nameof(UIWindowMyDeckCard)}: containerIcon 이 null 입니다.");
                return;
            }
            _tableTcgCard = TableLoaderManagerTcg.Instance.TableTcgCard;

            if (_tableTcgCard == null)
            {
                GcLogger.LogError($"{nameof(UIWindowMyDeckCard)}: TableTcgCard 가 초기화되지 않았습니다.");
                return;
            }
            
            uid = UIWindowConstants.WindowUid.TcgMyDeckCard;
            maxCountIcon = AddressableLoaderSettingsTcg.Instance.tcgSettings.maxDeckCardCount;

            base.Awake();

            IconPoolManager.SetSetIconHandler(new SetIconHandlerMyDeckCard());
            DragDropHandler.SetStrategy(new DragDropStrategyMyDeckCard());
        }

        protected override void Start()
        {
            base.Start();

            if (SceneGame == null)
            {
                GcLogger.LogWarning($"{nameof(UIWindowMyDeckCard)}: SceneGame 이 null 입니다.");
                return;
            }

            var saveManagerTcg = TcgPackageManager.Instance?.saveDataManagerTcg;
            if (saveManagerTcg == null)
            {
                GcLogger.LogWarning($"{nameof(UIWindowMyDeckCard)}: saveDataManagerTcg 가 null 입니다.");
            }
            else
            {
                myDeckData = saveManagerTcg.MyDeck;
            }

            _popupManager = SceneGame.popupManager;
            _windowMyDeck = SceneGame.uIWindowManager.GetUIWindowByUid<UIWindowMyDeck>(UIWindowConstants.WindowUid.TcgMyDeck);
            _uiWindowCardInfo = SceneGame.uIWindowManager.GetUIWindowByUid<UIWindowCardInfo>(UIWindowConstants.WindowUid.TcgCardInfo);
        }

        public void UpdateCardInfo(int index)
        {
            _deckIndex = index;
            // 순서 중요. Active 된 상태에서 BuildCardElements 함수 처리를 해야 함 
            Show(true);
        }
        public override void OnShow(bool show)
        {
            if (SceneGame == null || TableLoaderManager.Instance == null) return;
            if (show)
            {
                LoadMyDeckCardData();
            }
        }

        private void LoadMyDeckCardData()
        {
            if (myDeckData == null)
            {
                GcLogger.LogWarning($"{nameof(UIWindowMyDeckCard)}: MyDeckData 가 초기화되지 않았습니다.");
                return;
            }

            var deckSaveData = myDeckData.GetDeckInfoByIndex(_deckIndex);
            if (deckSaveData == null)
            {
                GcLogger.LogWarning($"{nameof(UIWindowMyDeckCard)}: 인덱스 {_deckIndex} 에 해당하는 덱 정보를 찾을 수 없습니다.");
                return;
            }
            SetDeckName(deckSaveData);
            SetDeckCardCount(deckSaveData);

            for (int i = 0; i < maxCountIcon; i++)
            {
                var slot = GetSlotByIndex(i);
                if (slot == null) continue;
                slot.gameObject.SetActive(false);
            }
            int index = 0;
            foreach (var cardCounts in deckSaveData.cardList)
            {
                var cardUid = cardCounts.Key;
                var count = cardCounts.Value;
                
                var info = _tableTcgCard.GetDataByUid(cardUid);
                if (info == null)
                {
                    GcLogger.LogWarning($"{nameof(UIWindowMyDeckCard)}: cardUid {cardUid} 에 대한 카드 정보를 찾을 수 없습니다.");
                    continue;
                }

                var slot = GetSlotByIndex(index);
                slot?.gameObject.SetActive(true);

                var icon = GetIconByIndex(index);
                if (!icon) continue;
                UIIcon uiIcon = icon.GetComponent<UIIcon>();
                if (!uiIcon) continue;
                UIIconMyDeckCard uiIconMyDeckCard = uiIcon as UIIconMyDeckCard;
                if (uiIconMyDeckCard)
                {
                    uiIconMyDeckCard.SetDeckIndex(_deckIndex);
                }
                uiIcon.Initialize(this, uid, index, index, iconSize, slotSize);
                uiIcon.ChangeInfoByUid(cardUid, count, 1);
                index++;
            }
        }

        private void SetDeckName(MyDeckSaveData deckSaveData)
        {
            if (!textName)
            {
                GcLogger.LogWarning($"{nameof(UIWindowMyDeckCard)}: textName 이 할당되지 않았습니다.");
                return;
            }

            textName.text = deckSaveData?.deckName ?? string.Empty;
        }

        private void SetDeckCardCount(MyDeckSaveData deckSaveData)
        {
            if (!textCardCount)
            {
                return;
            }

            var count = deckSaveData?.cardList?.Count ?? 0;
            textCardCount.text = $"{count}/{maxCountIcon}";
        }

        public void AddCardToDeck(int dropIconUid)
        {
            if (myDeckData == null)
            {
                GcLogger.LogWarning($"{nameof(UIWindowMyDeckCard)}: MyDeckData 가 초기화되지 않아 카드를 추가할 수 없습니다.");
                return;
            }

            var result = myDeckData.AddCardToDeck(_deckIndex, dropIconUid);
            if (!result) return;
            LoadMyDeckCardData();
        }
        /// <summary>
        /// 아이템 정보 보기
        /// </summary>
        /// <param name="show"></param>
        /// <param name="icon"></param>
        public override void ShowItemInfo(bool show, UIIcon icon = null)
        {
            if (show)
            {
                if (icon == null) return;
                _uiWindowCardInfo.SetCardUid(icon.uid, icon.gameObject, UIWindowCardInfo.PositionType.Right, slotSize);
            }
            else
            {
                _uiWindowCardInfo.Show(false);
            }
        }
        
        /// <summary>
        /// 카드 아이콘 좌클릭 처리
        /// </summary>
        /// <param name="index"></param>
        public override void SetSelectedIcon(int index)
        {
            if (removeCardButtonType != RemoveCardButtonType.Left) return;
            var icon = GetIconByIndex(index);
            RemoveCardToDeck(icon);
        }
        /// <summary>
        /// 카드 아이콘 우클릭 처리
        /// </summary>
        /// <param name="icon"></param>
        public override void OnRightClick(UIIcon icon)
        {
            if (removeCardButtonType != RemoveCardButtonType.Right) return;
            RemoveCardToDeck(icon);
        }

        public void RemoveCardToDeck(UIIcon icon)
        {
            int result = myDeckData.RemoveCardToDeck(_deckIndex, icon.uid);
            if (result == -1) return;
            // 개수가 0 이면 덱에서 지우기
            if (result == 0)
            {
                DetachIcon(icon.index);
            }
            icon.SetCount(result);
        }
    }
}