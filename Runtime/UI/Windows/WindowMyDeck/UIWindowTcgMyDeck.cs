using System.Collections.Generic;
using GGemCo2DCore;
using UnityEngine;
using Button = UnityEngine.UI.Button;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 플레이어가 보유한 “나의 덱(My Deck)” 목록을 표시/관리하는 윈도우입니다.
    /// <para>- 저장된 덱 리스트 로드 및 슬롯/아이콘 바인딩</para>
    /// <para>- 새 덱 생성(입력창 호출) 및 덱 삭제</para>
    /// <para>- 덱 선택 시 상세(카드 구성) 창 갱신</para>
    /// <para>- 기본 덱(Default Deck) 표시/설정 상태 동기화</para>
    /// </summary>
    public class UIWindowTcgMyDeck : UIWindow
    {
        [Header(UIWindowConstants.TitleHeaderIndividual)]
        [Tooltip("새로운 덱 만들기 버튼")]
        public Button buttonCreateNew;

        /// <summary>저장된 덱 데이터(목록/구성)를 관리하는 모델입니다.</summary>
        private MyDeckData _myDeckData;

        /// <summary>플레이어 TCG 관련 저장 데이터(기본 덱 인덱스 등)입니다.</summary>
        private PlayerDataTcg _playerDataTcg;

        /// <summary>덱 이름 입력을 위한 공용 입력창 윈도우입니다.</summary>
        private UIWindowInputField _windowInputField;

        /// <summary>선택한 덱의 카드 구성을 보여주는 상세 윈도우입니다.</summary>
        private UIWindowTcgMyDeckCard _windowTcgMyDeckCard;

        /// <summary>덱 생성 입력창에 사용할 제목/라벨 문자열(로컬라이즈).</summary>
        private string _titleInputField;

        /// <summary>현재 기본 덱으로 표시된 덱 아이콘 캐시입니다.</summary>
        private UIIconMyDeck _defaultIconMyDeck;

        /// <summary>
        /// 윈도우 UID 및 아이콘/드래그드롭 처리기를 등록하고, 버튼 이벤트를 연결합니다.
        /// </summary>
        protected override void Awake()
        {
            uid = UIWindowConstants.WindowUid.TcgMyDeck;
            if (TableLoaderManager.Instance == null) return;

            base.Awake();

            IconPoolManager.SetSetIconHandler(new SetIconHandlerMyDeck());
            DragDropHandler.SetStrategy(new DragDropStrategyMyDeck());

            buttonCreateNew?.onClick.AddListener(OnClickCreateNew);
        }

        /// <summary>
        /// 오브젝트 파괴 시 버튼 이벤트를 해제합니다.
        /// </summary>
        private void OnDestroy()
        {
            buttonCreateNew?.onClick.RemoveAllListeners();
        }

        /// <summary>
        /// 시작 시점에 저장 데이터 및 관련 윈도우 참조를 캐싱하고, 로컬라이즈 문자열을 준비합니다.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            if (SceneGame != null && TcgPackageManager.Instance.saveDataManagerTcg != null)
            {
                _myDeckData = TcgPackageManager.Instance.saveDataManagerTcg.MyDeck;
                _playerDataTcg = TcgPackageManager.Instance.saveDataManagerTcg.PlayerTcg;
            }

            _windowInputField = SceneGame.uIWindowManager.GetUIWindowByUid<UIWindowInputField>(
                UIWindowConstants.WindowUid.InputField);

            _windowTcgMyDeckCard = SceneGame.uIWindowManager.GetUIWindowByUid<UIWindowTcgMyDeckCard>(
                UIWindowConstants.WindowUid.TcgMyDeckCard);

            _titleInputField = LocalizationManagerTcg.Instance.GetUIWindowMyDeckByKey("Button_Create_New");
        }

        /// <summary>
        /// 윈도우 표시/숨김 시 호출됩니다.
        /// 표시될 때는 덱 데이터를 로드하고, 선택 상태를 초기화합니다.
        /// </summary>
        /// <param name="show">true면 표시, false면 숨김.</param>
        public override void OnShow(bool show)
        {
            if (SceneGame == null || TableLoaderManager.Instance == null) return;

            if (show)
            {
                LoadMyDeckData();
            }

            // 표시/숨김과 관계없이 선택된 아이콘 상태를 초기화합니다.
            RemoveSelectedIcon();
        }

        /// <summary>
        /// 저장된 덱 목록을 읽어 슬롯/아이콘을 갱신하고, 기본 덱 표시 상태를 동기화합니다.
        /// </summary>
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

                // 덱 아이콘을 현재 윈도우/UID/인덱스 기준으로 초기화 후 정보 갱신
                uiIcon.Initialize(this, uid, data.index, data.index, iconSize, slotSize);
                uiIcon.ChangeInfoByUid(data.index, 1, 1);

                // 덱 목록에서는 드래그를 사용하지 않습니다.
                uiIcon.SetDrag(false);
            }

            if (_playerDataTcg != null)
                SetDefaultDeck(_playerDataTcg.defaultDeckIndex);
        }

        /// <summary>
        /// “새 덱 만들기” 버튼 클릭 시 호출됩니다.
        /// 최대 덱 개수를 초과하면 경고 메시지를 표시하고, 아니면 입력창을 엽니다.
        /// </summary>
        private void OnClickCreateNew()
        {
            if (_myDeckData.GetCurrentCount() >= maxCountIcon)
            {
                SceneGame.systemMessageManager.ShowMessageWarning("Tcg_System_MaxDeckCount", maxCountIcon);
                return;
            }

            _windowInputField.UpdateInfo(_titleInputField, OnCreateNew);
        }

        /// <summary>
        /// 입력창에서 덱 이름 입력이 완료되었을 때 호출되어 새 덱을 생성합니다.
        /// </summary>
        /// <param name="deckName">생성할 덱 이름.</param>
        private void OnCreateNew(string deckName)
        {
            var index = _myDeckData.AddNewDeck(deckName);
            if (index < 0) return;

            LoadMyDeckData();
        }

        /// <summary>
        /// 덱 아이콘(엘리먼트)을 선택했을 때 호출됩니다.
        /// 선택된 덱의 카드 구성 정보를 상세 윈도우에 반영합니다.
        /// </summary>
        /// <param name="icon">선택된 덱 아이콘.</param>
        protected override void OnSelectedIcon(UIIcon icon)
        {
            _windowTcgMyDeckCard?.UpdateCardInfo(icon.index);
        }

        /// <summary>
        /// 지정한 덱에 카드를 추가합니다.
        /// </summary>
        /// <param name="deckIndex">대상 덱 인덱스.</param>
        /// <param name="cardUid">추가할 카드 UID.</param>
        public void AddCardToDeck(int deckIndex, int cardUid)
        {
            var result = _myDeckData.AddCardToDeck(deckIndex, cardUid);
            if (!result) return;
        }

        /// <summary>
        /// 지정한 인덱스의 덱을 삭제하고 UI/기본 덱 상태를 정리한 뒤 목록을 다시 로드합니다.
        /// </summary>
        /// <param name="index">삭제할 덱 인덱스.</param>
        public void RemoveDeck(int index)
        {
            bool result = _myDeckData.RemoveDeck(index);
            if (!result) return;

            // 순서 중요: 아이콘을 분리(detach)한 뒤, 기본 덱이 삭제된 경우 기본 덱 인덱스를 해제합니다.
            DetachIcon(index);

            if (_defaultIconMyDeck && _defaultIconMyDeck.index == index)
            {
                result = _playerDataTcg.SetDefaultDeckIndex(-1);
                if (result)
                    _defaultIconMyDeck = null;
            }

            LoadMyDeckData();
        }

        /// <summary>
        /// 지정한 덱을 기본 덱으로 UI에 표시합니다.
        /// 이미 기본 덱으로 표시되어 있으면 아무 작업도 하지 않습니다.
        /// </summary>
        /// <param name="index">기본 덱으로 표시할 덱 인덱스.</param>
        public void SetDefaultDeck(int index)
        {
            if (index < 0) return;
            if (_defaultIconMyDeck && _defaultIconMyDeck.index == index) return;

            UIIcon uiIcon = GetIconByIndex(index);
            if (GcLogger.IsNull(uiIcon, nameof(uiIcon))) return;

            UIIconMyDeck uiIconMyDeck = uiIcon as UIIconMyDeck;
            if (GcLogger.IsNull(uiIconMyDeck, nameof(uiIconMyDeck))) return;

            // 기존 기본 덱 표시 해제 후, 새 기본 덱 표시 적용
            if (_defaultIconMyDeck != null) _defaultIconMyDeck.SetDefault(false);
            uiIconMyDeck.SetDefault(true);

            _defaultIconMyDeck = uiIconMyDeck;
        }
    }
}
