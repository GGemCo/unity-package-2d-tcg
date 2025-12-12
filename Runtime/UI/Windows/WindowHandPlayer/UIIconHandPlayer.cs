using GGemCo2DCore;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GGemCo2DTcg
{
    public class UIIconHandPlayer : UIIconCard, IPointerEnterHandler, IPointerExitHandler
    {
        private UIWindowTcgCardInfo _windowTcgCardInfo;
        private UIWindowTcgHandPlayer _windowTcgHandPlayer;
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
            _windowTcgHandPlayer = SceneGame.Instance.uIWindowManager.GetUIWindowByUid<UIWindowTcgHandPlayer>(UIWindowConstants.WindowUid.TcgHandPlayer);
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
        
        public override bool ChangeInfoByUid(int deckIndex, int iconCount = 0, int iconLevel = 0, bool iconIsLearn = false, int remainCoolTime = 0)
        {
            var info = tableTcgCard.GetDataByUid(deckIndex);
            if (info == null)
            {
                GcLogger.LogError($"tcg_card 테이블에 없는 카드 입니다. uid: {deckIndex}");
                return false;
            }

            base.ChangeInfoByUid(deckIndex, iconCount, iconLevel, iconIsLearn, remainCoolTime);
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
            _tcgBattleDataCard = tcgBattleDataCard;
        }

        public TcgBattleDataCard GetCardRuntime()
        {
            return _tcgBattleDataCard;
        }
    }
}