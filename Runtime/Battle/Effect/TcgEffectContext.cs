using System.Collections.Generic;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 이펙트 실행 시 필요한 모든 컨텍스트 정보.
    /// - 어떤 카드에서 실행되는지, 시전자/상대/타겟은 누구인지 등을 포함합니다.
    /// </summary>
    public sealed class TcgEffectContext
    {
        public TcgBattleContext BattleContext { get; }
        public TcgBattleSideState Caster { get; }
        public TcgBattleSideState Opponent { get; }

        public CardRuntime SourceCard { get; }
        public TcgUnitRuntime TargetUnit { get; set; }  // 필요 시 설정

        public int Value { get; set; }

        public Dictionary<string, string> ExtraParams { get; }

        public TcgEffectContext(
            TcgBattleContext battleContext,
            TcgBattleSideState caster,
            TcgBattleSideState opponent,
            CardRuntime sourceCard,
            int value,
            Dictionary<string, string> extraParams = null)
        {
            BattleContext = battleContext;
            Caster = caster;
            Opponent = opponent;
            SourceCard = sourceCard;
            Value = value;
            ExtraParams = extraParams ?? new Dictionary<string, string>();
        }
    }
}