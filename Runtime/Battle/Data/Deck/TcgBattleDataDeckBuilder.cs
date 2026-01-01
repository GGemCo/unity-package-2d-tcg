using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    public static class TcgBattleDataDeckBuilder
    {
        public static List<TcgBattleDataCardInHand> BuildRuntimeDeckCardList(Dictionary<int,int> cardList)
        {
            var table = TableLoaderManagerTcg.Instance.TableTcgCard;
            var list = new List<TcgBattleDataCardInHand>();

            foreach (var info in cardList)
            {
                int uid = info.Key;
                if (!table.TryGetDataByUid(uid, out var row))
                {
                    GcLogger.LogError($"Card UID {uid} not found in table.");
                    continue;
                }
                TcgBattleDataCardInHand tcgBattleDataCardInHand = TcgBattleDataCardFactory.CreateBattleDataCard(row);

                list.Add(tcgBattleDataCardInHand);
            }

            return list;
        }

        public static TcgBattleDataCardInHand BuildRuntimeHeroCard(int uid)
        {
            var table = TableLoaderManagerTcg.Instance.TableTcgCard;
            if (table.TryGetDataByUid(uid, out var row)) return TcgBattleDataCardFactory.CreateBattleDataCard(row);
            
            GcLogger.LogError($"Card UID {uid} not found in table.");
            return null;
        }
    }
}