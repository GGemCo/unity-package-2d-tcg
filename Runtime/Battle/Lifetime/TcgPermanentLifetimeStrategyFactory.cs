namespace GGemCo2DTcg
{
    /// <summary>
    /// 테이블 정의값을 기반으로 Permanent 수명(Lifetime) 전략 인스턴스를 생성하는 팩토리입니다.
    /// </summary>
    /// <remarks>
    /// <para>
    /// 테이블(<c>StruckTableTcgCardPermanent</c>)의 Lifetime 관련 컬럼
    /// (<c>lifetimeType</c>, <c>lifetimeParamA</c>, <c>lifetimeParamB</c>)을 해석하여
    /// 적절한 <see cref="ITcgPermanentLifetimeStrategy"/> 구현체를 생성합니다.
    /// </para>
    /// <para><b>호환성 노트</b></para>
    /// <para>
    /// <c>tcg_card_permanent.txt</c>에 Lifetime 컬럼이 없더라도,
    /// 빌드 단계에서 기본값이 Indefinite로 설정되는 전제를 기반으로 기존 데이터와 호환됩니다.
    /// </para>
    /// </remarks>
    public static class TcgPermanentLifetimeStrategyFactory
    {
        /// <summary>
        /// Permanent 정의 데이터로부터 수명 전략 인스턴스를 생성합니다.
        /// </summary>
        /// <param name="definition">
        /// Permanent 테이블 행 정의 데이터입니다. <c>null</c>인 경우 기본 정책(Indefinite)을 반환합니다.
        /// </param>
        /// <returns>
        /// 정의된 Lifetime 타입에 해당하는 전략 인스턴스를 반환하며,
        /// 알 수 없는 타입이거나 정의가 비정상인 경우 기본 정책(Indefinite)으로 폴백합니다.
        /// </returns>
        public static ITcgPermanentLifetimeStrategy Build(StruckTableTcgCardPermanent definition)
        {
            if (definition == null)
                return new PermanentLifetimeIndefinite();

            return definition.lifetimeType switch
            {
                ConfigCommonTcg.TcgPermanentLifetimeType.Durability =>
                    new PermanentLifetimeDurability(definition.lifetimeParamA),

                ConfigCommonTcg.TcgPermanentLifetimeType.DurationTurns =>
                    new PermanentLifetimeDurationTurns(
                        definition.lifetimeParamA,
                        ConvertTickTrigger(definition.lifetimeParamB)),

                ConfigCommonTcg.TcgPermanentLifetimeType.TriggerCount =>
                    new PermanentLifetimeTriggerCount(definition.lifetimeParamA),

                _ => new PermanentLifetimeIndefinite()
            };
        }

        /// <summary>
        /// 테이블의 원시 값(<c>lifetimeParamB</c>)을 턴 감소 트리거 타입으로 변환합니다.
        /// </summary>
        /// <param name="raw">
        /// 트리거 타입의 원시 값입니다. 값이 0 이하이면 기본 트리거(<c>OnTurnEnd</c>)로 간주합니다.
        /// </param>
        /// <returns>변환된 <see cref="TcgAbilityConstants.TcgAbilityTriggerType"/> 값입니다.</returns>
        /// <remarks>
        /// <para>
        /// 기존 파서/데이터와의 호환을 위해 enum을 정수로도 허용하며,
        /// 이 경우 단순 캐스팅으로 처리합니다.
        /// </para>
        /// <para>
        /// NOTE: <paramref name="raw"/>가 enum 정의 범위를 벗어나는 경우도 캐스팅은 성공할 수 있으므로,
        /// 호출부/데이터 검증 계층에서 유효성 보장을 전제로 합니다.
        /// </para>
        /// </remarks>
        private static TcgAbilityConstants.TcgAbilityTriggerType ConvertTickTrigger(int raw)
        {
            // lifetimeParamB가 비어있거나 0이면, 일반적으로 "내 턴 종료"를 선택합니다.
            if (raw <= 0) return TcgAbilityConstants.TcgAbilityTriggerType.OnTurnEnd;

            // int -> enum 변환
            // (테이블에서 string으로 관리할 수도 있지만, 기존 파서와의 호환을 위해 int도 허용)
            return (TcgAbilityConstants.TcgAbilityTriggerType)raw;
        }
    }
}
