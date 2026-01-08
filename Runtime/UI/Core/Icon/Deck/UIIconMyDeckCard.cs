using System;
using GGemCo2DCore;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GGemCo2DTcg
{
    /// <summary>
    /// "내 덱(My Deck)" 카드 목록에서 카드 한 장(또는 수량)을 표시하는 UI 아이콘 컴포넌트입니다.
    /// - 카드 이름/수량 표시
    /// - 호버 시 아이템(카드) 정보 표시
    /// - 좌클릭 선택, 우클릭 컨텍스트 동작 처리
    /// - 비활성화 시 선택/오버/정보 창 상태를 정리합니다.
    /// </summary>
    public class UIIconMyDeckCard : UIIcon, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header(UIWindowConstants.TitleHeaderIndividual)]
        [Tooltip("카드 이름")]
        public TextMeshProUGUI textName;

        /// <summary>
        /// 이 아이콘이 속한 덱의 인덱스입니다.
        /// (현재 클래스에서는 저장만 하고 직접 사용하지 않으며, 외부에서 참조/확장을 위해 보관합니다.)
        /// </summary>
        private int _deckIndex;

        private UIWindowTcgMyDeckCard _windowTcgMyDeckCard;
        private StruckTableTcgCard _struckTableTcgCard;
        private MyDeckData _myDeckData;
        private UIWindowTcgCardInfo _windowTcgCardInfo;

        /// <summary>
        /// Unity 생명주기: 첫 프레임 시작 시 호출됩니다.
        /// 카드 상세/툴팁 표시 용도의 카드 정보 윈도우를 캐싱합니다.
        /// </summary>
        protected override void Start()
        {
            base.Start();
            _windowTcgCardInfo = SceneGame.Instance.uIWindowManager
                .GetUIWindowByUid<UIWindowTcgCardInfo>(UIWindowConstants.WindowUid.TcgCardInfo);
        }

        /// <summary>
        /// Unity 생명주기: 오브젝트가 비활성화될 때 호출됩니다.
        /// 선택/오버 UI 상태 및 카드 정보 창을 정리하여, 다른 UI로 전환 시 잔상이 남지 않도록 합니다.
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
        /// 카드 UID를 기준으로 아이콘 표시 정보를 갱신합니다.
        /// - 테이블(<see cref="TableLoaderManagerTcg"/>)에서 카드 메타를 조회하여 이름/타입 표시를 설정합니다.
        /// - 덱 저장 데이터(<see cref="MyDeckData"/>) 및 현재 윈도우 참조를 캐싱합니다.
        /// </summary>
        /// <param name="cardUid">표시할 카드의 고유 UID입니다.</param>
        /// <param name="iconCount">덱에 포함된 수량(표시용)입니다.</param>
        /// <param name="iconLevel">레벨(기본 구현과의 호환용)입니다.</param>
        /// <param name="iconIsLearn">학습/해금 여부(기본 구현과의 호환용)입니다.</param>
        /// <param name="remainCoolTime">남은 쿨타임(기본 구현과의 호환용)입니다.</param>
        /// <returns>정보 갱신 성공 시 true를 반환합니다.</returns>
        public override bool ChangeInfoByUid(
            int cardUid,
            int iconCount = 0,
            int iconLevel = 0,
            bool iconIsLearn = false,
            int remainCoolTime = 0)
        {
            uid = cardUid;
            SetCount(iconCount);

            // 카드 메타 데이터 조회 및 캐싱
            _struckTableTcgCard = TableLoaderManagerTcg.Instance.TableTcgCard.GetDataByUid(cardUid);

            // 현재 윈도우/저장 데이터 캐싱
            _windowTcgMyDeckCard = window as UIWindowTcgMyDeckCard;
            _myDeckData = TcgPackageManager.Instance.saveDataManagerTcg.MyDeck;

            // 표시 텍스트 갱신
            if (textName != null)
            {
                textName.text = _struckTableTcgCard.name;
            }

            // Hero 카드는 강조 표시(색상 변경)
            ImageIcon.color = _struckTableTcgCard.type == CardConstants.Type.Hero ? Color.yellow : Color.white;

            UpdateInfo();
            return true;
        }

        /// <summary>
        /// 아이콘 표시 정보를 초기화합니다.
        /// 기본 초기화 후 카드 이름 텍스트를 비웁니다.
        /// </summary>
        public override void ClearIconInfos()
        {
            base.ClearIconInfos();
            if (textName != null)
            {
                textName.text = "";
            }
        }

        /// <summary>
        /// 덱 내 카드 수량을 설정하고, 표시 포맷(<c>xN</c>)으로 갱신합니다.
        /// </summary>
        /// <param name="value">표시할 카드 수량입니다.</param>
        public override void SetCount(int value)
        {
            base.SetCount(value);
            if (textCount == null) return;

            textCount.text = $"x{value}";
        }

        /// <summary>
        /// 마우스 포인터가 아이콘 위로 올라오면 카드 정보 표시와 오버(하이라이트) 표시를 켭니다.
        /// </summary>
        /// <param name="eventData">포인터 이벤트 데이터입니다.</param>
        public void OnPointerEnter(PointerEventData eventData)
        {
            window.ShowItemInfo(true, this);
            ShowOverImage(true);
        }

        /// <summary>
        /// 마우스 클릭 입력을 처리합니다.
        /// - 좌클릭: 해당 아이콘을 선택 상태로 설정합니다.
        /// - 우클릭: 컨텍스트 동작(예: 덱에서 제거/관리)을 호출합니다.
        /// </summary>
        /// <param name="eventData">포인터 이벤트 데이터입니다.</param>
        public void OnPointerClick(PointerEventData eventData)
        {
            // GcLogger.Log($"UIIconMyDeckCard OnPointerClick");
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
                // 수량이 없거나 UID가 유효하지 않으면 컨텍스트 동작을 수행하지 않습니다.
                if (uid <= 0 || GetCount() <= 0) return;
                window.OnRightClick(this);
            }
        }

        /// <summary>
        /// 마우스 포인터가 아이콘을 벗어나면 카드 정보 표시와 오버(하이라이트) 표시를 끕니다.
        /// </summary>
        /// <param name="eventData">포인터 이벤트 데이터입니다.</param>
        public void OnPointerExit(PointerEventData eventData)
        {
            // GcLogger.Log("OnPointerExit "+eventData);
            window.ShowItemInfo(false);
            ShowOverImage(false);
        }

        /// <summary>
        /// 이 아이콘이 속한 덱의 인덱스를 설정합니다.
        /// </summary>
        /// <param name="deckIndex">덱 인덱스입니다.</param>
        public void SetDeckIndex(int deckIndex)
        {
            _deckIndex = deckIndex;
        }
    }
}
