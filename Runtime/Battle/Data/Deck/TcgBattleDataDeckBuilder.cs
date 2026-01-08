using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 덱 구성 데이터(UID 목록 등)를 기반으로 전투 런타임 카드 객체를 생성하는 빌더입니다.
    /// 카드 테이블에서 UID로 행 데이터를 조회한 뒤 <see cref="TcgBattleDataCardFactory"/>를 통해
    /// <see cref="TcgBattleDataCardInHand"/> 인스턴스를 생성합니다.
    /// </summary>
    public static class TcgBattleDataDeckBuilder
    {
        /// <summary>
        /// 덱 카드 UID 목록을 런타임 카드 리스트로 변환합니다.
        /// </summary>
        /// <param name="cardList">
        /// 카드 UID와 (보통) 수량을 담은 딕셔너리입니다.
        /// 현재 구현은 수량(<c>Value</c>)을 사용하지 않고 UID(<c>Key</c>)만을 사용합니다.
        /// </param>
        /// <returns>생성된 런타임 손패 카드 목록입니다.</returns>
        /// <remarks>
        /// - 카드 UID가 테이블에 없으면 에러 로그를 남기고 해당 항목은 건너뜁니다.
        /// - 현재는 <paramref name="cardList"/>의 수량 정보가 반영되지 않습니다.
        ///   (필요 시 수량만큼 여러 장을 생성하거나, 별도 스택 구조로 처리하도록 확장할 수 있습니다.)
        /// </remarks>
        public static List<TcgBattleDataCardInHand> BuildRuntimeDeckCardList(Dictionary<int, int> cardList)
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

        /// <summary>
        /// 지정한 UID의 히어로 카드를 테이블에서 조회하여 런타임 카드로 생성합니다.
        /// </summary>
        /// <param name="uid">히어로 카드 UID입니다.</param>
        /// <returns>생성된 런타임 히어로 카드이며, UID가 유효하지 않으면 null을 반환합니다.</returns>
        /// <remarks>
        /// 테이블에 UID가 없으면 에러 로그를 남깁니다.
        /// </remarks>
        public static TcgBattleDataCardInHand BuildRuntimeHeroCard(int uid)
        {
            var table = TableLoaderManagerTcg.Instance.TableTcgCard;

            if (table.TryGetDataByUid(uid, out var row))
                return TcgBattleDataCardFactory.CreateBattleDataCard(row);

            GcLogger.LogError($"Card UID {uid} not found in table.");
            return null;
        }
    }
}
