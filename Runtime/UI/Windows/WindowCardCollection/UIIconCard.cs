using System.Collections;
using GGemCo2DCore;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GGemCo2DTcg
{
    public class UIIconCard : UIIcon, IPointerClickHandler
    {
        [Header(UIWindowConstants.TitleHeaderIndividual)] 
        [Tooltip("이름")]
        public TextMeshProUGUI textName;
        // [Tooltip("타입")]
        // public TextMeshProUGUI textType;
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

        protected TableTcgCard tableTcgCard;
        
        private StruckTableTcgCard _struckTableTcgCard;
        private UIWindowTcgMyDeckCard _windowTcgMyDeckCard;
        
        protected override void Awake()
        {
            base.Awake();
            IconType = IconConstants.Type.TcgCard;
            _struckTableTcgCard = null;
            if (TableLoaderManager.Instance == null) return;
            tableTcgCard = TableLoaderManagerTcg.Instance.TableTcgCard;
        }

        protected override void Start()
        {
            base.Start();
            _windowTcgMyDeckCard = SceneGame.Instance.uIWindowManager.GetUIWindowByUid<UIWindowTcgMyDeckCard>(UIWindowConstants.WindowUid.TcgMyDeckCard);
        }

        public override bool ChangeInfoByUid(int cardUid, int iconCount = 0, int iconLevel = 0, bool iconIsLearn = false, int remainCoolTime = 0)
        {
            var info = tableTcgCard.GetDataByUid(cardUid);
            if (info == null)
            {
                GcLogger.LogError($"tcg_card 테이블에 없는 카드 입니다. uid: {cardUid}");
                return false;
            }
            _struckTableTcgCard = info;

            uid = cardUid;
            SetCount(iconCount);
            SetCardName();
            if (textCost != null)
            {
                textCost.text = $"{info.cost}";
            }
            if (textDescription != null)
            {
                textDescription.text = info.description;
            }

            var key = $"{ConfigAddressableKeyTcg.Card.ImageArt}_{info.uid}";
            imageArtWork.sprite = AddressableLoaderCard.Instance.GetImageArtByName(key);
            
            key = $"{ConfigAddressableKeyTcg.Card.ImageBorder}_{info.grade}";
            imageBorder.sprite = AddressableLoaderCard.Instance.GetImageBorderByName(key);

            var canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1;
            }
            
            UpdateInfo();
            return true;
        }

        private void SetCardName()
        {
            if (textName == null) return;
            textName.text = _struckTableTcgCard.name;
        }

        /// <summary>
        /// 아이콘 이미지 업데이트 하기
        /// </summary>
        protected override void UpdateIconImage()
        {
            if (imageArtWork == null) return;
            var key = $"{ConfigAddressableKeyTcg.Card.ImageArt}_{_struckTableTcgCard.uid}";
            imageArtWork.sprite = AddressableLoaderCard.Instance.GetImageArtByName(key);
        }

        public override void SetCount(int value)
        {
            count = value;
            if (textCount != null)
            {
                textCount.text = count <= 1 ? "" : $"x{count}";
            }
        }

        public bool IsType(int dropdownIndex)
        {
            // dropdownType 초기화 시 EnumCache<CardConstants.Type>.Values 순서대로 넣었다면,
            // EnumCache 를 이용해서 역매핑하면 됨.
            if (dropdownIndex <= 0) return true; // 전체
            var values = EnumCache<CardConstants.Type>.Values;
            if (dropdownIndex >= ((ICollection)values).Count) return false;
            return _struckTableTcgCard.type == values[dropdownIndex];
        }

        public bool IsGrade(int dropdownIndex)
        {
            if (dropdownIndex <= 0) return true;
            var values = EnumCache<CardConstants.Grade>.Values;
            if (dropdownIndex >= ((ICollection)values).Count) return false;
            return _struckTableTcgCard.grade == values[dropdownIndex];
        }

        public bool IsCost(int dropdownIndex)
        {
            if (dropdownIndex <= 0) return true; // 0 → 전체
            return _struckTableTcgCard.cost == dropdownIndex;
        }
        /// <summary>
        /// 덱이 선택되어있을 때, 카드를 클릭하면 바로 덱에 포함 된다.
        /// </summary>
        /// <param name="eventData"></param>
        public virtual void OnPointerClick(PointerEventData eventData)
        {
            // 덱이 선택되어있는지 체크
            if (_windowTcgMyDeckCard.IsOpen())
            {
                _windowTcgMyDeckCard.AddCardToDeck(uid);
            }
        }

        public void UpdateAttack(int attackValue)
        {
            if (!textAttack) return;
            imageAttack.gameObject.SetActive(attackValue > 0);
            textAttack.text = $"{attackValue}";
        }

        public void UpdateHealth(int healthValue, int damageValue = 0)
        {
            if (damageValue > 0)
            {
                MetadataDamageText metadataDamageText = new MetadataDamageText
                {
                    Damage = damageValue,
                    Color = Color.red,
                    WorldPosition = textHealth.transform.position + new Vector3(0, 0, 0),
                };
                SceneGame.Instance.damageTextManager.ShowDamageText(metadataDamageText);
            }
            if (!textHealth) return;
            imageHealth.gameObject.SetActive(healthValue > 0);
            textHealth.text = $"{healthValue}";
        }
    }
}