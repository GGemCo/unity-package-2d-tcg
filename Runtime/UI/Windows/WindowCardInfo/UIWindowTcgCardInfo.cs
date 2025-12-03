using System;
using System.Collections;
using System.Collections.Generic;
using GGemCo2DCore;
using TMPro;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 카드 정보 윈도우
    /// </summary>
    public class UIWindowTcgCardInfo : UIWindow
    {
        public enum PositionType
        {
            None,
            Left,
            Right,
        }
        private TableTcgCard _tableTcgCard;
        [Header(UIWindowConstants.TitleHeaderIndividual)]
        [Header("기본정보")]
        [Tooltip("카드 이름")]
        public TextMeshProUGUI textName;
        [Tooltip("카드 타입")]
        public TextMeshProUGUI textType;
        [Tooltip("카드 등급")]
        public TextMeshProUGUI textGrade;
        [Tooltip("카드 소모 비용")]
        public TextMeshProUGUI textCost;
        [Tooltip("카드 중복 소지 가능 개수")]
        public TextMeshProUGUI textMaxCopiesPerDeck;
        [Tooltip("카드 설명")]
        public TextMeshProUGUI textDescription;
        
        private StruckTableTcgCard _struckTableTcgCard;
        private LocalizationManagerTcg _localizationManagerTcg;
        
        protected override void Awake()
        {
            uid = UIWindowConstants.WindowUid.TcgCardInfo;
            if (TableLoaderManager.Instance == null) return;
            _tableTcgCard = TableLoaderManagerTcg.Instance.TableTcgCard;
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
            _localizationManagerTcg = LocalizationManagerTcg.Instance;
        }

        public void SetCardUid(int itemUid, GameObject icon, PositionType type, Vector2 iconSlotSize, Vector2? pivot = null, Vector3? position = null)
        {
            if (icon == null || itemUid <= 0) return;
            _struckTableTcgCard = _tableTcgCard.GetDataByUid(itemUid);
            if (_struckTableTcgCard is not { uid: > 0 }) return;
            
            SetName();
            SetType();
            SetGrade();
            SetCost();
            SetMaxCopiesPerDeck();
            SetDescription();
            Show(true);
            // active 된 후 위치 조정한다.
            
            // null 체크 후 기본값 대입 (예: pivot이 null이면 Vector2.zero 사용)
            Vector2 finalPivot = pivot ?? Vector2.zero;
            Vector3 finalPosition = position ?? Vector3.zero;
            SetPosition(icon, type, iconSlotSize, finalPivot, finalPosition);
        }

        /// <summary>
        /// 이름 설정하기
        /// </summary>
        private void SetName()
        {
            if (_struckTableTcgCard == null) return;
            textName.text = string.Format(_localizationManagerTcg.GetUIWindowCardInfoByKey("Text_Name"),
            _localizationManagerTcg.GetCardNameByKey(_struckTableTcgCard.uid.ToString()));
        }
        /// <summary>
        /// 타입 설정하기
        /// </summary>
        private void SetType()
        {
            if (_struckTableTcgCard == null) return;
            textType.text = string.Format(_localizationManagerTcg.GetUIWindowCardInfoByKey("Text_Type"), _struckTableTcgCard.type);
        }
        private void SetMaxCopiesPerDeck()
        {
            if (_struckTableTcgCard == null) return;
            textMaxCopiesPerDeck.text = $"MaxCopiesPerDeck: {_struckTableTcgCard.maxCopiesPerDeck}";
        }

        private void SetCost()
        {
            if (_struckTableTcgCard == null) return;
            textCost.text = $"Cost: {_struckTableTcgCard.cost}";
        }

        private void SetGrade()
        {
            if (_struckTableTcgCard == null) return;
            textGrade.text = $"Grade: {_struckTableTcgCard.grade}";
        }

        private void SetDescription()
        {
            if (_struckTableTcgCard == null) return;
            textDescription.text = $"{_struckTableTcgCard.description}";
        }

        /// <summary>
        /// 위치 보정하기
        /// </summary>
        /// <param name="icon"></param>
        /// <param name="type"></param>
        /// <param name="iconSlotSize"></param>
        /// <param name="pivot"></param>
        /// <param name="position"></param>
        private void SetPosition(GameObject icon, PositionType type, Vector2 iconSlotSize, Vector2 pivot, Vector2 position)
        {
            RectTransform itemInfoRect = GetComponent<RectTransform>();
            if (type == PositionType.Left)
            {
                itemInfoRect.pivot = new Vector2(0, 1f);
                transform.position = new Vector3(
                    icon.transform.position.x + iconSlotSize.x / 2f,
                    icon.transform.position.y + iconSlotSize.y / 2f);
            }
            else if (type == PositionType.Right)
            {
                itemInfoRect.pivot = new Vector2(1f, 1f);
                transform.position = new Vector2(
                    icon.transform.position.x - iconSlotSize.x / 2f,
                    icon.transform.position.y + iconSlotSize.y / 2f);
            }
            else
            {
                itemInfoRect.pivot = pivot;
                transform.position = position;
            }

            // 화면 밖 체크 & 보정
            StartCoroutine(DelayClampToScreen(itemInfoRect));
        }
        /// <summary>
        /// 위치 보정 코루틴
        /// </summary>
        /// <param name="rectTransform"></param>
        /// <returns></returns>
        private IEnumerator DelayClampToScreen(RectTransform rectTransform)
        {
            yield return null; // 한 프레임 대기
            MathHelper.ClampToScreen(rectTransform);
        }
    }
}