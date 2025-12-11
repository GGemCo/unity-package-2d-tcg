using System.Collections.Generic;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 능력 실행 시 필요한 모든 컨텍스트 정보.
    /// - 어떤 카드에서 실행되는지, 시전자/상대/타겟은 누구인지 등을 포함합니다.
    /// </summary>
    public sealed class TcgAbilityContext
    {
        public TcgBattleDataMain BattleDataMain { get; }
        public TcgBattleDataSide Caster { get; }
        public TcgBattleDataSide Opponent { get; }

        public TcgBattleDataCard SourceTcgBattleDataCard { get; }
        public TcgBattleDataFieldCard TargetBattleData { get; set; }  // 필요 시 설정

        public int Value { get; set; }

        public Dictionary<string, string> ExtraParams { get; }

        public TcgAbilityContext(
            TcgBattleDataSide caster,
            TcgBattleDataSide opponent,
            TcgBattleDataCard sourceTcgBattleDataCard,
            int value,
            Dictionary<string, string> extraParams = null)
        {
            Caster = caster;
            Opponent = opponent;
            SourceTcgBattleDataCard = sourceTcgBattleDataCard;
            Value = value;
            ExtraParams = extraParams ?? new Dictionary<string, string>();
        }
    }
}