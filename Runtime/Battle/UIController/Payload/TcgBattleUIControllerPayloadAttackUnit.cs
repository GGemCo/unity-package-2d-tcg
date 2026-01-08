namespace GGemCo2DTcg
{
    /// <summary>
    /// 유닛 간 공격(유닛 → 유닛 / 유닛 → 영웅) 연출에 사용되는 전투 결과 페이로드.
    /// </summary>
    /// <remarks>
    /// 이 페이로드는 연출(UI) 전용 데이터로,
    /// 공격 처리 이후의 체력 값과 상호 피해량을 함께 전달한다.
    /// 실제 전투 계산 로직과는 분리되어 있으며,
    /// 프리젠테이션 계층에서는 이 값을 그대로 사용해 HP 갱신 및 데미지 팝업을 표시한다.
    /// </remarks>
    public class TcgBattleUIControllerPayloadAttackUnit
    {
        /// <summary>
        /// 공격 처리 이후의 공격자 현재 체력.
        /// </summary>
        /// <remarks>
        /// 피해 적용 후 값이며, UIIconCard.UpdateHealth 등에 그대로 사용된다.
        /// </remarks>
        public int AttackerHealth { get; }

        /// <summary>
        /// 공격 처리로 인해 공격자가 받은 피해량.
        /// </summary>
        /// <remarks>
        /// 0 이상 값으로 전달되며, 연출 시 음수로 변환되어 데미지 팝업에 사용된다.
        /// </remarks>
        public int DamageToAttacker { get; }

        /// <summary>
        /// 공격 처리 이후의 대상(피격자) 현재 체력.
        /// </summary>
        /// <remarks>
        /// 피해 적용 후 값이며, UIIconCard.UpdateHealth 등에 그대로 사용된다.
        /// </remarks>
        public int TargetHealth { get; }

        /// <summary>
        /// 공격 처리로 인해 대상(피격자)이 받은 피해량.
        /// </summary>
        /// <remarks>
        /// 0 이상 값으로 전달되며, 연출 시 음수로 변환되어 데미지 팝업에 사용된다.
        /// </remarks>
        public int DamageToTarget { get; }

        /// <summary>
        /// 공격 연출에 필요한 전투 결과 데이터를 생성한다.
        /// </summary>
        /// <param name="attackerHealth">공격 처리 이후 공격자의 현재 체력.</param>
        /// <param name="damageToAttacker">공격 처리로 인해 공격자가 받은 피해량.</param>
        /// <param name="targetHealth">공격 처리 이후 대상의 현재 체력.</param>
        /// <param name="damageToTarget">공격 처리로 인해 대상이 받은 피해량.</param>
        public TcgBattleUIControllerPayloadAttackUnit(
            int attackerHealth,
            int damageToAttacker,
            int targetHealth,
            int damageToTarget)
        {
            AttackerHealth = attackerHealth;
            DamageToAttacker = damageToAttacker;
            TargetHealth = targetHealth;
            DamageToTarget = damageToTarget;
        }
    }
}
