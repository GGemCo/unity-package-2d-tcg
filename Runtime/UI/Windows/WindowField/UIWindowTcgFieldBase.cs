using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    public abstract class UIWindowTcgFieldBase : UIWindow
    {
        [Header(UIWindowConstants.TitleHeaderIndividual)]
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
        
        public void RefreshBoard(TcgBattleDataSide battleDataSide)
        {
            if (GcLogger.IsNull(battleDataSide, nameof(battleDataSide))) return;

            for (int i = 0; i < maxCountIcon; i++)
            {
                var slot = GetSlotByIndex(i);
                if (GcLogger.IsNull(slot, nameof(slot))) continue;
                if (i < battleDataSide.Board.Cards.Count)
                {
                    slot.gameObject.SetActive(true);
                    var card = battleDataSide.Board.Cards[i];
                
                    var uiIcon = SetIconCount(i, card.Uid, 1);
                    if (!uiIcon) { i++; continue; }

                    // AI쪽은 드래그 되지 않도록 처리
                    uiIcon.SetDrag(_possibleDrag);
                    
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
    }
}