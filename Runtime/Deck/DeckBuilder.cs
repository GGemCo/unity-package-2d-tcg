using System.Collections.Generic;
using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    public static class DeckBuilder
    {
        public static List<CardRuntime> BuildRuntimeDeck(Dictionary<int,int> cardList)
        {
            var table = TableLoaderManagerTcg.Instance.TableTcgCard;
            var list = new List<CardRuntime>();

            foreach (var info in cardList)
            {
                int uid = info.Key;
                if (!table.TryGetDataByUid(uid, out var row))
                {
                    GcLogger.LogError($"Card UID {uid} not found in table.");
                    continue;
                }
                CardRuntime card = TcgCardRuntimeFactory.CreateCardRuntime(row);

                list.Add(card);
            }

            return list;
        }
    }
}