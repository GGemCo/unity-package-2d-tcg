namespace GGemCo2DTcg
{
    /// <summary>
    /// 단일 능력 데이터.
    /// - 카드 테이블에서 한 줄에 여러 능력를 가지는 형태라면
    ///   List&lt;TcgAbilityData&gt; 로 CardRuntime 안에 보관합니다.
    /// </summary>
    public sealed class TcgAbilityData
    {
        /// <summary>
        /// 인라인 Ability 정의입니다.
        /// </summary>
        public TcgAbilityDefinition ability;


        /// <summary>
        /// (선택) 명시적으로 타겟이 결정된 경우 전달합니다.
        /// - 기본적으로 타겟 규칙은 Ability 정의(<see cref="TcgAbilityDefinition.tcgAbilityTargetType"/>)에 따릅니다.
        /// </summary>
        public TcgBattleDataCardInField explicitTarget;
    }
}