namespace GGemCo2DTcg
{
    /// <summary>
    /// TCG 카드 능력 데이터(row)를 기반으로 <see cref="TcgAbilityDefinition"/>을 생성하는 빌더(헬퍼) 클래스입니다.
    /// </summary>
    /// <remarks>
    /// 테이블/데이터 구조(<see cref="ITcgCardAbilityRow"/>)와 런타임 능력 정의(<see cref="TcgAbilityDefinition"/>)를
    /// 느슨하게 연결하여, 호출 측이 생성 규칙을 반복 구현하지 않도록 돕습니다.
    /// </remarks>
    public static class TcgAbilityBuilder
    {
        /// <summary>
        /// 카드 능력 데이터로부터 <see cref="TcgAbilityDefinition"/> 생성을 시도합니다.
        /// </summary>
        /// <typeparam name="T">
        /// 카드 능력 데이터를 표현하는 타입으로, <see cref="ITcgCardAbilityRow"/>를 구현해야 합니다.
        /// </typeparam>
        /// <param name="row">능력 생성을 위한 원본 카드 능력 데이터입니다.</param>
        /// <param name="ability">
        /// 생성에 성공하면 생성된 능력 정의가 설정되며, 실패하면 기본값(<c>default</c>)이 설정됩니다.
        /// </param>
        /// <returns><paramref name="row"/>가 null이 아니면 true, 그렇지 않으면 false를 반환합니다.</returns>
        private static bool TryBuildAbility<T>(T row, out TcgAbilityDefinition ability)
            where T : class, ITcgCardAbilityRow
        {
            if (row == null)
            {
                ability = default;
                return false;
            }

            ability = new TcgAbilityDefinition(
                row.Uid,
                row.AbilityType,
                row.TcgAbilityTriggerType,
                row.TcgAbilityTargetType,
                row.ParamA,
                row.ParamB,
                row.ParamC
            );

            return true;
        }

        /// <summary>
        /// 카드 능력 데이터로부터 <see cref="TcgAbilityDefinition"/>을 생성합니다.
        /// </summary>
        /// <typeparam name="T">
        /// 카드 능력 데이터를 표현하는 타입으로, <see cref="ITcgCardAbilityRow"/>를 구현해야 합니다.
        /// </typeparam>
        /// <param name="row">능력 생성을 위한 원본 카드 능력 데이터입니다.</param>
        /// <returns>
        /// 생성에 성공하면 <see cref="TcgAbilityDefinition"/>을 반환하며,
        /// <paramref name="row"/>가 null이면 기본값(<c>default</c>)을 반환합니다.
        /// </returns>
        public static TcgAbilityDefinition BuildAbility<T>(T row)
            where T : class, ITcgCardAbilityRow
        {
            return TryBuildAbility(row, out var ability) ? ability : default;
        }

        /// <summary>
        /// 손패의 카드 정보를 기반으로, "사용(OnPlay)" 시점에 실행될 <see cref="TcgAbilityDefinition"/>을 생성합니다.
        /// </summary>
        /// <param name="cardInHand">능력 정의를 생성할 대상 손패 카드 데이터입니다.</param>
        /// <returns>
        /// 카드 타입과 상세 데이터가 유효하면 해당 카드에 대응하는 능력 정의를 반환하고,
        /// 생성할 수 없으면 기본값(<c>default</c>)을 반환합니다.
        /// </returns>
        /// <remarks>
        /// NOTE: 현재 구현은 카드 타입별로 상세 데이터(SpellDetail 등)를 그대로 사용합니다.
        ///       단, 크리처(<see cref="CardConstants.Type.Creature"/>)는 테이블 기반 스펠 데이터가 없는 경우를 가정하여
        ///       공격력(<c>cardInHand.Attack</c>)을 ParamA로 사용하는 "임시 Damage Ability"를 생성합니다.
        ///       (Uid=1 등은 임시 값이므로, 테이블/룰이 정해지면 교체하는 것이 안전합니다.)
        /// </remarks>
        public static TcgAbilityDefinition BuildOnPlayAbilityDefinition(TcgBattleDataCardInHand cardInHand)
        {
            if (cardInHand == null) return default;

            switch (cardInHand.Type)
            {
                // 크리처 카드일 때는 Damage Ability를 임시로 생성하여 사용한다.
                case CardConstants.Type.Creature:
                    StruckTableTcgCardSpell struckTableTcgCardSpell = new StruckTableTcgCardSpell
                    {
                        Uid = 1,
                        AbilityType = TcgAbilityConstants.TcgAbilityType.Damage,
                        TcgAbilityTargetType = TcgAbilityConstants.TcgAbilityTargetType.AllEnemies,
                        TcgAbilityTriggerType = TcgAbilityConstants.TcgAbilityTriggerType.OnPlay,
                        ParamA = cardInHand.Attack
                    };
                    return BuildAbility(struckTableTcgCardSpell);

                case CardConstants.Type.Spell:
                    return BuildAbility(cardInHand.SpellDetail);

                case CardConstants.Type.Equipment:
                    return BuildAbility(cardInHand.EquipmentDetail);

                case CardConstants.Type.Permanent:
                    return BuildAbility(cardInHand.PermanentDetail);

                case CardConstants.Type.Event:
                    return BuildAbility(cardInHand.EventDetail);

                default:
                    return default;
            }
        }
    }
}
