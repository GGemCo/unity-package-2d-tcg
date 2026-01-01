namespace GGemCo2DTcg
{
    public class TcgBattleUIControllerPayloadAttackUnit
    {
        public int AttackerHealth { get; }
        public int DamageToAttacker { get; }
        public int TargetHealth { get; }
        public int DamageToTarget { get; }

        public TcgBattleUIControllerPayloadAttackUnit(int attackerHealth, int damageToAttacker, int targetHealth, int damageToTarget)
        {
            AttackerHealth = attackerHealth;
            DamageToAttacker = damageToAttacker;
            TargetHealth   = targetHealth;
            DamageToTarget    = damageToTarget;
        }
    }
}