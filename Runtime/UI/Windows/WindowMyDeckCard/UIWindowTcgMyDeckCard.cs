using System;
using GGemCo2DCore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GGemCo2DTcg
{
    /// <summary>
    /// “나의 덱(My Deck)” 목록에서 덱을 선택했을 때,
    /// 해당 덱에 포함된 카드(영웅 포함) 목록을 표시하고 편집(추가/제거/기본 덱 지정)하는 UI 윈도우입니다.
    /// </summary>
    public class UIWindowTcgMyDeckCard : UIWindow
    {
        /// <summary>
        /// 덱 카드 목록에서 카드를 제거할 때 사용할 마우스 버튼 타입입니다.
        /// </summary>
        public enum RemoveCardButtonType
        {
            /// <summary>좌클릭으로 카드 제거.</summary>
            Left,

            /// <summary>우클릭으로 카드 제거.</summary>
            Right,
        }

        [Header(UIWindowConstants.TitleHeaderIndividual)]
        [Tooltip("덱 이름")]
        public TMP_Text textName;

        [Tooltip("덱에 포함된 카드 개수")]
        public TMP_Text textCardCount;

        [Tooltip("카드 제거에 사용할 마우스 버튼 타입")]
        public RemoveCardButtonType removeCardButtonType = RemoveCardButtonType.Left;

        [Tooltip("선택한 덱을 기본 덱으로 설정하는 버튼")]
        public Button buttonSetDefaultDeck;

        /// <summary>
        /// 현재 표시/편집 중인 덱 인덱스입니다.
        /// </summary>
        private int _deckIndex;

        /// <summary>
        /// 덱 저장 데이터 접근 모델입니다. Start에서 SaveManager를 통해 주입/캐싱됩니다.
        /// </summary>
        public MyDeckData myDeckData;

        /// <summary>팝업 표시를 담당하는 매니저입니다.</summary>
        private PopupManager _popupManager;

        /// <summary>덱 목록 윈도우(기본 덱 표시 갱신에 사용)입니다.</summary>
        private UIWindowTcgMyDeck _windowTcgMyDeck;

        /// <summary>카드 정보(툴팁/상세) 표시 윈도우입니다.</summary>
        private UIWindowTcgCardInfo _uiWindowTcgCardInfo;

        /// <summary>카드 테이블(UID로 카드 정보 조회)에 대한 참조입니다.</summary>
        private TableTcgCard _tableTcgCard;

        /// <summary>플레이어 저장 데이터(기본 덱 인덱스 저장)에 대한 참조입니다.</summary>
        private PlayerDataTcg _playerDataTcg;

        /// <summary>
        /// 윈도우 초기화 시 필수 참조를 검증하고,
        /// 아이콘 핸들러/드래그드롭 전략 등록 및 버튼 이벤트를 연결합니다.
        /// </summary>
        protected override void Awake()
        {
            if (TableLoaderManager.Instance == null)
            {
                return;
            }

            if (iconPrefab == null)
            {
                GcLogger.LogError($"{nameof(UIWindowTcgMyDeckCard)}: iconPrefab 가 지정되지 않았습니다.");
                return;
            }

            if (containerIcon == null)
            {
                GcLogger.LogError($"{nameof(UIWindowTcgMyDeckCard)}: containerIcon 이 null 입니다.");
                return;
            }

            _tableTcgCard = TableLoaderManagerTcg.Instance.TableTcgCard;
            if (_tableTcgCard == null)
            {
                GcLogger.LogError($"{nameof(UIWindowTcgMyDeckCard)}: TableTcgCard 가 초기화되지 않았습니다.");
                return;
            }

            uid = UIWindowConstants.WindowUid.TcgMyDeckCard;

            // 덱 카드 최대 수(슬롯 수)를 설정합니다.
            maxCountIcon = AddressableLoaderSettingsTcg.Instance.tcgSettings.maxDeckCardCount;

            base.Awake();

            IconPoolManager.SetSetIconHandler(new SetIconHandlerMyDeckCard());
            DragDropHandler.SetStrategy(new DragDropStrategyMyDeckCard());

            buttonSetDefaultDeck?.onClick.AddListener(OnClickSetDefaultDeck);
        }

        /// <summary>
        /// 오브젝트 파괴 시 버튼 이벤트를 해제합니다.
        /// </summary>
        protected void OnDestroy()
        {
            buttonSetDefaultDeck?.onClick.RemoveAllListeners();
        }

        /// <summary>
        /// 시작 시점에 저장 데이터 및 연동 윈도우 참조를 캐싱합니다.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            if (SceneGame == null)
            {
                GcLogger.LogWarning($"{nameof(UIWindowTcgMyDeckCard)}: SceneGame 이 null 입니다.");
                return;
            }

            var saveManagerTcg = TcgPackageManager.Instance?.saveDataManagerTcg;
            if (saveManagerTcg == null)
            {
                GcLogger.LogWarning($"{nameof(UIWindowTcgMyDeckCard)}: saveDataManagerTcg 가 null 입니다.");
            }
            else
            {
                myDeckData = saveManagerTcg.MyDeck;
                _playerDataTcg = saveManagerTcg.PlayerTcg;
            }

            _popupManager = SceneGame.popupManager;
            _windowTcgMyDeck = SceneGame.uIWindowManager.GetUIWindowByUid<UIWindowTcgMyDeck>(
                UIWindowConstants.WindowUid.TcgMyDeck);

            _uiWindowTcgCardInfo = SceneGame.uIWindowManager.GetUIWindowByUid<UIWindowTcgCardInfo>(
                UIWindowConstants.WindowUid.TcgCardInfo);
        }

        /// <summary>
        /// 덱 목록에서 선택된 덱 인덱스를 받아 현재 윈도우의 표시 대상을 갱신합니다.
        /// </summary>
        /// <param name="index">선택된 덱 인덱스.</param>
        public void UpdateCardInfo(int index)
        {
            _deckIndex = index;

            // NOTE: Active 된 상태에서 LoadMyDeckCardData(슬롯 구성)를 수행해야 하므로 Show를 먼저 호출합니다.
            Show(true);
        }

        /// <summary>
        /// 윈도우 표시/숨김 시 호출됩니다. 표시될 때 덱 카드 목록을 로드합니다.
        /// </summary>
        /// <param name="show">true면 표시, false면 숨김.</param>
        public override void OnShow(bool show)
        {
            if (SceneGame == null || TableLoaderManager.Instance == null) return;

            if (show)
            {
                LoadMyDeckCardData();
            }
        }

        /// <summary>
        /// 현재 덱 인덱스의 카드 목록(영웅 포함)을 읽어 슬롯/아이콘을 다시 구성합니다.
        /// </summary>
        private void LoadMyDeckCardData()
        {
            if (myDeckData == null)
            {
                GcLogger.LogWarning($"{nameof(UIWindowTcgMyDeckCard)}: MyDeckData 가 초기화되지 않았습니다.");
                return;
            }

            var deckSaveData = myDeckData.GetDeckInfoByIndex(_deckIndex);
            if (deckSaveData == null)
            {
                GcLogger.LogWarning($"{nameof(UIWindowTcgMyDeckCard)}: 인덱스 {_deckIndex} 에 해당하는 덱 정보를 찾을 수 없습니다.");
                return;
            }

            SetDeckName(deckSaveData);
            SetDeckCardCount(deckSaveData);

            // 모든 슬롯을 초기 상태(비활성)로 리셋
            for (int i = 0; i < maxCountIcon; i++)
            {
                var slot = GetSlotByIndex(i);
                if (slot == null) continue;
                slot.gameObject.SetActive(false);
            }

            // 0번 슬롯은 영웅 카드
            SetHeroCard(deckSaveData);

            // 1번부터 일반 카드 목록을 채움
            int index = 1;
            foreach (var cardCounts in deckSaveData.cardList)
            {
                var cardUid = cardCounts.Key;
                var count = cardCounts.Value;

                // 카드 UID가 유효한지(테이블 존재) 확인
                var info = _tableTcgCard.GetDataByUid(cardUid);
                if (info == null)
                {
                    GcLogger.LogWarning($"{nameof(UIWindowTcgMyDeckCard)}: cardUid {cardUid} 에 대한 카드 정보를 찾을 수 없습니다.");
                    continue;
                }

                var slot = GetSlotByIndex(index);
                slot?.gameObject.SetActive(true);

                var icon = GetIconByIndex(index);
                if (!icon) continue;

                UIIcon uiIcon = icon.GetComponent<UIIcon>();
                if (!uiIcon) continue;

                // 덱 카드 아이콘에는 현재 덱 인덱스를 주입하여, 제거/드롭 등의 동작에 사용합니다.
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

        /// <summary>
        /// 덱의 영웅 카드(0번 슬롯)를 표시합니다.
        /// </summary>
        /// <param name="deckSaveData">대상 덱 저장 데이터.</param>
        private void SetHeroCard(MyDeckSaveData deckSaveData)
        {
            if (deckSaveData == null || deckSaveData.heroCardUid <= 0) return;

            int cardUid = deckSaveData.heroCardUid;
            int count = 1;
            int index = 0;

            var slot = GetSlotByIndex(index);
            slot?.gameObject.SetActive(true);

            var icon = GetIconByIndex(index);
            if (!icon) return;

            UIIcon uiIcon = icon.GetComponent<UIIcon>();
            if (!uiIcon) return;

            UIIconMyDeckCard uiIconMyDeckCard = uiIcon as UIIconMyDeckCard;
            if (uiIconMyDeckCard)
            {
                uiIconMyDeckCard.SetDeckIndex(_deckIndex);
            }

            uiIcon.Initialize(this, uid, index, index, iconSize, slotSize);
            uiIcon.ChangeInfoByUid(cardUid, count, 1);
        }

        /// <summary>
        /// 덱 이름 텍스트를 갱신합니다.
        /// </summary>
        /// <param name="deckSaveData">대상 덱 저장 데이터.</param>
        private void SetDeckName(MyDeckSaveData deckSaveData)
        {
            if (!textName)
            {
                GcLogger.LogWarning($"{nameof(UIWindowTcgMyDeckCard)}: textName 이 할당되지 않았습니다.");
                return;
            }

            textName.text = deckSaveData?.deckName ?? string.Empty;
        }

        /// <summary>
        /// 덱 카드 개수 텍스트를 갱신합니다.
        /// </summary>
        /// <param name="deckSaveData">대상 덱 저장 데이터.</param>
        private void SetDeckCardCount(MyDeckSaveData deckSaveData)
        {
            if (!textCardCount)
            {
                return;
            }

            // NOTE: cardList.Count는 "종류 수"일 수 있습니다. (같은 카드의 수량 합계가 아닌 점에 유의)
            var count = deckSaveData?.cardList?.Count ?? 0;
            textCardCount.text = $"{count}/{maxCountIcon}";
        }

        /// <summary>
        /// 드롭된 카드 UID를 현재 덱에 추가한 뒤, 목록을 다시 로드합니다.
        /// 영웅 타입 카드면 영웅 슬롯으로 추가합니다.
        /// </summary>
        /// <param name="dropIconUid">추가할 카드 UID.</param>
        public void AddCardToDeck(int dropIconUid)
        {
            if (myDeckData == null)
            {
                GcLogger.LogWarning($"{nameof(UIWindowTcgMyDeckCard)}: MyDeckData 가 초기화되지 않아 카드를 추가할 수 없습니다.");
                return;
            }

            var info = _tableTcgCard.GetDataByUid(dropIconUid);

            bool result;
            if (info != null && info.type == CardConstants.Type.Hero)
            {
                result = myDeckData.AddHeroCardToDeck(_deckIndex, dropIconUid);
            }
            else
            {
                result = myDeckData.AddCardToDeck(_deckIndex, dropIconUid);
            }

            if (!result) return;
            LoadMyDeckCardData();
        }

        /// <summary>
        /// 카드 아이콘의 상세 정보(툴팁)를 표시/숨김 처리합니다.
        /// </summary>
        /// <param name="show">true면 표시, false면 숨김.</param>
        /// <param name="icon">표시할 카드 아이콘(표시 시 필수).</param>
        public override void ShowItemInfo(bool show, UIIcon icon = null)
        {
            if (show)
            {
                if (icon == null) return;
                _uiWindowTcgCardInfo.SetCardUid(icon.uid, icon.gameObject, UIWindowTcgCardInfo.PositionType.Right, slotSize);
            }
            else
            {
                _uiWindowTcgCardInfo.Show(false);
            }
        }

        /// <summary>
        /// 카드 아이콘 좌클릭 처리입니다.
        /// 설정된 <see cref="removeCardButtonType"/>이 Left일 때 선택된 카드를 덱에서 제거합니다.
        /// </summary>
        /// <param name="index">클릭된 아이콘 인덱스.</param>
        public override void SetSelectedIcon(int index)
        {
            if (removeCardButtonType != RemoveCardButtonType.Left) return;

            var icon = GetIconByIndex(index);
            RemoveCardToDeck(icon);
        }

        /// <summary>
        /// 카드 아이콘 우클릭 처리입니다.
        /// 설정된 <see cref="removeCardButtonType"/>이 Right일 때 클릭된 카드를 덱에서 제거합니다.
        /// </summary>
        /// <param name="icon">클릭된 아이콘.</param>
        public override void OnRightClick(UIIcon icon)
        {
            if (removeCardButtonType != RemoveCardButtonType.Right) return;

            RemoveCardToDeck(icon);
        }

        /// <summary>
        /// 지정한 카드 아이콘(UID)을 현재 덱에서 1장 제거하고, UI 카운트/슬롯 상태를 갱신합니다.
        /// </summary>
        /// <param name="icon">제거 대상 카드 아이콘.</param>
        public void RemoveCardToDeck(UIIcon icon)
        {
            int result = myDeckData.RemoveCardToDeck(_deckIndex, icon.uid);
            if (result == -1) return;

            // 개수가 0이면 덱 목록에서 제거(아이콘 분리)
            if (result == 0)
            {
                DetachIcon(icon.index);
            }

            icon.SetCount(result);
        }

        /// <summary>
        /// 현재 선택된 덱을 기본 덱으로 저장하고, 덱 목록 UI 및 완료 팝업을 갱신합니다.
        /// </summary>
        private void OnClickSetDefaultDeck()
        {
            // 영웅 추가 여부 체크(기본 덱으로 저장하기 위한 최소 조건)
            int heroCardUid = myDeckData.GetHeroCardUidByDeckIndex(_deckIndex);
            if (heroCardUid <= 0)
            {
                SceneGame.systemMessageManager.ShowMessageError("System_Tcg_AddHeroCard");
                return;
            }

            bool result = _playerDataTcg.SetDefaultDeckIndex(_deckIndex);
            if (!result) return;

            if (_windowTcgMyDeck)
                _windowTcgMyDeck.SetDefaultDeck(_deckIndex);

            PopupMetadata popupMetadata = new PopupMetadata
            {
                PopupType = PopupManager.Type.Default,
                Title = "System_Tcg_Save",
                Message = "System_Tcg_SaveCompleted",
            };
            _popupManager.ShowPopup(popupMetadata);
        }
    }
}
