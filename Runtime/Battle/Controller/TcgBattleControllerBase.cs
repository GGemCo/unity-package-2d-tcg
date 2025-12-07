using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    public class TcgBattleControllerBase
    {
        protected TcgBattleDataSide battleDataSide;
        protected TcgBattleManager battleManager;
        protected TcgPackageManager packageManager;
        protected SaveDataManagerTcg saveDataManagerTcg;
        protected GGemCoTcgSettings tcgSettings;
        protected SeedManager seedManager;

        public virtual TcgBattleDataSide Initialize(TcgBattleManager tcgBattleManager)
        {
            this.battleManager = tcgBattleManager;
            packageManager = TcgPackageManager.Instance;
            saveDataManagerTcg = packageManager.saveDataManagerTcg;
            tcgSettings = AddressableLoaderSettingsTcg.Instance.tcgSettings;
            seedManager = new SeedManager();
            return null;
        }

        protected void InitializeSideState(ConfigCommonTcg.TcgPlayerSide side, TcgBattleDataDeck<TcgBattleDataCard> tcgBattleDataDeck)
        {
            battleDataSide = new TcgBattleDataSide(side, tcgBattleDataDeck);
            
            int startingHandCardCount = tcgSettings.startingHandCardCount;
            if (startingHandCardCount <= 0)
            {
                GcLogger.LogError($"{nameof(GGemCoTcgSettings)} 스크립터블 오브젝트에서 startingHandCardCount 값을 셋팅해주세요.");
                return;
            }
            for (int i = 0; i < startingHandCardCount; i++)
            {
                if (tcgBattleDataDeck.TryDraw(out var firstCard))
                {
                    GcLogger.Log($"드로우 {i+1}: {firstCard.Uid}");
                }
                battleDataSide.AddCardToHand(firstCard);
            }
            battleDataSide.InitializeMana(1);
        }
        public void IncreaseMaxMana(int addAmount)
        {
            if (battleDataSide == null)
            {
                GcLogger.LogError($"{nameof(TcgBattleDataSide)} 클래스가 생성되지 않았습니다.");
                return;
            }
            battleDataSide.IncreaseMaxMana(addAmount);
        }

        public IReadOnlyList<TcgBattleDataFieldCard> GetBoardCards()
        {
            return battleDataSide.Board;
        }
        public int GetCurrentMana()
        {
            return battleDataSide.CurrentMana;
        }
        public int GetCurrentMaxMana()
        {
            return battleDataSide.CurrentMaxMana;
        }

        public TcgBattleDataSide GetBattleDataSide()
        {
            return battleDataSide;
        }
    }
}