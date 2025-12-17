using System.Collections.Generic;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 단일 능력 데이터.
    /// - 카드 테이블에서 한 줄에 여러 능력를 가지는 형태라면
    ///   List&lt;TcgAbilityData&gt; 로 CardRuntime 안에 보관합니다.
    /// </summary>
    public sealed class TcgAbilityData
    {
        public TcgAbilityConstants.TcgAbilityId abilityId;
        public int value; // 데미지/힐/버프 수치 등

        public CardConstants.TargetType targetType;

        /// <summary>
        /// 추가 파라미터가 필요할 때 사용 (예: 드로우 개수와 별개 옵션 등).
        /// </summary>
        public Dictionary<string, string> extraParams;
    }
}