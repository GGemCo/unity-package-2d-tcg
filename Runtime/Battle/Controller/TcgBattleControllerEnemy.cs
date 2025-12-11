using System.Collections.Generic;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 기본 규칙 기반 AI 컨트롤러 예시.
    /// - 실제 로직은 추후 점수 평가/프로파일과 함께 확장 가능합니다.
    /// </summary>
    public sealed class TcgBattleControllerEnemy : ITcgPlayerController
    {
        public ConfigCommonTcg.TcgPlayerSide Side { get; }
        public ConfigCommonTcg.TcgPlayerKind Kind { get; }

        private TcgBattleDataMain _battleDataMain;
        private TcgBattleDataSide _me;
        private TcgBattleDataSide _opponent;

        public TcgBattleControllerEnemy(
            ConfigCommonTcg.TcgPlayerSide side,
            ConfigCommonTcg.TcgPlayerKind kind = ConfigCommonTcg.TcgPlayerKind.AiEasy)
        {
            Side = side;
            Kind = kind;
        }

        public void Initialize(TcgBattleDataMain battleDataMain)
        {
            _battleDataMain = battleDataMain;
            _me = battleDataMain.GetSideState(Side);
            _opponent = battleDataMain.GetOpponentState(Side);
        }

        public void DecideTurnActions(
            TcgBattleDataMain context,
            List<TcgBattleCommand> outCommands)
        {
            // 방어 코드
            if (outCommands == null)
                return;

            outCommands.Clear();

            // 1) 낼 수 있는 카드 중 코스트가 맞는 카드 찾아서 1장 사용(예시)
            foreach (var card in _me.Hand)
            {
                if (card.Cost <= _me.CurrentManaValue)
                {
                    outCommands.Add(TcgBattleCommand.PlayCard(Side, card));
                    break;
                }
            }

            // 2) 필드에 유닛이 있으면, 적 유닛/영웅 공격 명령 추가 (아주 단순한 예시)
            foreach (var myUnit in _me.Board)
            {
                if (!myUnit.CanAttack)
                    continue;

                if (_opponent.Board.Count > 0)
                {
                    // 가장 체력이 낮은 유닛을 대상으로 공격
                    TcgBattleDataFieldCard lowHpTarget = null;
                    int minHp = int.MaxValue;
                    foreach (var enemy in _opponent.Board)
                    {
                        if (enemy.hp.Value < minHp)
                        {
                            minHp = enemy.hp.Value;
                            lowHpTarget = enemy;
                        }
                    }

                    if (lowHpTarget != null)
                    {
                        outCommands.Add(
                            TcgBattleCommand.AttackUnit(Side, myUnit, lowHpTarget));
                    }
                }
                else
                {
                    outCommands.Add(
                        TcgBattleCommand.AttackHero(Side, myUnit));
                }
            }

            // 3) 마지막에 턴 종료 명령 추가
            outCommands.Add(TcgBattleCommand.EndTurn(Side));
        }

        public void Dispose()
        {
            _battleDataMain = null;
            _me = null;
            _opponent = null;
        }
    }
}
