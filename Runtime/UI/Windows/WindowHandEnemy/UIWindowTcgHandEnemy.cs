using System.Collections.Generic;
using GGemCo2DCore;
using TMPro;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 상대 AI의 카드가 있는 윈도우
    /// </summary>
    public class UIWindowTcgHandEnemy : UIWindow
    {
        [Header(UIWindowConstants.TitleHeaderIndividual)]
        public TMP_Text textCurrentMana;
        
        private TcgBattleControllerEnemy _battleControllerEnemy;

        protected override void Awake()
        {
            if (TableLoaderManager.Instance == null)
            {
                return;
            }
            if (containerIcon == null)
            {
                GcLogger.LogError($"{nameof(UIWindowTcgHandEnemy)}: containerIcon 이 null 입니다.");
                return;
            }
            
            uid = UIWindowConstants.WindowUid.TcgHandEnemy;

            base.Awake();

            IconPoolManager.SetSetIconHandler(new SetIconHandlerEnemy());
            DragDropHandler.SetStrategy(new DragDropStrategyHandEnemy());
        }

        public void RefreshHand(IReadOnlyList<TcgBattleDataCard> hand)
        {
            // 기존 SetFirstCard 기반 구현을 확장하여,
            // hand 리스트를 기준으로 슬롯/아이콘 다시 배치
            DetachAllIcons();
            int i = 0;
            foreach (var tcgBattleDataCard in hand)
            {
                var uiIcon = SetIconCount(i, tcgBattleDataCard.Uid, 1);
                if (!uiIcon) continue;
                var uiIconHandEnemy = uiIcon as UIIconHandEnemy;
                if (!uiIconHandEnemy) continue;
                uiIconHandEnemy.SetBattleDataCard(tcgBattleDataCard);
                i++;
            }
        }
        /// <summary>
        /// 현재 마나 표시 정보 업데이트
        /// </summary>
        /// <param name="currentMana">현재 마나</param>
        /// <param name="maxMana">현재 최대 마나</param>
        public void SetMana(int currentMana, int maxMana)
        {
            if (!textCurrentMana) return;
            textCurrentMana.text = $"{currentMana}/{maxMana}";
        }
    }
}