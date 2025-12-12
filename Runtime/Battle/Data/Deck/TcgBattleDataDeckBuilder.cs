using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    public static class TcgBattleDataDeckBuilder
    {
        public static List<TcgBattleDataCard> BuildRuntimeDeckCardList(Dictionary<int,int> cardList)
        {
            var table = TableLoaderManagerTcg.Instance.TableTcgCard;
            var list = new List<TcgBattleDataCard>();

            foreach (var info in cardList)
            {
                int uid = info.Key;
                if (!table.TryGetDataByUid(uid, out var row))
                {
                    GcLogger.LogError($"Card UID {uid} not found in table.");
                    continue;
                }
                TcgBattleDataCard tcgBattleDataCard = TcgBattleDataCardFactory.CreateCardRuntime(row);

                list.Add(tcgBattleDataCard);
            }

            return list;
        }

        public static TcgBattleDataCard BuildRuntimeHeroCard(int uid)
        {
            var table = TableLoaderManagerTcg.Instance.TableTcgCard;
            if (table.TryGetDataByUid(uid, out var row)) return TcgBattleDataCardFactory.CreateCardRuntime(row);
            
            GcLogger.LogError($"Card UID {uid} not found in table.");
            return null;
        }
    }
}