using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// TCG 전투 필드(UI) 윈도우의 공통 베이스 클래스입니다.
    /// 영웅 슬롯/아이콘 및 필드 슬롯들의 아이콘 바인딩/갱신 로직을 제공하며,
    /// Player/Enemy 등 Side 별 차이는 파생 클래스에서 UID/전략/아이콘을 결정합니다.
    /// </summary>
    public abstract class UIWindowTcgFieldBase : UIWindow
    {
        [Header(UIWindowConstants.TitleHeaderIndividual)]
        [Tooltip("영웅 슬롯")]
        public UISlot slotHero;

        [Tooltip("영웅 아이콘")]
        public UIIconCard iconHero;

        /// <summary>
        /// 이 윈도우가 사용할 Window UID를 반환합니다. (예: Player/Enemy 필드)
        /// </summary>
        protected abstract UIWindowConstants.WindowUid WindowUid { get; }

        /// <summary>
        /// 아이콘 풀에서 사용할 SetIcon 처리기(핸들러)를 생성합니다.
        /// </summary>
        /// <returns>아이콘 세팅 로직을 담당하는 핸들러.</returns>
        protected abstract ISetIconHandler CreateSetIconHandler();

        /// <summary>
        /// 드래그/드롭 처리 방식을 결정하는 전략 객체를 생성합니다.
        /// </summary>
        /// <returns>드래그/드롭 전략.</returns>
        protected abstract IDragDropStrategy CreateDragDropStrategy();

        /// <summary>
        /// 파생 클래스에서 실제로 사용할 영웅 아이콘 타입을 캐싱해 반환합니다.
        /// (예: Player/Enemy에 따라 iconHero 또는 별도 프리팹/타입을 선택)
        /// </summary>
        /// <returns>영웅으로 사용할 <see cref="UIIconCard"/>.</returns>
        protected abstract UIIconCard GetHeroIcon();

        /// <summary>
        /// 현재 윈도우에서 아이콘 드래그가 가능한지 여부를 나타냅니다.
        /// (예: Enemy 필드는 드래그 불가)
        /// </summary>
        private bool _possibleDrag;

        /// <summary>
        /// 윈도우 초기화 시점에 UID/핸들러/전략을 설정하고, Side에 따라 드래그 가능 여부를 결정합니다.
        /// </summary>
        protected override void Awake()
        {
            // 테이블 로더가 준비되지 않은 상황에서는 초기화를 진행하지 않습니다.
            if (TableLoaderManager.Instance == null)
                return;

            // UIWindow 기반에서 사용하는 컨테이너가 미할당이면 진행 불가.
            if (GcLogger.IsNullUnity(containerIcon, nameof(containerIcon))) return;

            uid = WindowUid;

            base.Awake();

            IconPoolManager.SetSetIconHandler(CreateSetIconHandler());
            DragDropHandler.SetStrategy(CreateDragDropStrategy());

            // 기본은 드래그 가능, Enemy 필드는 드래그 불가.
            _possibleDrag = true;
            if (WindowUid == UIWindowConstants.WindowUid.TcgFieldEnemy)
                _possibleDrag = false;
        }

        /// <summary>
        /// 시작 시점에 영웅 슬롯/아이콘의 기본 바인딩을 완료합니다.
        /// Awake에서 초기화되는 컴포넌트들의 값을 변경하기 위해 Start에서 처리합니다.
        /// </summary>
        protected override void Start()
        {
            base.Start();
            // 슬롯, 아이콘의 Awake함수에서 초기화 하기 때문에, 프로퍼티 값을 변경하기 위해서 Start에서 처리
            InitializeHeroObject();
        }

        /// <summary>
        /// 영웅 슬롯과 영웅 아이콘을 현재 윈도우/UID/인덱스에 맞게 초기화합니다.
        /// </summary>
        private void InitializeHeroObject()
        {
            if (GcLogger.HasAnyUnassigned(this,
                    (slotHero, nameof(slotHero)),
                    (iconHero, nameof(iconHero))))
                return;

            // Hero Slot 설정
            slotHero.window = this;
            slotHero.windowUid = uid;
            slotHero.index = ConfigCommonTcg.IndexHeroSlot;

            // Hero Icon 설정
            iconHero.window = this;
            iconHero.index = ConfigCommonTcg.IndexHeroSlot;
            iconHero.slotIndex = ConfigCommonTcg.IndexHeroSlot;
        }

        /// <summary>
        /// 인덱스에 해당하는 슬롯을 반환합니다. 영웅 슬롯 인덱스는 별도 처리합니다.
        /// </summary>
        /// <param name="index">슬롯 인덱스.</param>
        /// <returns>요청한 인덱스의 <see cref="UISlot"/>.</returns>
        public override UISlot GetSlotByIndex(int index)
        {
            if (index == ConfigCommonTcg.IndexHeroSlot) return slotHero;
            return base.GetSlotByIndex(index);
        }

        /// <summary>
        /// 인덱스에 해당하는 아이콘을 반환합니다. 영웅 아이콘 인덱스는 별도 처리합니다.
        /// </summary>
        /// <param name="index">아이콘 인덱스.</param>
        /// <returns>요청한 인덱스의 <see cref="UIIcon"/>.</returns>
        public override UIIcon GetIconByIndex(int index)
        {
            if (index == ConfigCommonTcg.IndexHeroSlot) return iconHero;
            return base.GetIconByIndex(index);
        }

        /// <summary>
        /// 전투 데이터(Side)를 기반으로 필드 UI를 갱신합니다.
        /// 영웅 카드 표시를 처리한 뒤, 필드 카드 목록을 슬롯에 바인딩하고 아이콘 상태(드래그/스탯)를 갱신합니다.
        /// </summary>
        /// <param name="battleDataSide">플레이어/적 Side의 전투 데이터.</param>
        public void RefreshField(TcgBattleDataSide battleDataSide)
        {
            if (GcLogger.IsNull(battleDataSide, nameof(battleDataSide))) return;

            // 영웅 카드 표시(공통 처리)
            SetHeroCard(battleDataSide.Field.Hero);

            for (int i = 0; i < maxCountIcon; i++)
            {
                var slot = GetSlotByIndex(i);
                if (GcLogger.IsNull(slot, nameof(slot))) continue;

                if (i < battleDataSide.Field.Cards.Count)
                {
                    slot.gameObject.SetActive(true);
                    var card = battleDataSide.Field.Cards[i];

                    var uiIcon = SetIconCount(i, card.Uid, 1);
                    if (!uiIcon)
                    {
                        // NOTE: 기존 코드의 동작을 보존합니다. (여기서 i++ 후 continue는 다음 인덱스를 스킵합니다)
                        i++;
                        continue;
                    }

                    // AI쪽은 드래그 되지 않도록 처리
                    uiIcon.SetDrag(_possibleDrag);
                    uiIcon.gameObject.transform.SetParent(slot.transform, false);
                    uiIcon.gameObject.transform.localPosition = Vector3.zero;

                    card.SetIndex(i);

                    if (slot.CanvasGroup)
                    {
                        slot.CanvasGroup.alpha = 1f;
                    }

                    // 공격/체력 표시 갱신
                    var uiIconCard = uiIcon.GetComponent<UIIconCard>();
                    if (uiIconCard != null)
                    {
                        uiIconCard.UpdateAttack(card.Attack);
                        uiIconCard.UpdateHealth(card.Health);
                    }
                }
                else
                {
                    slot.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 영웅 카드(아이콘)를 갱신합니다.
        /// 영웅이 없거나 사망(HP 0 이하)한 경우 아이콘을 비활성화합니다.
        /// </summary>
        /// <param name="heroData">영웅 전투 데이터.</param>
        private void SetHeroCard(TcgBattleDataCardInField heroData)
        {
            // 영웅 데이터가 없거나 사망한 경우: 아이콘 비활성화
            if (heroData == null || heroData.Health <= 0)
            {
                if (iconHero) iconHero.gameObject.SetActive(false);
                return;
            }

            // 파생에서 결정한 영웅 아이콘을 사용
            var heroIcon = GetHeroIcon();
            if (GcLogger.IsNull(heroIcon, nameof(heroIcon)))
            {
                if (iconHero) iconHero.gameObject.SetActive(false);
                return;
            }

            heroIcon.gameObject.SetActive(true);

            // 공통 세팅
            heroIcon.windowUid = uid;
            heroIcon.ChangeInfoByUid(heroData.Uid, 1);
            UpdateCardInfo(heroIcon, heroData.Attack, heroData.Health);

            // 파생에서 추가 작업이 필요하면 훅 제공 가능
            // OnAfterSetHeroCard(heroIcon, heroData);
        }

        /// <summary>
        /// 카드형 아이콘(<see cref="UIIconCard"/>)의 공격력/체력 표시를 갱신합니다.
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
    }
}
