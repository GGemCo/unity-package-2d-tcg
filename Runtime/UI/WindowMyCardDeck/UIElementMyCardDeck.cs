using GGemCo2DCore;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GGemCo2DTcg
{
    public class UIElementMyCardDeck : UIIcon, IPointerEnterHandler
    {
        [Header(UIWindowConstants.TitleHeaderIndividual)] 
        [Tooltip("이름")]
        public TextMeshProUGUI textName;
        // [Tooltip("타입")]
        // public TextMeshProUGUI textType;
        [Tooltip("자원 비용")]
        public TextMeshProUGUI textCost;

        [Tooltip("이미지")] 
        public Image imageArtWork;
        public Image imageBorder;
        
        private StruckTableTcgCard _struckTableTcgCard;
        private TableTcgCard _tableTcgCard;

        // todo 정리 필요
        public UIWindowMyCardDeck uiWindowMyCardDeck;
        protected override void Awake()
        {
            base.Awake();
            window = uiWindowMyCardDeck;
            windowUid = UIWindowConstants.WindowUid.TcgMyCardDeck;
            IconType = IconConstants.Type.TcgCard;
            _struckTableTcgCard = null;
            if (TableLoaderManager.Instance == null) return;
            _tableTcgCard = TableLoaderManagerTcg.Instance.TableTcgCard;
        }
        public override bool ChangeInfoByUid(int iconUid, int iconCount = 0, int iconLevel = 0, bool iconIsLearn = false, int remainCoolTime = 0)
        {
            var info = _tableTcgCard.GetDataByUid(iconUid);
            if (info == null)
            {
                GcLogger.LogError($"tcg_card 테이블에 없는 카드 입니다. uid: {iconUid}");
                return false;
            }

            uid = iconUid;
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
        }

        public override void SetCount(int value)
        {
            count = value;
            if (textCount != null)
            {
                textCount.text = count <= 1 ? "" : $"x{count}";
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            GcLogger.Log($"UIElementMyCardDeck OnPointerEnter");
            
            GameObject droppedIcon = eventData.pointerDrag;
            GameObject targetIcon = eventData.pointerEnter;
            GcLogger.Log($"droppedIcon: {droppedIcon.name}, targetIcon: {targetIcon.name}");
        }
    }
}