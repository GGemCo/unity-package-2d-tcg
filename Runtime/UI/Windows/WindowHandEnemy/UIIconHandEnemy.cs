using GGemCo2DCore;
using UnityEngine.EventSystems;

namespace GGemCo2DTcg
{
    public class UIIconHandEnemy : UIIconCard, IPointerEnterHandler, IPointerExitHandler
    {
        private UIWindowTcgCardInfo _windowTcgCardInfo;
        private TcgBattleDataCard _tcgBattleDataCard;

        protected override void Start()
        {
            base.Start();
            _windowTcgCardInfo = SceneGame.Instance.uIWindowManager.GetUIWindowByUid<UIWindowTcgCardInfo>(UIWindowConstants.WindowUid.TcgCardInfo);
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
            // _windowTcgHandEnemy.OnClickCard(_tcgBattleDataCard);
        }

        public void SetBattleDataCard(TcgBattleDataCard tcgBattleDataCard)
        {
            _tcgBattleDataCard = tcgBattleDataCard;
            
            if (_tcgBattleDataCard == null)
                return;

            // 초기값 반영
            UpdateAttack(_tcgBattleDataCard.attack.Value);
            UpdateHealth(_tcgBattleDataCard.health.Value);
        }
    }
}