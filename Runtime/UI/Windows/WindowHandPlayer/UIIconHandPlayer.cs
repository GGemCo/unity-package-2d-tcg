using GGemCo2DCore;
using UnityEngine.EventSystems;

namespace GGemCo2DTcg
{
    public class UIIconHandPlayer : UIIconCard, IPointerEnterHandler, IPointerExitHandler
    {
        private UIWindowTcgCardInfo _windowTcgCardInfo;
        private TcgBattleDataCard _tcgBattleDataCard;
        
        // 클릭 드래그 핸들러
        private UIClickDragHandler _clickDragHandler;

        protected override void Awake()
        {
            base.Awake();
            
            // 클릭 드래그 핸들러 붙이기 (이미 붙어 있으면 재사용)
            _clickDragHandler = GetComponent<UIClickDragHandler>();
            if (_clickDragHandler == null)
                _clickDragHandler = gameObject.AddComponent<UIClickDragHandler>();
        }
        protected override void Start()
        {
            base.Start();
            _windowTcgCardInfo = SceneGame.Instance.uIWindowManager.GetUIWindowByUid<UIWindowTcgCardInfo>(UIWindowConstants.WindowUid.TcgCardInfo);
        }
        
        private void OnDisable()
        {
            if (!SceneGame.Instance) return;
            if (IsSelected())
            {
                SceneGame.Instance.uIWindowManager.ShowSelectIconImage(false);
            }
            SceneGame.Instance.uIWindowManager.ShowOverIconImage(false);
            _windowTcgCardInfo?.Show(false);
        }
        
        public override bool ChangeInfoByUid(int cardUid, int iconCount = 0, int iconLevel = 0, bool iconIsLearn = false, int remainCoolTime = 0)
        {
            var info = tableTcgCard.GetDataByUid(cardUid);
            if (info == null)
            {
                GcLogger.LogError($"tcg_card 테이블에 없는 카드 입니다. uid: {cardUid}");
                return false;
            }

            base.ChangeInfoByUid(cardUid, iconCount, iconLevel, iconIsLearn, remainCoolTime);
            return true;
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
        }

        public void OnPointerExit(PointerEventData eventData)
        {
        }

        /// <summary>
        /// 덱이 선택되어있을 때, 카드를 클릭하면 바로 덱에 포함 된다.
        /// </summary>
        /// <param name="eventData"></param>
        public override void OnPointerClick(PointerEventData eventData)
        {
            if (_tcgBattleDataCard == null)
            {
                GcLogger.LogError($"{nameof(TcgBattleDataCard)} 정보가 없습니다.");
                return;
            }
            
            // 클릭 드래그 토글
            _clickDragHandler?.ToggleClickDrag();
        }

        public void SetBattleDataCard(TcgBattleDataCard tcgBattleDataCard)
        {
            // todo 정리 필요. _tcgBattleDataCard 를 사용하지 않는 방향으로
            _tcgBattleDataCard = tcgBattleDataCard;
            
            if (_tcgBattleDataCard == null)
                return;

            // 초기값 반영
            UpdateAttack(_tcgBattleDataCard.attack.Value);
            UpdateHealth(_tcgBattleDataCard.health.Value);
        }

        public TcgBattleDataCard GetBattleDataCard()
        {
            return _tcgBattleDataCard;
        }
    }
}