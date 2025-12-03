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
        
        public TextMeshProUGUI textAtk;
        public TextMeshProUGUI textHp;

        [Tooltip("이미지")] 
        public Image imageArtWork;
        public Image imageBorder;
        
        public Image imageAtk;
        public Image imageHp;

        private StruckTableTcgCard _struckTableTcgCard;
        private TableTcgCard _tableTcgCard;
        private MyDeckData _myDeckData;
        private UIWindowTcgMyDeckCard _windowTcgMyDeckCard;
        
        protected override void Awake()
        {
            base.Awake();
            IconType = IconConstants.Type.TcgCard;
            _struckTableTcgCard = null;
            if (TableLoaderManager.Instance == null) return;
            _tableTcgCard = TableLoaderManagerTcg.Instance.TableTcgCard;
            
        }

        protected override void Start()
        {
            base.Start();
            _myDeckData = TcgPackageManager.Instance.saveDataManagerTcg.MyDeck;
            _windowTcgMyDeckCard = SceneGame.Instance.uIWindowManager.GetUIWindowByUid<UIWindowTcgMyDeckCard>(UIWindowConstants.WindowUid.TcgMyDeckCard);
        }

        public override bool ChangeInfoByUid(int deckIndex, int iconCount = 0, int iconLevel = 0, bool iconIsLearn = false, int remainCoolTime = 0)
        {
            var info = _tableTcgCard.GetDataByUid(deckIndex);
            if (info == null)
            {
                GcLogger.LogError($"tcg_card 테이블에 없는 카드 입니다. uid: {deckIndex}");
                return false;
            }

            uid = deckIndex;
            SetCount(iconCount);
            _struckTableTcgCard = info;
            
            if (textName != null)
            {
                textName.text = info.name;
            }
            if (textCost != null)
            {
                textCost.text = $"{info.cost}";
            }
            if (textDescription != null)
            {
                textDescription.text = info.description;
            }

            ShowAtkHp(false);

            var key = $"{ConfigAddressableKeyTcg.Card.ImageArt}_{info.uid}";
            imageArtWork.sprite = AddressableLoaderCard.Instance.GetImageArtByName(key);
            
            key = $"{ConfigAddressableKeyTcg.Card.ImageBorder}_{info.grade}";
            imageBorder.sprite = AddressableLoaderCard.Instance.GetImageBorderByName(key);

            InitializeCreature();
            
            UpdateInfo();
            return true;
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
        private void InitializeCreature()
        {
            if (_struckTableTcgCard.type != CardConstants.Type.Creature) return;
            ShowAtkHp(true);
            
            if (textAtk != null)
            {
            }
        }

        private void ShowAtkHp(bool show)
        {
            if (imageAtk != null)
            {
                imageAtk.gameObject.SetActive(show);
            }
            if (imageHp != null)
            {
                imageHp.gameObject.SetActive(show);
            }
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
        public void OnPointerClick(PointerEventData eventData)
        {
            // 덱이 선택되어있는지 체크
            if (!_windowTcgMyDeckCard.IsOpen())
            {
                return;
            }
            _windowTcgMyDeckCard.AddCardToDeck(uid);
        }
    }
}