namespace GGemCo2DTcg
{
    /// <summary>
    /// Permanent Lifetime 전략 생성기.
    /// - 테이블( StruckTableTcgCardPermanent )의 Lifetime 컬럼을 읽어
    ///   적절한 전략 인스턴스를 생성합니다.
    /// 
    /// NOTE:
    /// - tcg_card_permanent.txt에 Lifetime 컬럼이 없더라도
    ///   BuildRow에서 Indefinite로 기본 설정되므로 기존 데이터와 호환됩니다.
    /// </summary>
    public static class TcgPermanentLifetimeStrategyFactory
    {
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
