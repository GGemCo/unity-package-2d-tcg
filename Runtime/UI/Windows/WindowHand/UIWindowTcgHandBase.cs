using GGemCo2DCore;
using TMPro;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// TCG 핸드 공통 베이스 윈도우(플레이어/Enemy 공용).
    /// <para>- 영웅/핸드 카드 표시</para>
    /// <para>- 마나 텍스트 기본 표시</para>
    /// <para>- 아이콘 핸들러/드래그 전략은 파생 클래스에서 제공</para>
    /// </summary>
    public abstract class UIWindowTcgHandBase : UIWindow
    {
        [Header(UIWindowConstants.TitleHeaderIndividual)]
        public TMP_Text textCurrentMana;

        /// <summary>
        /// 각 Side 별 윈도우 UID를 반환합니다. (Player/Enemy가 다름)
        /// </summary>
        protected abstract UIWindowConstants.WindowUid WindowUid { get; }

        /// <summary>
        /// 아이콘 생성/세팅을 담당하는 핸들러를 생성합니다. (파생 클래스에서 제공)
        /// </summary>
        /// <returns>아이콘 세팅 핸들러 인스턴스.</returns>
        protected abstract ISetIconHandler CreateSetIconHandler();

        /// <summary>
        /// 드래그/드랍 동작을 정의하는 전략을 생성합니다. (파생 클래스에서 제공)
        /// </summary>
        /// <returns>드래그/드랍 전략 인스턴스.</returns>
        protected abstract IDragDropStrategy CreateDragDropStrategy();

        /// <summary>
        /// Enemy는 드래그 불가, Player는 드래그 가능하도록 여부를 계산합니다.
        /// </summary>
        private bool PossibleDrag => WindowUid != UIWindowConstants.WindowUid.TcgHandEnemy;

        /// <summary>
        /// 윈도우 초기화(핸들러/전략 등록 포함)를 수행합니다.
        /// </summary>
        protected override void Awake()
        {
            // 테이블 로더가 준비되지 않았으면 초기화를 진행하지 않습니다.
            if (TableLoaderManager.Instance == null)
                return;

            // 상위 UIWindow에서 사용하는 containerIcon이 필수라면 조기 검증합니다.
            if (containerIcon == null)
            {
                GcLogger.LogError($"{GetType().Name}: containerIcon 이 null 입니다.");
                return;
            }

            // 파생 클래스가 제공하는 UID로 윈도우를 식별합니다.
            uid = WindowUid;

            base.Awake();

            // 파생에서 제공한 핸들러/전략을 공용 매니저/핸들러에 주입합니다.
            IconPoolManager.SetSetIconHandler(CreateSetIconHandler());
            DragDropHandler.SetStrategy(CreateDragDropStrategy());
        }

        /// <summary>
        /// 윈도우 해제 시 필요한 정리 작업을 수행합니다.
        /// (현재는 비어 있으며, 파생/호출부 정책에 따라 풀 반환/이벤트 해제 등을 추가할 수 있습니다.)
        /// </summary>
        public void Release()
        {
            // Intentionally left blank.
        }

        #region Hand / Hero 표시

        /// <summary>
        /// 주어진 전투 사이드 데이터로 영웅/핸드 UI를 갱신합니다.
        /// </summary>
        /// <param name="battleDataSide">표시할 사이드의 전투 데이터.</param>
        public void RefreshHand(TcgBattleDataSide battleDataSide)
        {
            if (battleDataSide == null)
            {
                GcLogger.LogError($"{GetType().Name}: battleDataSide 가 null 입니다.");
                return;
            }

            // 핸드 카드 표시
            for (int i = 0; i < maxCountIcon; i++)
            {
                var slot = GetSlotByIndex(i);
                if (slot == null) continue;

                if (i < battleDataSide.Hand.Cards.Count)
                {
                    slot.gameObject.SetActive(true);

                    var card = battleDataSide.Hand.Cards[i];
                    var uiIcon = SetIconCount(i, card.Uid, 1);
                    if (!uiIcon) continue;
                    slot.SetAlpha(1);
                    // Enemy(AI) 쪽은 드래그 불가 처리
                    uiIcon.SetDrag(PossibleDrag);
                    uiIcon.gameObject.transform.SetParent(slot.transform, false);
                    uiIcon.gameObject.transform.localPosition = Vector3.zero;

                    // 공격/체력 표시 갱신
                    UpdateCardInfo(uiIcon, card.Attack, card.Health);
                }
                else
                {
                    // 카드가 없으면 해당 슬롯 자체를 비활성화
                    slot.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 카드형 아이콘(UIIconCard)의 공격력/체력 표시를 갱신합니다.
        /// </summary>
        /// <param name="uiIcon">갱신 대상 아이콘.</param>
        /// <param name="attack">표시할 공격력.</param>
        /// <param name="health">표시할 체력(HP).</param>
        private void UpdateCardInfo(UIIcon uiIcon, int attack, int health)
        {
            var uiIconCard = uiIcon.GetComponent<UIIconCard>();
            if (uiIconCard == null) return;

            uiIconCard.UpdateAttack(attack);
            uiIconCard.UpdateHealth(health);
        }

        #endregion

        #region Mana 표시

        /// <summary>
        /// 현재 마나/최대 마나 텍스트를 갱신합니다. (플레이어/Enemy 공통 기본 구현)
        /// </summary>
        /// <param name="currentMana">현재 마나.</param>
        /// <param name="maxMana">최대 마나.</param>
        public virtual void SetMana(int currentMana, int maxMana)
        {
            if (!textCurrentMana) return;
            textCurrentMana.text = $"{currentMana}/{maxMana}";
        }

        #endregion
    }
}
