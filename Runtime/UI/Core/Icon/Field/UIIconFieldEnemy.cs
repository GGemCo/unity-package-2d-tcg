using GGemCo2DCore;
using UnityEngine.EventSystems;
using R3;

namespace GGemCo2DTcg
{
    public class UIIconFieldEnemy : UIIconCard, IPointerEnterHandler, IPointerExitHandler
    {
        private UIWindowTcgCardInfo _windowTcgCardInfo;
        private TcgBattleDataCardInField _tcgBattleDataCardInField;
        // private readonly CompositeDisposable _bindDisposables = new();

        protected override void Awake()
        {
            base.Awake();
            borderKeyPrefix = $"{ConfigAddressableKeyTcg.Card.ImageBorderField}_";
        }
        protected override void Start()
        {
            base.Start();
            _windowTcgCardInfo = SceneGame.Instance.uIWindowManager.GetUIWindowByUid<UIWindowTcgCardInfo>(UIWindowConstants.WindowUid.TcgCardInfo);
        }
        
        private void OnDisable()
        {
            // if (IsSelected())
            // {
            //     SceneGame.Instance.uIWindowManager.ShowSelectIconImage(false);
            // }
            // SceneGame.Instance.uIWindowManager.ShowOverIconImage(false);
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
            if (_tcgBattleDataCardInField == null)
            {
                GcLogger.LogError($"{nameof(TcgBattleDataCardInField)} 정보가 없습니다.");
                return;
            }
        }
    }
}