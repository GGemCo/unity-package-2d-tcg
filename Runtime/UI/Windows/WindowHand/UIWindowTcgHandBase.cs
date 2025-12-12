using GGemCo2DCore;
using TMPro;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// TCG 핸드 공통 베이스 윈도우 (플레이어/Enemy 공용)
    /// - 영웅/핸드 카드 표시
    /// - 마나 텍스트 기본 표시
    /// - 아이콘 핸들러/드래그 전략은 파생 클래스에서 제공
    /// </summary>
    public abstract class UIWindowTcgHandBase : UIWindow
    {
        [Header(UIWindowConstants.TitleHeaderIndividual)]
        public TMP_Text textCurrentMana;

        // 각 Side 별 UID (Player/Enemy 가 다름)
        protected abstract UIWindowConstants.WindowUid WindowUid { get; }

        protected abstract ISetIconHandler CreateSetIconHandler();
        protected abstract IDragDropStrategy CreateDragDropStrategy();

        protected override void Awake()
        {
            if (TableLoaderManager.Instance == null)
                return;

            if (containerIcon == null)
            {
                GcLogger.LogError($"{GetType().Name}: containerIcon 이 null 입니다.");
                return;
            }

            uid = WindowUid;

            base.Awake();

            IconPoolManager.SetSetIconHandler(CreateSetIconHandler());
            DragDropHandler.SetStrategy(CreateDragDropStrategy());
        }

        public void Release()
        {
            
        }

        #region Hand / Hero 표시

        public void RefreshHand(TcgBattleDataSide battleDataSide)
        {
            if (battleDataSide == null)
            {
                GcLogger.LogError($"{GetType().Name}: battleDataSide 가 null 입니다.");
                return;
            }

            DetachAllIcons();

            // 0번: 영웅 카드
            SetHeroCard(battleDataSide.TcgBattleDataDeck.HeroCard);

            // 1번부터: 핸드 카드
            int i = 1;
            foreach (var card in battleDataSide.Hand)
            {
                var uiIcon = SetIconCount(i, card.Uid, 1);
                if (!uiIcon) { i++; continue; }

                BindCardIcon(uiIcon, card, isHero: false);
                i++;
            }
        }

        private void SetHeroCard(TcgBattleDataCard heroCard)
        {
            if (heroCard == null) return;

            var uiIcon = SetIconCount(0, heroCard.Uid, 1);
            if (!uiIcon) return;

            BindCardIcon(uiIcon, heroCard, isHero: true);
        }

        /// <summary>
        /// 실제 UIIcon 타입(Player/Enemy 전용)으로 캐스팅/바인딩 하는 훅
        /// </summary>
        protected abstract void BindCardIcon(UIIcon uiIcon, TcgBattleDataCard card, bool isHero);

        #endregion

        #region Mana 표시

        /// <summary>
        /// 기본 마나 텍스트 표시 (플레이어/Enemy 공통 기본 구현)
        /// </summary>
        public virtual void SetMana(int currentMana, int maxMana)
        {
            if (!textCurrentMana) return;
            textCurrentMana.text = $"{currentMana}/{maxMana}";
        }

        #endregion
    }
}
