namespace GGemCo2DTcg
{
    /// <summary>
    /// TCG 카드 능력 데이터(row)를 기반으로 <see cref="TcgAbilityDefinition"/>을 생성하는 헬퍼 클래스입니다.
    /// </summary>
    public static class TcgAbilityBuilder
    {
        /// <summary>
        /// 카드 능력 데이터로부터 <see cref="TcgAbilityDefinition"/> 생성을 시도합니다.
        /// </summary>
        /// <typeparam name="T">
        /// 카드 능력 데이터를 표현하는 타입으로, <see cref="ITcgCardAbilityRow"/>를 구현해야 합니다.
        /// </typeparam>
        /// <param name="row">
        /// 능력 생성을 위한 원본 카드 능력 데이터입니다.
        /// </param>
        /// <param name="ability">
        /// 생성에 성공한 경우 생성된 능력 정의가 설정되며, 실패 시 기본값이 설정됩니다.
        /// </param>
        /// <returns>
        /// <paramref name="row"/>가 유효하여 능력 생성에 성공하면 true, 그렇지 않으면 false를 반환합니다.
        /// </returns>
        private static bool TryBuildAbility<T>(T row, out TcgAbilityDefinition ability)
            where T : class, ITcgCardAbilityRow
        {
            if (row == null)
            {
                ability = default;
                return false;
            }

            ability = new TcgAbilityDefinition(
                row.uid,
                row.abilityType,
                row.tcgAbilityTriggerType,
                row.tcgAbilityTargetType,
                row.paramA,
                row.paramB,
                row.paramC
            );

            return true;
        }

        /// <summary>
        /// 카드 능력 데이터로부터 <see cref="TcgAbilityDefinition"/>을 생성합니다.
        /// </summary>
        /// <typeparam name="T">
        /// 카드 능력 데이터를 표현하는 타입으로, <see cref="ITcgCardAbilityRow"/>를 구현해야 합니다.
        /// </typeparam>
        /// <param name="row">
        /// 능력 생성을 위한 원본 카드 능력 데이터입니다.
        /// </param>
        /// <returns>
        /// 생성에 성공한 경우 <see cref="TcgAbilityDefinition"/>을 반환하며,
        /// <paramref name="row"/>가 null인 경우 기본값을 반환합니다.
        /// </returns>
        public static TcgAbilityDefinition BuildAbility<T>(T row)
            where T : class, ITcgCardAbilityRow
        {
            return TryBuildAbility(row, out var ability) ? ability : default;
        }
    }
}
