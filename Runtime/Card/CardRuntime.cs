
namespace GGemCo2DTcg
{
    /// <summary>
    /// 셔플/드로우 시스템에서 사용하는 카드 최소 정보 인터페이스.
    /// 가중 셔플 등을 위해 코스트 등 기본 스탯을 제공한다.
    /// </summary>
    public interface ICardInfo
    {
        /// <summary>카드 플레이 코스트.</summary>
        int Cost { get; }
        // 필요 시 Type, Grade 등도 여기서 노출 가능:
        // CardConstants.Type Type { get; }
        // CardConstants.Grade Grade { get; }
    }

    /// <summary>
    /// TCG 카드의 런타임 표현.
    /// TableTcgCard + 현재 전투에서의 상태(버프, 변형 등)를 담는 그릇으로 사용할 수 있다.
    /// </summary>
    public class CardRuntime : ICardInfo
    {
        public int Uid { get; }
        public int Cost { get; }

        // 예: 테이블/상수에서 가져온 정보
        // public CardConstants.Type Type { get; }
        // public CardConstants.Grade Grade { get; }

        public CardRuntime(int uid, int cost)
        {
            Uid = uid;
            Cost = cost;
        }

        // TableTcgCard 연동 팩토리 메서드 
        public static CardRuntime FromTable(StruckTableTcgCard row)
        {
            return new CardRuntime(row.uid, row.cost);
        }
    }
}