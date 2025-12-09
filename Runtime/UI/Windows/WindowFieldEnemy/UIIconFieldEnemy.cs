using GGemCo2DCore;
using UnityEngine.EventSystems;
using R3;

namespace GGemCo2DTcg
{
    public class UIIconFieldEnemy : UIIconCard, IPointerEnterHandler, IPointerExitHandler
    {
        private UIWindowTcgCardInfo _windowTcgCardInfo;
        private UIWindowTcgFieldEnemy _windowTcgFieldEnemy;
        private TcgBattleDataFieldCard _tcgBattleDataFieldCard;

        protected override void Awake()
        {
            base.Awake();
            
        }
        protected override void Start()
        {
            base.Start();
            _windowTcgCardInfo = SceneGame.Instance.uIWindowManager.GetUIWindowByUid<UIWindowTcgCardInfo>(UIWindowConstants.WindowUid.TcgCardInfo);
            _windowTcgFieldEnemy = SceneGame.Instance.uIWindowManager.GetUIWindowByUid<UIWindowTcgFieldEnemy>(UIWindowConstants.WindowUid.TcgFieldEnemy);
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
            if (_tcgBattleDataFieldCard == null)
            {
                GcLogger.LogError($"{nameof(TcgBattleDataFieldCard)} 정보가 없습니다.");
                return;
            }
        }

        public void SetBattleDataFieldCard(TcgBattleDataFieldCard tcgBattleDataFieldCard)
        {
            _tcgBattleDataFieldCard = tcgBattleDataFieldCard;
            
            _tcgBattleDataFieldCard.hp
                .Subscribe(SetHp)
                .AddTo(this);
        }

        private void SetHp(int value)
        {
            if (textHp == null) return;
            textHp.text = $"{value}";
        }

        public TcgBattleDataFieldCard GetBattleDataFieldCard()
        {
            return _tcgBattleDataFieldCard;
        }
    }
}