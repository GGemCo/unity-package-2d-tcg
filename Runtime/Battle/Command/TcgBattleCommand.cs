using System.Collections.Generic;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 전투 중 플레이어(사람/AI)가 요청하는 단일 액션을 표현하는 데이터 모델.
    /// - 이 클래스 자체는 "어떻게 실행할지" 를 알지 않고,
    ///   오직 "무엇을 할지" 만 표현합니다.
    /// </summary>
    public sealed class TcgBattleCommand
    {
        public ConfigCommonTcg.TcgBattleCommandType CommandType { get; private set; }

        /// <summary>
        /// 이 명령을 요청한 플레이어 측.
        /// </summary>
        public ConfigCommonTcg.TcgPlayerSide Side { get; private set; }

        /// <summary>
        /// 카드 사용 시: 손에서 사용할 카드 런타임 참조.
        /// </summary>
        public TcgBattleDataCard tcgBattleDataCard;

        /// <summary>
        /// 공격 시: 공격자 유닛.
        /// </summary>
        public TcgBattleDataFieldCard Attacker;

        /// <summary>
        /// 공격 시: 대상 유닛 (영웅 공격이면 null 로 사용).
        /// </summary>
        public TcgBattleDataFieldCard targetBattleData;
        // 영웅 공격시
        public TcgBattleDataFieldCard targetBattleDataHero;

        /// <summary>
        /// 확장용 추가 데이터 (예: 스펠 턴수, 선택된 옵션 등).
        /// </summary>
        public Dictionary<string, object> ExtraData;

        private TcgBattleCommand() { }

        public static TcgBattleCommand PlayCard(
            ConfigCommonTcg.TcgPlayerSide side,
            TcgBattleDataCard tcgBattleDataCard)
        {
            return new TcgBattleCommand
            {
                CommandType = ConfigCommonTcg.TcgBattleCommandType.PlayCardFromHand,
                Side = side,
                tcgBattleDataCard = tcgBattleDataCard
            };
        }

        public static TcgBattleCommand AttackUnit(
            ConfigCommonTcg.TcgPlayerSide side,
            TcgBattleDataFieldCard attacker,
            TcgBattleDataFieldCard target)
        {
            return new TcgBattleCommand
            {
                CommandType = ConfigCommonTcg.TcgBattleCommandType.AttackUnit,
                Side = side,
                Attacker = attacker,
                targetBattleData = target
            };
        }

        public static TcgBattleCommand AttackHero(
            ConfigCommonTcg.TcgPlayerSide side,
            TcgBattleDataFieldCard attacker,
            TcgBattleDataFieldCard target)
        {
            return new TcgBattleCommand
            {
                CommandType = ConfigCommonTcg.TcgBattleCommandType.AttackHero,
                Side = side,
                Attacker = attacker,
                targetBattleDataHero = target
            };
        }

        public static TcgBattleCommand EndTurn(ConfigCommonTcg.TcgPlayerSide side)
        {
            return new TcgBattleCommand
            {
                CommandType = ConfigCommonTcg.TcgBattleCommandType.EndTurn,
                Side = side
            };
        }
    }
}
