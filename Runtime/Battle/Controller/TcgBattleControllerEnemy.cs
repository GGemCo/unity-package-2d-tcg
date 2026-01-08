using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 기본 규칙(룰 기반)으로 동작하는 적(AI) 컨트롤러 예시입니다.
    /// </summary>
    /// <remarks>
    /// - 이 컨트롤러는 “명령 목록을 생성”하는 역할만 수행하며, 실제 커맨드 실행은 전투 시스템/세션이 담당합니다.
    /// - 현재 구현은 매우 단순한 휴리스틱(공격 가능하면 공격, 낼 수 있으면 1장 플레이, 턴 종료)으로 구성되어 있습니다.
    /// - 추후 점수 평가(Heuristic scoring), 룰/프로파일 기반 의사결정 등으로 확장하기 쉽도록 형태를 유지합니다.
    /// </remarks>
    public sealed class TcgBattleControllerEnemy : ITcgPlayerController
    {
        /// <summary>
        /// 이 컨트롤러가 조종하는 진영(Side)입니다.
        /// </summary>
        public ConfigCommonTcg.TcgPlayerSide Side { get; }

        /// <summary>
        /// AI 난이도/종류를 나타내는 값입니다.
        /// </summary>
        public ConfigCommonTcg.TcgPlayerKind Kind { get; }

        private TcgBattleDataMain _battleDataMain;
        private TcgBattleDataSide _me;
        private TcgBattleDataSide _opponent;

        /// <summary>
        /// <see cref="TcgBattleControllerEnemy"/>를 생성합니다.
        /// </summary>
        /// <param name="side">AI가 조종할 진영(Side)입니다.</param>
        /// <param name="kind">AI의 종류/난이도입니다.</param>
        public TcgBattleControllerEnemy(
            ConfigCommonTcg.TcgPlayerSide side,
            ConfigCommonTcg.TcgPlayerKind kind = ConfigCommonTcg.TcgPlayerKind.AiEasy)
        {
            Side = side;
            Kind = kind;
        }

        /// <summary>
        /// 전투 데이터 참조를 주입하여 컨트롤러를 초기화합니다.
        /// </summary>
        /// <param name="battleDataMain">전투 진행에 필요한 메인 데이터입니다.</param>
        public void Initialize(TcgBattleDataMain battleDataMain)
        {
            _battleDataMain = battleDataMain;
            _me = battleDataMain.GetSideState(Side);
            _opponent = battleDataMain.GetOpponentState(Side);
        }

        /// <summary>
        /// 현재 턴에 수행할 커맨드 목록을 결정하여 <paramref name="outCommands"/>에 채웁니다.
        /// </summary>
        /// <param name="context">의사결정 시점의 전투 컨텍스트입니다.</param>
        /// <param name="outCommands">결정된 커맨드를 누적할 출력 리스트입니다.</param>
        /// <remarks>
        /// 현재 구현 정책:
        /// <list type="number">
        /// <item><description>공격 가능 유닛은 가능한 한 공격합니다(우선순위: 적 영웅 → 적 유닛(최저 체력)).</description></item>
        /// <item><description>마나로 낼 수 있는 카드가 있으면 1장만 필드로 플레이합니다.</description></item>
        /// <item><description>마지막에 턴 종료 커맨드를 추가합니다.</description></item>
        /// </list>
        /// </remarks>
        public void DecideTurnActions(
            TcgBattleDataMain context,
            List<TcgBattleCommand> outCommands)
        {
            // 방어 코드
            if (outCommands == null)
                return;

            outCommands.Clear();

            // 1) 공격 커맨드 생성(아주 단순한 예시)
            // - deadCardPlayer: "이번 의사결정 동안 이미 죽을 것으로 확정된" 플레이어 유닛을 제외하기 위한 캐시
            // - alreadyAttack: 한 턴에 한 번만 공격하도록 중복 공격 방지(인덱스 기반)
            // NOTE: deadCardEnemy는 현재 로직에서 사용되지 않습니다(확장 대비 또는 제거 가능).
            Dictionary<int, TcgBattleDataCardInField> deadCardEnemy = new Dictionary<int, TcgBattleDataCardInField>();
            Dictionary<int, TcgBattleDataCardInField> deadCardPlayer = new Dictionary<int, TcgBattleDataCardInField>();
            Dictionary<int, TcgBattleDataCardInField> alreadyAttack = new Dictionary<int, TcgBattleDataCardInField>();

            foreach (var battleDataFieldCard in _me.Field.Cards)
            {
                if (!battleDataFieldCard.CanAttack)
                    continue;

                if (battleDataFieldCard.Health <= 0)
                {
                    GcLogger.LogError($"[AI공격] hp가 0인데 공격 시도");
                    continue;
                }

                // 이미 공격한 카드는 스킵합니다.
                if (alreadyAttack.ContainsKey(battleDataFieldCard.Index))
                    continue;

                // 우선: 영웅 공격(영웅이 생존해 있으면 무조건 영웅을 목표로 함)
                if (_opponent.Field.Hero.Health > 0)
                {
                    outCommands.Add(
                        TcgBattleCommand.AttackHero(
                            Side,
                            ConfigCommonTcg.TcgZone.FieldEnemy,
                            battleDataFieldCard,
                            ConfigCommonTcg.TcgZone.FieldPlayer,
                            _opponent.Field.Hero));

                    alreadyAttack.TryAdd(battleDataFieldCard.Index, battleDataFieldCard);
                }
                else if (_opponent.Field.Count > 0)
                {
                    // 영웅이 없거나(사망) 공격 대상이 영웅이 아니면, 적 유닛 중 "체력이 가장 낮은" 대상을 찾습니다.
                    TcgBattleDataCardInField lowHpTarget = null;
                    int minHp = int.MaxValue;

                    foreach (var player in _opponent.Field.Cards)
                    {
                        // 이미 사망 확정(예측)된 타겟은 제외합니다.
                        if (deadCardPlayer.ContainsKey(player.Index))
                            continue;

                        if (player.Health < minHp)
                        {
                            minHp = player.Health;
                            lowHpTarget = player;
                        }
                    }

                    if (lowHpTarget != null)
                    {
                        outCommands.Add(
                            TcgBattleCommand.AttackUnit(
                                Side,
                                ConfigCommonTcg.TcgZone.FieldEnemy,
                                battleDataFieldCard,
                                ConfigCommonTcg.TcgZone.FieldPlayer,
                                lowHpTarget));

                        alreadyAttack.TryAdd(battleDataFieldCard.Index, battleDataFieldCard);

                        // 타겟 사망이 확정이면(간단 예측), 이후 다른 공격자가 같은 타겟을 고르는 것을 방지합니다.
                        if (minHp - battleDataFieldCard.Attack <= 0)
                            deadCardPlayer.TryAdd(lowHpTarget.Index, lowHpTarget);
                    }
                }
                else
                {
                    // 대상이 없는 경우(상대 필드가 비어 있음)에는 아무 것도 하지 않습니다.
                    // 필요하다면 여기서 “대기/특수 행동” 등을 추가할 수 있습니다.
                }
            }

            // 2) 낼 수 있는 카드 중 코스트가 맞는 카드 1장만 플레이(예시)
            foreach (var card in _me.Hand.Cards)
            {
                if (card.Cost <= _me.Mana.Current)
                {
                    outCommands.Add(
                        TcgBattleCommand.DrawCardToField(
                            Side,
                            ConfigCommonTcg.TcgZone.HandEnemy,
                            ConfigCommonTcg.TcgZone.FieldEnemy,
                            card));
                    break;
                }
            }

            // 3) 마지막에 턴 종료
            outCommands.Add(TcgBattleCommand.EndTurn(Side));
        }

        /// <summary>
        /// 컨트롤러가 보유한 전투 참조를 해제합니다.
        /// </summary>
        public void Dispose()
        {
            _battleDataMain = null;
            _me = null;
            _opponent = null;
        }
    }
}
