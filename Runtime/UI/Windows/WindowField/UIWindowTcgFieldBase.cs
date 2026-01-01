using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    public abstract class UIWindowTcgFieldBase : UIWindow
    {
        [Header(UIWindowConstants.TitleHeaderIndividual)]
        [Tooltip("영웅 슬롯")]
        public UISlot slotHero;

        [Tooltip("영웅 아이콘")]
        public UIIconCard iconHero;
        
        public Easing.EaseType fadeInEasing  = Easing.EaseType.EaseOutSine;
        public float fadeInDuration = 0.6f;
        
        public Easing.EaseType fadeOutEasing  = Easing.EaseType.EaseOutSine;
        public float fadeOutDuration = 0.6f;
        public float fadeOutDelayTime = 0.5f;
        
        // 1) "대상보다 조금 왼쪽 아래" 오프셋 (월드 좌표 기준)
        public Vector3 leftDownOffset = new Vector3(-24f, -18f, 0f);

        // 2) "뒤로" 이동 거리(타겟에서 멀어지는 방향으로)
        public Easing.EaseType backEasing  = Easing.EaseType.EaseOutSine;
        public float backDistance = 28f;
        public float backDuration = 0.5f;

        public Easing.EaseType hitEasing  = Easing.EaseType.EaseInQuintic;
        public float hitDuration  = 0.2f; // 빠르게 타격
        
        // 각 Side 별 UID (Player/Enemy 가 다름)
        protected abstract UIWindowConstants.WindowUid WindowUid { get; }
        
        protected abstract ISetIconHandler CreateSetIconHandler();
        protected abstract IDragDropStrategy CreateDragDropStrategy();

        /// <summary>
        /// 파생 클래스에서 실제로 사용할 영웅 아이콘 타입을 캐싱해 반환합니다.
        /// </summary>
        /// <returns>영웅으로 사용할 <see cref="UIIconCard"/>.</returns>
        protected abstract UIIconCard GetHeroIcon();
        
        private bool _possibleDrag;
        
        protected override void Awake()
        {
            if (TableLoaderManager.Instance == null)
                return;

            if (GcLogger.IsNullUnity(containerIcon, nameof(containerIcon))) return;

            uid = WindowUid;

            base.Awake();

            IconPoolManager.SetSetIconHandler(CreateSetIconHandler());
            DragDropHandler.SetStrategy(CreateDragDropStrategy());
            _possibleDrag = true;
            if (WindowUid == UIWindowConstants.WindowUid.TcgFieldEnemy)
                _possibleDrag = false;
        }
        /// <summary>
        /// 시작 시점에 영웅 슬롯/아이콘의 기본 바인딩을 완료합니다.
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
                    if (!uiIcon) { i++; continue; }

                    // AI쪽은 드래그 되지 않도록 처리
                    uiIcon.SetDrag(_possibleDrag);
                    uiIcon.gameObject.transform.SetParent(slot.transform, false);
                    uiIcon.gameObject.transform.localPosition = Vector3.zero;
                    
                    card.SetIndex(i);
                    // GcLogger.Log($"window: {WindowUid}, uid: {card.Uid}, index: {i}");
                    if (slot.CanvasGroup)
                    {
                        slot.CanvasGroup.alpha = 1f;
                    }
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
        /// 영웅이 없거나 사망한 경우 아이콘을 비활성화합니다.
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

    }
}