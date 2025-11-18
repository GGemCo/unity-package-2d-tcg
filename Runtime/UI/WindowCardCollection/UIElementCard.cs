using GGemCo2DCore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GGemCo2DTcg
{
    public class UIElementCard : UIIcon
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
        
        protected override void Awake()
        {
            base.Awake();
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
    }
}