using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 실제 유저(사람)를 위한 컨트롤러.
    /// - UI에서 발생한 입력을 큐에 쌓아두고,
    ///   턴 종료 시점에 BattleManager가 한꺼번에 실행합니다.
    /// </summary>
    public sealed class TcgHumanController : ITcgPlayerController
    {
        public ConfigCommonTcg.TcgPlayerSide Side { get; }
        public ConfigCommonTcg.TcgPlayerKind Kind => ConfigCommonTcg.TcgPlayerKind.Human;

        private readonly Queue<TcgBattleCommand> _pendingCommands = new Queue<TcgBattleCommand>(32);
        private TcgBattleContext _context;

        public TcgHumanController(ConfigCommonTcg.TcgPlayerSide side)
        {
            Side = side;
        }

        public void Initialize(TcgBattleContext context)
        {
            _context = context;
        }

        /// <summary>
        /// UI 쪽에서 호출하는 메서드.
        /// - 카드 사용/공격/턴 종료 요청이 들어오면 큐에 쌓습니다.
        /// </summary>
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
        /// 이번 턴에 수행할 명령들을 턴 종료 시점에 BattleManager가 가져갈 때 사용.
        /// - 내부 큐를 모두 비우고 outCommands 로 옮깁니다.
        /// </summary>
        public void DecideTurnActions(
            TcgBattleContext context,
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
    }
}