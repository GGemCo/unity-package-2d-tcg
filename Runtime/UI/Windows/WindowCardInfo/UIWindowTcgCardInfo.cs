using System.Collections;
using GGemCo2DCore;
using TMPro;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 카드 상세 정보(이름/타입/등급/비용/덱 중복 제한/설명)를 표시하는 UI 윈도우입니다.
    /// 아이콘 위치를 기준으로 좌/우/사용자 지정 위치에 배치하고, 화면 밖으로 나가지 않도록 보정합니다.
    /// </summary>
    public class UIWindowTcgCardInfo : UIWindow
    {
        /// <summary>
        /// 카드 정보 윈도우의 배치 기준 타입입니다.
        /// </summary>
        public enum PositionType
        {
            /// <summary>배치 방식 미지정(사용자 지정 pivot/position 사용)</summary>
            None,

            /// <summary>아이콘의 오른쪽 방향에 표시(좌측 pivot)</summary>
            Left,

            /// <summary>아이콘의 왼쪽 방향에 표시(우측 pivot)</summary>
            Right,
        }

        /// <summary>
        /// 카드 테이블 참조입니다.
        /// </summary>
        private TableTcgCard _tableTcgCard;

        [Header(UIWindowConstants.TitleHeaderIndividual)]
        [Header("기본정보")]
        [Tooltip("카드 이름 텍스트")]
        public TextMeshProUGUI textName;

        [Tooltip("카드 타입 텍스트")]
        public TextMeshProUGUI textType;

        [Tooltip("카드 등급 텍스트")]
        public TextMeshProUGUI textGrade;

        [Tooltip("카드 소모 비용 텍스트")]
        public TextMeshProUGUI textCost;

        [Tooltip("덱에 포함 가능한 최대 중복 개수 텍스트")]
        public TextMeshProUGUI textMaxCopiesPerDeck;

        [Tooltip("카드 설명 텍스트")]
        public TextMeshProUGUI textDescription;

        /// <summary>
        /// 현재 표시 중인 카드 테이블 데이터 캐시입니다.
        /// </summary>
        private StruckTableTcgCard _struckTableTcgCard;

        /// <summary>
        /// 카드 정보 창에서 사용하는 로컬라이제이션 매니저입니다.
        /// </summary>
        private LocalizationManagerTcg _localizationManagerTcg;

        /// <summary>
        /// 컴포넌트 초기화 시 호출됩니다.
        /// 윈도우 UID를 설정하고 카드 테이블을 캐싱합니다.
        /// </summary>
        protected override void Awake()
        {
            uid = UIWindowConstants.WindowUid.TcgCardInfo;

            // 테이블 로더가 아직 준비되지 않았으면 초기화 불가(조기 반환)
            if (TableLoaderManagerTcg.Instance == null) return;

            _tableTcgCard = TableLoaderManagerTcg.Instance.TableTcgCard;
            _localizationManagerTcg = LocalizationManagerTcg.Instance;

            base.Awake();
        }

        /// <summary>
        /// 카드 UID를 바인딩하고 UI 내용을 갱신한 뒤, 아이콘 기준으로 윈도우 위치를 설정합니다.
        /// </summary>
        /// <param name="itemUid">표시할 카드 UID</param>
        /// <param name="icon">기준이 되는 카드 아이콘 오브젝트</param>
        /// <param name="type">좌/우/사용자 지정 배치 타입</param>
        /// <param name="iconSlotSize">아이콘 슬롯 크기(좌/우 배치 오프셋 계산에 사용)</param>
        /// <param name="pivot">사용자 지정 pivot(미지정 시 Vector2.zero)</param>
        /// <param name="position">사용자 지정 position(미지정 시 Vector3.zero)</param>
        public void SetCardUid(
            int itemUid,
            GameObject icon,
            PositionType type,
            Vector2 iconSlotSize,
            Vector2? pivot = null,
            Vector3? position = null)
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

            // 내용 갱신 후 표시
            Show(true);

            // 활성화된 이후 위치를 조정한다(레이아웃/Rect 갱신 타이밍 고려)
            Vector2 finalPivot = pivot ?? Vector2.zero;
            Vector3 finalPosition = position ?? Vector3.zero;

            SetPosition(icon, type, iconSlotSize, finalPivot, finalPosition);
        }

        /// <summary>
        /// 카드 이름 텍스트를 설정합니다.
        /// 로컬라이즈 템플릿(Text_Name)에 카드명 키를 적용합니다.
        /// </summary>
        private void SetName()
        {
            if (_struckTableTcgCard == null) return;

            textName.text = string.Format(
                _localizationManagerTcg.GetUIWindowCardInfoByKey("Text_Name"),
                _localizationManagerTcg.GetCardNameByKey(_struckTableTcgCard.uid.ToString()));
        }

        /// <summary>
        /// 카드 타입 텍스트를 설정합니다.
        /// 로컬라이즈 템플릿(Text_Type)에 타입 값을 적용합니다.
        /// </summary>
        private void SetType()
        {
            if (_struckTableTcgCard == null) return;

            textType.text = string.Format(
                _localizationManagerTcg.GetUIWindowCardInfoByKey("Text_Type"),
                _struckTableTcgCard.type);
        }

        /// <summary>
        /// 덱에 포함 가능한 최대 중복 개수 텍스트를 설정합니다.
        /// </summary>
        private void SetMaxCopiesPerDeck()
        {
            if (_struckTableTcgCard == null) return;
            textMaxCopiesPerDeck.text = $"MaxCopiesPerDeck: {_struckTableTcgCard.maxCopiesPerDeck}";
        }

        /// <summary>
        /// 카드 비용 텍스트를 설정합니다.
        /// </summary>
        private void SetCost()
        {
            if (_struckTableTcgCard == null) return;
            textCost.text = $"Cost: {_struckTableTcgCard.cost}";
        }

        /// <summary>
        /// 카드 등급 텍스트를 설정합니다.
        /// </summary>
        private void SetGrade()
        {
            if (_struckTableTcgCard == null) return;
            textGrade.text = $"Grade: {_struckTableTcgCard.grade}";
        }

        /// <summary>
        /// 카드 설명 텍스트를 설정합니다.
        /// </summary>
        private void SetDescription()
        {
            if (_struckTableTcgCard == null) return;
            textDescription.text = $"{_struckTableTcgCard.description}";
        }

        /// <summary>
        /// 카드 아이콘 기준으로 윈도우의 pivot과 위치를 보정합니다.
        /// </summary>
        /// <param name="icon">기준 아이콘 오브젝트</param>
        /// <param name="type">배치 타입(Left/Right/None)</param>
        /// <param name="iconSlotSize">아이콘 슬롯 크기</param>
        /// <param name="pivot">type이 None일 때 사용할 pivot</param>
        /// <param name="position">type이 None일 때 사용할 position</param>
        private void SetPosition(
            GameObject icon,
            PositionType type,
            Vector2 iconSlotSize,
            Vector2 pivot,
            Vector2 position)
        {
            RectTransform itemInfoRect = GetComponent<RectTransform>();

            if (type == PositionType.Left)
            {
                // 아이콘의 오른쪽에 표시: 좌측 pivot + (아이콘 절반 크기만큼) 우/상 이동
                itemInfoRect.pivot = new Vector2(0, 1f);
                transform.position = new Vector3(
                    icon.transform.position.x + iconSlotSize.x / 2f,
                    icon.transform.position.y + iconSlotSize.y / 2f);
            }
            else if (type == PositionType.Right)
            {
                // 아이콘의 왼쪽에 표시: 우측 pivot + (아이콘 절반 크기만큼) 좌/상 이동
                itemInfoRect.pivot = new Vector2(1f, 1f);
                transform.position = new Vector2(
                    icon.transform.position.x - iconSlotSize.x / 2f,
                    icon.transform.position.y + iconSlotSize.y / 2f);
            }
            else
            {
                // 사용자 지정 배치
                itemInfoRect.pivot = pivot;
                transform.position = position;
            }

            // 화면 밖으로 나가지 않도록 다음 프레임에서 Rect를 기준으로 클램프 처리
            StartCoroutine(DelayClampToScreen(itemInfoRect));
        }

        /// <summary>
        /// 한 프레임 대기 후 RectTransform을 화면 영역 내로 보정하는 코루틴입니다.
        /// </summary>
        /// <param name="rectTransform">보정할 RectTransform</param>
        /// <returns>코루틴 IEnumerator</returns>
        private IEnumerator DelayClampToScreen(RectTransform rectTransform)
        {
            yield return null; // 한 프레임 대기(레이아웃 반영 후 보정)
            MathHelper.ClampToScreen(rectTransform);
        }
    }
}
