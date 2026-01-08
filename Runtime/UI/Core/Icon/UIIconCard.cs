using System.Collections;
using GGemCo2DCore;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GGemCo2DTcg
{
    /// <summary>
    /// TCG 카드 UI 아이콘을 표현합니다.
    /// 테이블 데이터(StruckTableTcgCard)를 바인딩하고, 아트/보더 스프라이트를 로드하여 표시합니다.
    /// 덱 편집 창이 열려 있을 때 클릭 시 덱에 카드를 추가합니다.
    /// </summary>
    public class UIIconCard : UIIcon, IPointerClickHandler
    {
        [Header(UIWindowConstants.TitleHeaderIndividual)]
        [Tooltip("카드 이름 텍스트")]
        public TextMeshProUGUI textName;

        [Tooltip("자원 비용(마나 등) 텍스트")]
        public TextMeshProUGUI textCost;

        [Tooltip("카드 설명 텍스트")]
        public TextMeshProUGUI textDescription;

        [Tooltip("공격력 텍스트")]
        public TextMeshProUGUI textAttack;

        [Tooltip("체력 텍스트")]
        public TextMeshProUGUI textHealth;

        [Tooltip("카드 아트워크 이미지")]
        public Image imageArtWork;

        [Tooltip("카드 테두리(등급 등) 이미지")]
        public Image imageBorder;

        [Tooltip("공격력 배경/아이콘 이미지")]
        public Image imageAttack;

        [Tooltip("체력 배경/아이콘 이미지")]
        public Image imageHealth;

        [Tooltip("마나(비용) 배경/아이콘 이미지")]
        public Image imageMana;

        /// <summary>
        /// 카드 테이블 접근자입니다.
        /// </summary>
        protected TableTcgCard tableTcgCard;

        /// <summary>
        /// 현재 바인딩된 카드 데이터입니다.
        /// </summary>
        protected StruckTableTcgCard CurrentCard => _cardData;

        /// <summary>
        /// 보더 스프라이트 키 생성에 사용되는 접두사입니다.
        /// (예: Hand/Field 등 표시 맥락에 따라 파생 클래스에서 변경)
        /// </summary>
        protected string borderKeyPrefix;

        /// <summary>
        /// 현재 카드 테이블 데이터 캐시입니다.
        /// </summary>
        private StruckTableTcgCard _cardData;

        /// <summary>
        /// 덱 편집/내 덱 UI 윈도우 참조입니다. 열려 있을 때 클릭 입력으로 카드 추가를 수행합니다.
        /// </summary>
        private UIWindowTcgMyDeckCard _windowTcgMyDeckCard;

        /// <summary>
        /// 아트워크 스프라이트 키 접두사입니다.
        /// </summary>
        private static readonly string ArtKeyPrefix = $"{ConfigAddressableKeyTcg.Card.ImageArt}_";

        /// <summary>현재 카드가 주문(Spell) 타입인지 여부입니다.</summary>
        public bool IsSpell => _cardData is { type: CardConstants.Type.Spell };

        /// <summary>현재 카드가 장비(Equipment) 타입인지 여부입니다.</summary>
        public bool IsEquipment => _cardData is { type: CardConstants.Type.Equipment };

        /// <summary>현재 카드가 영구(Permanent) 타입인지 여부입니다.</summary>
        public bool IsPermanent => _cardData is { type: CardConstants.Type.Permanent };

        /// <summary>현재 카드가 영웅(Hero) 타입인지 여부입니다.</summary>
        public bool IsHero => _cardData is { type: CardConstants.Type.Hero };

        /// <summary>현재 카드가 크리처(Creature) 타입인지 여부입니다.</summary>
        public bool IsCreature => _cardData is { type: CardConstants.Type.Creature };

        /// <summary>
        /// 컴포넌트 초기화 시 호출됩니다.
        /// 기본 보더 키 접두사(Hand), 아이콘 타입을 설정하고 테이블 로더를 통해 카드 테이블을 캐싱합니다.
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            borderKeyPrefix = $"{ConfigAddressableKeyTcg.Card.ImageBorderHand}_";
            IconType = IconConstants.Type.TcgCard;
            _cardData = null;

            // TableLoader가 준비되지 않았을 수 있으므로 안전하게 접근
            if (TableLoaderManager.Instance != null && TableLoaderManagerTcg.Instance != null)
            {
                tableTcgCard = TableLoaderManagerTcg.Instance.TableTcgCard;
            }
        }

        /// <summary>
        /// 첫 프레임 시작 시 호출됩니다.
        /// 덱 편집 창(UIWindowTcgMyDeckCard) 참조를 캐싱합니다.
        /// </summary>
        protected override void Start()
        {
            base.Start();
            TryCacheDeckWindow();
        }

        /// <summary>
        /// 카드 UID를 기준으로 아이콘에 표시되는 카드 정보를 변경합니다.
        /// 테이블 데이터를 조회해 내부 캐시를 갱신하고, UI 텍스트/스프라이트를 적용합니다.
        /// </summary>
        /// <param name="cardUid">변경할 카드의 고유 UID</param>
        /// <param name="iconCount">아이콘에 표시할 카드 수량</param>
        /// <param name="iconLevel">카드 레벨(현재 구현에서는 UI 반영 없음)</param>
        /// <param name="iconIsLearn">카드 학습 여부(현재 구현에서는 UI 반영 없음)</param>
        /// <param name="remainCoolTime">남은 쿨타임(현재 구현에서는 UI 반영 없음)</param>
        /// <returns>성공 시 true, 테이블 누락/조회 실패 시 false를 반환합니다.</returns>
        public override bool ChangeInfoByUid(
            int cardUid,
            int iconCount = 0,
            int iconLevel = 0,
            bool iconIsLearn = false,
            int remainCoolTime = 0)
        {
            if (tableTcgCard == null)
            {
                // 테이블 로더가 아직 초기화되지 않았거나 누락된 경우
                GcLogger.LogError($"{nameof(UIIconCard)}: {nameof(tableTcgCard)} 가 null 입니다. 카드 정보 바인딩 실패. uid: {cardUid}");
                return false;
            }

            var info = tableTcgCard.GetDataByUid(cardUid);
            if (info == null)
            {
                GcLogger.LogError($"tcg_card 테이블에 없는 카드 입니다. uid: {cardUid}");
                return false;
            }

            uid = cardUid;
            _cardData = info;

            SetCount(iconCount);
            ApplyCardView(info);

            // 상위(UIIcon)의 공통 갱신 루틴이 있다면 유지
            UpdateInfo();
            return true;
        }

        /// <summary>
        /// 카드 데이터 기반으로 뷰(UI 텍스트/스프라이트)를 적용합니다.
        /// </summary>
        /// <param name="info">바인딩할 카드 테이블 데이터</param>
        private void ApplyCardView(StruckTableTcgCard info)
        {
            SetVisible(true);

            if (textName != null)        textName.text = info.name;
            if (textCost != null)        textCost.text = info.cost.ToString();
            if (textDescription != null) textDescription.text = info.description;

            ApplySprites(info);
        }

        /// <summary>
        /// 카드 아트/보더 스프라이트를 로드하여 적용합니다.
        /// </summary>
        /// <param name="info">스프라이트 키 생성에 사용할 카드 테이블 데이터</param>
        private void ApplySprites(StruckTableTcgCard info)
        {
            // Artwork
            if (imageArtWork != null && AddressableLoaderCard.Instance != null)
            {
                var key = BuildArtKey(info.uid);
                imageArtWork.sprite = AddressableLoaderCard.Instance.GetImageArtByName(key);
            }

            // Border (grade)
            if (imageBorder != null && AddressableLoaderCard.Instance != null)
            {
                var key = BuildBorderKey(info.grade);
                imageBorder.sprite = AddressableLoaderCard.Instance.GetImageBorderByName(key);
            }
        }

        /// <summary>
        /// CanvasGroup를 통해 아이콘 표시 여부(알파)를 제어합니다.
        /// </summary>
        /// <param name="visible">표시 여부</param>
        private void SetVisible(bool visible)
        {
            if (CanvasGroup == null) return;
            SetAlpha(visible ? 1f : 0f);
        }

        /// <summary>
        /// 카드 UID로 아트워크 주소 키를 생성합니다.
        /// </summary>
        /// <param name="uid">카드 UID</param>
        /// <returns>아트워크 주소 키 문자열</returns>
        private static string BuildArtKey(int uid) => ArtKeyPrefix + uid;

        /// <summary>
        /// 카드 등급으로 보더 주소 키를 생성합니다.
        /// (borderKeyPrefix는 표시 맥락에 따라 Hand/Field 등으로 변경될 수 있습니다.)
        /// </summary>
        /// <param name="grade">카드 등급</param>
        /// <returns>보더 주소 키 문자열</returns>
        private string BuildBorderKey(CardConstants.Grade grade) => borderKeyPrefix + grade;

        /// <summary>
        /// 아이콘 이미지 업데이트 훅입니다.
        /// 상위 시스템(UIIcon)에서 호출될 수 있으며, 현재 카드의 아트워크만 갱신합니다.
        /// </summary>
        protected override void UpdateIconImage()
        {
            if (_cardData == null) return;
            if (imageArtWork == null) return;
            if (AddressableLoaderCard.Instance == null) return;

            imageArtWork.sprite = AddressableLoaderCard.Instance.GetImageArtByName(BuildArtKey(_cardData.uid));
        }

        /// <summary>
        /// 카드 수량 표시를 갱신합니다.
        /// 수량이 1 이하이면 표시를 숨기고, 2 이상이면 "xN" 형태로 표시합니다.
        /// </summary>
        /// <param name="value">설정할 카드 수량</param>
        public override void SetCount(int value)
        {
            count = value;

            if (textCount != null)
            {
                textCount.text = count <= 1 ? string.Empty : $"x{count}";
            }
        }

        /// <summary>
        /// 현재 카드가 지정된 타입 필터(드롭다운 인덱스)에 포함되는지 확인합니다.
        /// </summary>
        /// <param name="dropdownIndex">드롭다운 선택 인덱스(0 또는 1 이하는 전체)</param>
        /// <returns>필터에 포함되면 true, 아니면 false를 반환합니다.</returns>
        public bool IsType(int dropdownIndex)
        {
            if (_cardData == null) return false;
            if (dropdownIndex <= 0) return true; // 전체

            var values = EnumCache<CardConstants.Type>.Values;
            var max = GetCount(values);
            if (dropdownIndex >= max) return false;

            return _cardData.type == values[dropdownIndex];
        }

        /// <summary>
        /// 현재 카드가 지정된 등급 필터(드롭다운 인덱스)에 포함되는지 확인합니다.
        /// </summary>
        /// <param name="dropdownIndex">드롭다운 선택 인덱스(0 또는 1 이하는 전체)</param>
        /// <returns>필터에 포함되면 true, 아니면 false를 반환합니다.</returns>
        public bool IsGrade(int dropdownIndex)
        {
            if (_cardData == null) return false;
            if (dropdownIndex <= 0) return true;

            var values = EnumCache<CardConstants.Grade>.Values;
            var max = GetCount(values);
            if (dropdownIndex >= max) return false;

            return _cardData.grade == values[dropdownIndex];
        }

        /// <summary>
        /// 현재 카드가 지정된 비용 필터(드롭다운 인덱스)에 포함되는지 확인합니다.
        /// </summary>
        /// <param name="dropdownIndex">드롭다운 선택 인덱스(0 또는 1 이하는 전체)</param>
        /// <returns>필터에 포함되면 true, 아니면 false를 반환합니다.</returns>
        public bool IsCost(int dropdownIndex)
        {
            if (_cardData == null) return false;
            if (dropdownIndex <= 0) return true; // 전체

            return _cardData.cost == dropdownIndex;
        }

        /// <summary>
        /// 카드 아이콘 클릭 이벤트를 처리합니다.
        /// 덱 편집 창이 열려 있는 경우, 현재 카드(uid)를 덱에 추가합니다.
        /// </summary>
        /// <param name="eventData">포인터 클릭 이벤트 데이터</param>
        public virtual void OnPointerClick(PointerEventData eventData)
        {
            // 윈도우 참조가 끊겼을 수 있으므로 필요 시 재획득
            if (_windowTcgMyDeckCard == null)
            {
                TryCacheDeckWindow();
                if (_windowTcgMyDeckCard == null) return;
            }

            if (_windowTcgMyDeckCard.IsOpen())
            {
                _windowTcgMyDeckCard.AddCardToDeck(uid);
            }
        }

        /// <summary>
        /// 덱 편집 UI 윈도우 참조를 캐싱합니다.
        /// 씬/매니저가 준비되지 않은 경우에는 캐싱을 수행하지 않습니다.
        /// </summary>
        private void TryCacheDeckWindow()
        {
            if (SceneGame.Instance == null) return;
            if (SceneGame.Instance.uIWindowManager == null) return;

            _windowTcgMyDeckCard =
                SceneGame.Instance.uIWindowManager
                    .GetUIWindowByUid<UIWindowTcgMyDeckCard>(UIWindowConstants.WindowUid.TcgMyDeckCard);
        }

        /// <summary>
        /// 공격력 UI 표시를 갱신합니다.
        /// 공격력이 0 이하이면 공격력 영역을 숨기고, 그 외에는 값을 표시합니다.
        /// </summary>
        /// <param name="attackValue">표시할 공격력 값</param>
        public void UpdateAttack(int attackValue)
        {
            if (textAttack == null) return;

            if (imageAttack != null)
            {
                var shouldShow = attackValue > 0;
                if (imageAttack.gameObject.activeSelf != shouldShow)
                    imageAttack.gameObject.SetActive(shouldShow);
            }

            textAttack.text = attackValue.ToString();
        }

        /// <summary>
        /// 체력 UI 표시를 갱신합니다.
        /// 체력이 0 이하이면 체력 영역을 숨기고, 그 외에는 값을 표시합니다.
        /// </summary>
        /// <param name="healthValue">표시할 체력 값</param>
        public void UpdateHealth(int healthValue)
        {
            if (textHealth == null) return;

            if (imageHealth != null)
            {
                var shouldShow = healthValue > 0;
                if (imageHealth.gameObject.activeSelf != shouldShow)
                    imageHealth.gameObject.SetActive(shouldShow);
            }

            textHealth.text = healthValue.ToString();
        }

        /// <summary>
        /// EnumCache에서 반환되는 값 컬렉션의 개수를 가능한 범위에서 계산합니다.
        /// </summary>
        /// <typeparam name="T">컬렉션 타입</typeparam>
        /// <param name="values">EnumCache가 반환한 값 컬렉션</param>
        /// <returns>개수를 알 수 있으면 그 값, 알 수 없으면 int.MaxValue를 반환합니다.</returns>
        private static int GetCount<T>(T values)
        {
            // EnumCache<T>.Values 타입이 배열/리스트/ReadOnlyList 등일 수 있어 유연하게 처리
            if (values is System.Array arr) return arr.Length;

            // NOTE: 아래 분기는 values 실제 타입에 따라 컴파일/런타임 환경이 달라질 수 있습니다.
            // 필요 시 프로젝트에 맞게 정리하거나 제거하세요.
            if (values is ICollection c2) return c2.Count;

            return int.MaxValue;
        }
    }
}
