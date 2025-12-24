using System.Collections;
using GGemCo2DCore;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GGemCo2DTcg
{
    /// <summary>
    /// TCG 카드 UI 아이콘.
    /// - 테이블 데이터(StruckTableTcgCard)를 바인딩하고,
    /// - 아트/보더 스프라이트를 로드하여 표시합니다.
    /// - 덱 편집 창이 열려 있을 때 클릭 시 덱에 카드 추가를 수행합니다.
    /// </summary>
    public class UIIconCard : UIIcon, IPointerClickHandler
    {
        [Header(UIWindowConstants.TitleHeaderIndividual)]
        [Tooltip("이름")]
        public TextMeshProUGUI textName;

        [Tooltip("자원 비용")]
        public TextMeshProUGUI textCost;

        [Tooltip("설명")]
        public TextMeshProUGUI textDescription;

        public TextMeshProUGUI textAttack;
        public TextMeshProUGUI textHealth;

        [Tooltip("이미지")]
        public Image imageArtWork;
        public Image imageBorder;

        public Image imageAttack;
        public Image imageHealth;
        public Image imageMana;

        protected TableTcgCard tableTcgCard;

        protected StruckTableTcgCard CurrentCard => _cardData;

        private StruckTableTcgCard _cardData;
        private UIWindowTcgMyDeckCard _windowTcgMyDeckCard;
        private CanvasGroup _canvasGroup;

        private static readonly string ArtKeyPrefix    = $"{ConfigAddressableKeyTcg.Card.ImageArt}_";
        private static readonly string BorderKeyPrefix = $"{ConfigAddressableKeyTcg.Card.ImageBorder}_";

        protected override void Awake()
        {
            base.Awake();

            IconType = IconConstants.Type.TcgCard;
            _cardData = null;

            _canvasGroup = GetComponent<CanvasGroup>();

            // TableLoader가 준비되지 않았을 수 있으므로 안전하게 접근
            if (TableLoaderManager.Instance != null && TableLoaderManagerTcg.Instance != null)
            {
                tableTcgCard = TableLoaderManagerTcg.Instance.TableTcgCard;
            }
        }

        protected override void Start()
        {
            base.Start();
            TryCacheDeckWindow();
        }

        public override bool ChangeInfoByUid(int cardUid, int iconCount = 0, int iconLevel = 0, bool iconIsLearn = false, int remainCoolTime = 0)
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

            UpdateInfo(); // 상위(UIIcon) 공통 갱신 루틴이 있다면 유지
            return true;
        }

        /// <summary>
        /// 현재 카드 데이터 기반으로 뷰(UI 텍스트/스프라이트 등)를 적용합니다.
        /// </summary>
        private void ApplyCardView(StruckTableTcgCard info)
        {
            SetVisible(true);

            if (textName != null)        textName.text = info.name;
            if (textCost != null)        textCost.text = info.cost.ToString();
            if (textDescription != null) textDescription.text = info.description;

            ApplySprites(info);
        }

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

        private void SetVisible(bool visible)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = visible ? 1f : 0f;
            }
        }

        private static string BuildArtKey(int uid) => ArtKeyPrefix + uid;
        private static string BuildBorderKey(CardConstants.Grade grade) => BorderKeyPrefix + grade;

        /// <summary>
        /// 아이콘 이미지 업데이트(상위 시스템에서 호출될 수 있음).
        /// </summary>
        protected override void UpdateIconImage()
        {
            if (_cardData == null) return;
            if (imageArtWork == null) return;
            if (AddressableLoaderCard.Instance == null) return;

            imageArtWork.sprite = AddressableLoaderCard.Instance.GetImageArtByName(BuildArtKey(_cardData.uid));
        }

        public override void SetCount(int value)
        {
            count = value;

            if (textCount != null)
            {
                textCount.text = count <= 1 ? string.Empty : $"x{count}";
            }
        }

        public bool IsType(int dropdownIndex)
        {
            if (_cardData == null) return false;
            if (dropdownIndex <= 0) return true; // 전체

            var values = EnumCache<CardConstants.Type>.Values;
            var max = GetCount(values);
            if (dropdownIndex >= max) return false;

            return _cardData.type == values[dropdownIndex];
        }

        public bool IsGrade(int dropdownIndex)
        {
            if (_cardData == null) return false;
            if (dropdownIndex <= 0) return true;

            var values = EnumCache<CardConstants.Grade>.Values;
            var max = GetCount(values);
            if (dropdownIndex >= max) return false;

            return _cardData.grade == values[dropdownIndex];
        }

        public bool IsCost(int dropdownIndex)
        {
            if (_cardData == null) return false;
            if (dropdownIndex <= 0) return true; // 전체

            return _cardData.cost == dropdownIndex;
        }

        /// <summary>
        /// 덱이 선택되어있을 때, 카드를 클릭하면 바로 덱에 포함합니다.
        /// </summary>
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

        private void TryCacheDeckWindow()
        {
            if (SceneGame.Instance == null) return;
            if (SceneGame.Instance.uIWindowManager == null) return;

            _windowTcgMyDeckCard =
                SceneGame.Instance.uIWindowManager
                    .GetUIWindowByUid<UIWindowTcgMyDeckCard>(UIWindowConstants.WindowUid.TcgMyDeckCard);
        }

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

        public void UpdateHealth(int healthValue, int damageValue = 0)
        {
            if (damageValue > 0)
            {
                ShowDamageText(damageValue);
            }

            if (textHealth == null) return;

            if (imageHealth != null)
            {
                var shouldShow = healthValue > 0;
                if (imageHealth.gameObject.activeSelf != shouldShow)
                    imageHealth.gameObject.SetActive(shouldShow);
            }

            textHealth.text = healthValue.ToString();
        }

        private void ShowDamageText(int damageValue)
        {
            // 텍스트가 없거나 매니저가 없으면 스킵
            if (textHealth == null) return;
            if (SceneGame.Instance == null) return;
            if (SceneGame.Instance.damageTextManager == null) return;

            var metadata = new MetadataDamageText
            {
                Damage = damageValue,
                Color = Color.red,
                WorldPosition = textHealth.transform.position
            };

            SceneGame.Instance.damageTextManager.ShowDamageText(metadata);
        }

        private static int GetCount<T>(T values)
        {
            // EnumCache<T>.Values 타입이 배열/리스트/ReadOnlyList 등일 수 있어 유연하게 처리
            if (values is System.Array arr) return arr.Length;
            if (values is System.Collections.Generic.ICollection<CardConstants.Type> c1) return c1.Count; // 타입에 따라 컴파일 오류 가능(필요 시 제거)
            if (values is ICollection c2) return c2.Count;
            return int.MaxValue;
        }
    }
}
