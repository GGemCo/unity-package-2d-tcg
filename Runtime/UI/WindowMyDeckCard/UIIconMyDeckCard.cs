using System;
using GGemCo2DCore;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GGemCo2DTcg
{
    public class UIIconMyDeckCard : UIIcon, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header(UIWindowConstants.TitleHeaderIndividual)] 
        [Tooltip("카드 이름")]
        public TextMeshProUGUI textName;

        private int _deckIndex;
        private UIWindowMyDeckCard _windowMyDeckCard;
        private StruckTableTcgCard _struckTableTcgCard;
        private MyDeckData _myDeckData;
        private UIWindowCardInfo _windowCardInfo;
        
        protected override void Start()
        {
            base.Start();
            _windowCardInfo = SceneGame.Instance.uIWindowManager.GetUIWindowByUid<UIWindowCardInfo>(UIWindowConstants.WindowUid.TcgCardInfo);
        }

        private void OnDisable()
        {
            if (IsSelected())
            {
                SceneGame.Instance.uIWindowManager.ShowSelectIconImage(false);
            }
            SceneGame.Instance.uIWindowManager.ShowOverIconImage(false);
            _windowCardInfo?.Show(false);
        }

        public override bool ChangeInfoByUid(int cardUid, int iconCount = 0, int iconLevel = 0, bool iconIsLearn = false, int remainCoolTime = 0)
        {
            uid = cardUid;
            SetCount(iconCount);
            _struckTableTcgCard = TableLoaderManagerTcg.Instance.TableTcgCard.GetDataByUid(cardUid);
            
            if (textName != null)
            {
                textName.text = _struckTableTcgCard.name;
            }
            _windowMyDeckCard = window as UIWindowMyDeckCard;
            _myDeckData = TcgPackageManager.Instance.saveDataManagerTcg.MyDeck;
            UpdateInfo();
            return true;
        }

        public override void ClearIconInfos()
        {
            base.ClearIconInfos();
            if (textName != null)
            {
                textName.text = "";
            }
        }

        public override void SetCount(int value)
        {
            base.SetCount(value);
            if (textCount == null) return;
            textCount.text = $"x{value}";
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            window.ShowItemInfo(true, this);
            ShowOverImage(true);
        }
        /// <summary>
        /// 클랙하면, 덱에서 빼기
        /// </summary>
        /// <param name="eventData"></param>
        public void OnPointerClick(PointerEventData eventData)
        {
            // GcLogger.Log($"UIIconMyDeckCard OnPointerClick");
            if (!PossibleClick) return;
            if (IsLock()) return;
            if(eventData.button == PointerEventData.InputButton.Left)
            {
                if (!window) return;
                window.SetSelectedIcon(index);
            }
            else if(eventData.button == PointerEventData.InputButton.Middle)
            {
            }
            else if(eventData.button == PointerEventData.InputButton.Right)
            {
                if (uid <= 0 || GetCount() <= 0) return;
                window.OnRightClick(this);
            }
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            // GcLogger.Log("OnPointerExit "+eventData);
            window.ShowItemInfo(false);
            ShowOverImage(false);
        }

        public void SetDeckIndex(int deckIndex)
        {
            _deckIndex = deckIndex;
        }
    }
}