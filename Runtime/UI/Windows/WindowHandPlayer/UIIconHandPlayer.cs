using GGemCo2DCore;
using UnityEngine.EventSystems;

namespace GGemCo2DTcg
{
    public class UIIconHandPlayer : UIIconCard, IPointerEnterHandler, IPointerExitHandler
    {
        private UIWindowTcgCardInfo _windowTcgCardInfo;
        private UIWindowTcgHandPlayer _windowTcgHandPlayer;
        private TcgBattleDataCard _tcgBattleDataCard;

        protected override void Awake()
        {
            base.Awake();
            
        }
        protected override void Start()
        {
            base.Start();
            _windowTcgCardInfo = SceneGame.Instance.uIWindowManager.GetUIWindowByUid<UIWindowTcgCardInfo>(UIWindowConstants.WindowUid.TcgCardInfo);
            _windowTcgHandPlayer = SceneGame.Instance.uIWindowManager.GetUIWindowByUid<UIWindowTcgHandPlayer>(UIWindowConstants.WindowUid.TcgHandPlayer);
        }
        
        private void OnDisable()
        {
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
            _windowTcgHandPlayer.OnClickCard(_tcgBattleDataCard);
        }

        public void SetCardRuntime(TcgBattleDataCard tcgBattleDataCard)
        {
            _tcgBattleDataCard = tcgBattleDataCard;
        }

        public TcgBattleDataCard GetCardRuntime()
        {
            return _tcgBattleDataCard;
        }
    }
}