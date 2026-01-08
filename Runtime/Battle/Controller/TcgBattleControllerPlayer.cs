using System;
using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 실제 유저(사람)의 입력을 받아 턴 단위로 실행될 명령을 큐잉(Queueing)하는 컨트롤러입니다.
    /// UI에서 발생한 카드 사용/공격/턴 종료 등의 요청을 저장해 두었다가, 턴 종료 시점에 BattleManager가 일괄 실행할 수 있도록 제공합니다.
    /// </summary>
    public sealed class TcgBattleControllerPlayer : ITcgPlayerController
    {
        /// <summary>
        /// 이 컨트롤러가 담당하는 플레이어 진영(아군/적군 등)입니다.
        /// </summary>
        public ConfigCommonTcg.TcgPlayerSide Side { get; }

        /// <summary>
        /// 플레이어 유형입니다. 이 컨트롤러는 항상 사람(Human) 플레이어를 나타냅니다.
        /// </summary>
        public ConfigCommonTcg.TcgPlayerKind Kind => ConfigCommonTcg.TcgPlayerKind.Human;

        /// <summary>
        /// UI에서 들어온 명령을 턴 종료까지 보관하는 대기 큐입니다.
        /// </summary>
        private readonly Queue<TcgBattleCommand> _pendingCommands = new Queue<TcgBattleCommand>(32);

        /// <summary>
        /// 전투 전역/주요 데이터를 참조하기 위한 컨텍스트입니다. (현재 클래스에서는 보관만 합니다)
        /// </summary>
        private TcgBattleDataMain _battleDataMain;

        /// <summary>
        /// 사람 플레이어 컨트롤러를 생성합니다.
        /// </summary>
        /// <param name="side">이 컨트롤러가 담당할 플레이어 진영입니다.</param>
        public TcgBattleControllerPlayer(ConfigCommonTcg.TcgPlayerSide side)
        {
            Side = side;
        }

        /// <summary>
        /// 전투 컨텍스트(메인 데이터)를 주입하여 컨트롤러를 초기화합니다.
        /// </summary>
        /// <param name="battleDataMain">현재 전투의 메인 데이터 컨텍스트입니다.</param>
        public void Initialize(TcgBattleDataMain battleDataMain)
        {
            _battleDataMain = battleDataMain;
        }

        /// <summary>
        /// UI에서 들어온 전투 명령을 큐에 적재합니다.
        /// 카드 사용/공격/턴 종료 요청 등이 들어오면, 턴 종료 시점까지 보관해 두었다가 일괄 처리됩니다.
        /// </summary>
        /// <param name="command">큐에 적재할 전투 명령입니다.</param>
        /// <remarks>
        /// - <paramref name="command"/>가 null이면 무시합니다.  
        /// - 명령의 <c>Side</c>가 컨트롤러의 <see cref="Side"/>와 다르면 경고 로그를 남기고 무시합니다.
        /// </remarks>
        public void EnqueueCommand(TcgBattleCommand command)
        {
            if (command == null)
                return;

            if (command.Side != Side)
            {
                GcLogger.LogWarning(
                    $"[TcgHumanController] Side mismatch. Controller={Side}, Command={command.Side}");
                return;
            }

            _pendingCommands.Enqueue(command);
        }

        /// <summary>
        /// 이번 턴에 실행할 명령 목록을 결정(수집)하여 BattleManager에 전달합니다.
        /// 내부 대기 큐의 모든 명령을 꺼내 <paramref name="outCommands"/>로 이동시키며, 호출 후 대기 큐는 비워집니다.
        /// </summary>
        /// <param name="context">현재 턴/전투 상황 컨텍스트입니다. (인터페이스 규격상 전달되며, 현재 구현에서는 사용하지 않습니다)</param>
        /// <param name="outCommands">이번 턴에 실행할 명령을 받을 리스트입니다.</param>
        /// <remarks>
        /// <paramref name="outCommands"/>가 null이면 아무 작업도 하지 않습니다.
        /// </remarks>
        public void DecideTurnActions(
            TcgBattleDataMain context,
            List<TcgBattleCommand> outCommands)
        {
            if (outCommands == null)
                return;

            outCommands.Clear();

            while (_pendingCommands.Count > 0)
            {
                outCommands.Add(_pendingCommands.Dequeue());
            }
        }

        /// <summary>
        /// 컨트롤러가 보유한 전투 컨텍스트 참조를 해제합니다.
        /// </summary>
        public void Dispose()
        {
            _battleDataMain = null;
        }
    }
}
