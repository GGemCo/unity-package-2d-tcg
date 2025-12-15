using System;
using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 한 번의 전투 진행(턴, 커맨드 실행, 승패 판정 등) 을 담당하는 순수 도메인 클래스.
    /// UI, MonoBehaviour, Scene 에 의존하지 않도록 설계합니다.
    /// </summary>
    public sealed class TcgBattleSession : IDisposable
    {
        /// <summary>
        /// 전투 전체 상태 (플레이어/적, 보드, 덱, 묘지 등).
        /// 기존 프로젝트의 TcgBattleDataMain 을 그대로 사용합니다.
        /// </summary>
        public TcgBattleDataMain Context { get; }

        /// <summary>
        /// 현재 턴이 플레이어 차례인지 여부.
        /// </summary>
        public bool IsPlayerTurn => Context.ActiveSide == ConfigCommonTcg.TcgPlayerSide.Player;

        /// <summary>
        /// 전투가 종료되었는지 여부.
        /// </summary>
        public bool IsBattleEnded { get; private set; }

        /// <summary>
        /// 승리한 편. 아직 종료되지 않았다면 None.
        /// </summary>
        public ConfigCommonTcg.TcgPlayerSide Winner { get; private set; }
        

        private readonly Dictionary<ConfigCommonTcg.TcgBattleCommandType, ITcgBattleCommandHandler> _commandHandlers;
        private readonly ITcgPlayerController _playerController;
        private readonly ITcgPlayerController _enemyController;

        // 내부 버퍼: 한 턴 동안 실행할 커맨드를 임시 보관.
        private readonly List<TcgBattleCommand> _commandBuffer = new List<TcgBattleCommand>(32);
        // CommandResult.FollowUpCommands 까지 포함해서 처리할 실행 큐
        private readonly List<TcgBattleCommand> _executionQueue = new List<TcgBattleCommand>(64);
        private readonly GGemCoTcgSettings _tcgSettings;
        private readonly SystemMessageManager _systemMessageManager;

        public TcgBattleSession(
            TcgBattleDataSide playerSide,
            TcgBattleDataSide enemySide,
            Dictionary<ConfigCommonTcg.TcgBattleCommandType, ITcgBattleCommandHandler> commandHandlers,
            ITcgPlayerController playerController,
            ITcgPlayerController enemyController,
            GGemCoTcgSettings settings,
            SystemMessageManager systemMessageManager)
        {
            _commandHandlers  = commandHandlers ?? throw new ArgumentNullException(nameof(commandHandlers));
            _playerController = playerController ?? throw new ArgumentNullException(nameof(playerController));
            _enemyController  = enemyController ?? throw new ArgumentNullException(nameof(enemyController));
            _systemMessageManager  = systemMessageManager ?? throw new ArgumentNullException(nameof(systemMessageManager));
            _tcgSettings  = settings ?? throw new ArgumentNullException(nameof(settings));

            Context = new TcgBattleDataMain(this, playerSide, enemySide, systemMessageManager)
            {
                // 기본값: 플레이어부터 시작
                ActiveSide = ConfigCommonTcg.TcgPlayerSide.Player
            };

            IsBattleEnded      = false;
            Winner             = ConfigCommonTcg.TcgPlayerSide.None;
            
            // 컨트롤러 초기화
            _playerController.Initialize(Context);
            _enemyController.Initialize(Context);
        }

        /// <summary>
        /// 플레이어 턴용으로 커맨드를 수집/실행합니다. 
        /// (UI에서 직접 커맨드를 넣는 구조라면 사용하지 않고, UI에서 ExecuteCommand를 직접 호출하게 할 수 있습니다.)
        /// </summary>
        public void ExecutePlayerTurn()
        {
            if (IsBattleEnded) return;
            if (!IsPlayerTurn) return;

            _commandBuffer.Clear();
            _playerController.DecideTurnActions(Context, _commandBuffer);
            ExecuteCommands(_commandBuffer);
        }

        /// <summary>
        /// 플레이어 턴을 실행하면서, 실행된 커맨드들의 Trace를 수집합니다.
        /// UI에서 연출을 재생하고 싶을 때 사용합니다.
        /// </summary>
        public void ExecutePlayerTurnWithTrace(List<TcgBattleCommandTrace> traces)
        {
            if (IsBattleEnded) return;
            if (!IsPlayerTurn) return;

            _commandBuffer.Clear();
            _playerController.DecideTurnActions(Context, _commandBuffer);
            ExecuteCommandsWithTrace(_commandBuffer, traces);
        }

        /// <summary>
        /// AI 턴용으로 커맨드를 수집/실행합니다.
        /// </summary>
        public void ExecuteEnemyTurn()
        {
            if (IsBattleEnded) return;
            if (IsPlayerTurn) return;

            _commandBuffer.Clear();
            _enemyController.DecideTurnActions(Context, _commandBuffer);
            ExecuteCommands(_commandBuffer);
        }

        /// <summary>
        /// AI 턴을 실행하면서, 실행된 커맨드들의 Trace를 수집합니다.
        /// UI에서 연출을 재생하고 싶을 때 사용합니다.
        /// </summary>
        public void ExecuteEnemyTurnWithTrace(List<TcgBattleCommandTrace> traces)
        {
            if (IsBattleEnded) return;
            if (IsPlayerTurn) return;

            _commandBuffer.Clear();
            _enemyController.DecideTurnActions(Context, _commandBuffer);
            ExecuteCommandsWithTrace(_commandBuffer, traces);
        }

        /// <summary>
        /// 턴 종료 처리: ActiveSide 변경, 다음 턴 준비 등.
        /// </summary>
        public void EndTurn()
        {
            if (IsBattleEnded)
                return;

            // 1) End-of-Turn 효과 처리
            ResolveEndOfTurnEffects();

            // 2) 턴 수 증가 (플레이어 턴, 적 턴은 매 턴 증가)
            Context.TurnCount++;

            // 3) ActiveSide 전환 (플레이어 ↔ Enemy)
            Context.ActiveSide =
                (Context.ActiveSide == ConfigCommonTcg.TcgPlayerSide.Player)
                    ? ConfigCommonTcg.TcgPlayerSide.Enemy
                    : ConfigCommonTcg.TcgPlayerSide.Player;

            // 4) Start-of-Turn 초기 처리
            IncreaseMaxManaByTurnOff();
            DrawStartOfTurnCard();

            // 5) Start-of-Turn 효과 처리
            ResolveStartOfTurnEffects();

            // 6) 전투 종료 여부 검사
            TryCheckBattleEnd();
        }
        private void ResolveEndOfTurnEffects()
        {
            // TODO: 
            //  - 턴 종료 후 죽는 크리처(독, 출혈 등)
            //  - 지속 카드 효과
            //  - “턴 종료 시 체력 회복”, “턴 종료 시 피해” 등
        }

        private void ResolveStartOfTurnEffects()
        {
            // TODO:
            //  - “턴 시작 시 공격력 증가”
            //  - “턴 시작 시 비용 감소”
            //  - 버프/디버프 갱신
            //  - 지속 주문 효과 Tick
        }

        private void DrawStartOfTurnCard()
        {
            var side = Context.GetSideState(Context.ActiveSide);

            // 덱이 비었을 때 처리는 자유 설계 (피해 주기, 패티그 등)
            side.DrawOneCard();
        }
        /// <summary>
        /// 적 턴 종료 시, 총 마나 올려주고, 마나 리셋 하기 
        /// </summary>
        private void IncreaseMaxManaByTurnOff()
        {
            if (Context.ActiveSide != ConfigCommonTcg.TcgPlayerSide.Player) return;
            var playerSide = Context.GetSideState(ConfigCommonTcg.TcgPlayerSide.Player);
            playerSide.IncreaseMaxMana(_tcgSettings.countManaAfterTurn, _tcgSettings.countMaxManaInBattle);
            playerSide.RestoreManaFull();
            var enemySide = Context.GetSideState(ConfigCommonTcg.TcgPlayerSide.Enemy);
            enemySide.IncreaseMaxMana(_tcgSettings.countManaAfterTurn, _tcgSettings.countMaxManaInBattle);
            enemySide.RestoreManaFull();
        }

        /// <summary>
        /// 강제로 전투를 종료시키고 승패를 지정합니다.
        /// </summary>
        public void ForceEnd(ConfigCommonTcg.TcgPlayerSide winner)
        {
            IsBattleEnded = true;
            Winner        = winner;
        }

        /// <summary>
        /// HP 등 상태를 확인하여 전투 종료 여부를 판정합니다.
        /// </summary>
        private void TryCheckBattleEnd()
        {
            if (IsBattleEnded) return;

            var playerHp = Context.Player.HeroHp;
            var enemyHp  = Context.Enemy.HeroHp;

            if (playerHp <= 0 && enemyHp <= 0)
            {
                IsBattleEnded = true;
                Winner = ConfigCommonTcg.TcgPlayerSide.None;
            }
            else if (playerHp <= 0)
            {
                IsBattleEnded = true;
                Winner = ConfigCommonTcg.TcgPlayerSide.Enemy;
            }
            else if (enemyHp <= 0)
            {
                IsBattleEnded = true;
                Winner = ConfigCommonTcg.TcgPlayerSide.Player;
            }
        }

        public void Dispose()
        {
            _playerController?.Dispose();
            _enemyController?.Dispose();
            Context.ClearOwner();
        }
        
        #region Command Execution
        /// <summary>
        /// 외부(UI 등)에서 개별 전투 커맨드를 전달하여 실행할 때 사용합니다.
        /// (FollowUpCommands 까지 포함해서 한 번에 처리됩니다.)
        /// </summary>
        public void ExecuteCommand(in TcgBattleCommand command)
        {
            if (IsBattleEnded) return;

            _executionQueue.Clear();
            _executionQueue.Add(command);

            ProcessExecutionQueue();
        }

        /// <summary>
        /// 외부(UI 등)에서 개별 전투 커맨드를 전달하여 실행하며,
        /// 실행된 커맨드들의 Trace를 수집합니다.
        /// </summary>
        public void ExecuteCommandWithTrace(in TcgBattleCommand command, List<TcgBattleCommandTrace> traces)
        {
            if (IsBattleEnded) return;
            traces?.Clear();

            _executionQueue.Clear();
            _executionQueue.Add(command);

            ProcessExecutionQueue(traces);
        }


        /// <summary>
        /// 여러 개의 커맨드를 순차적으로 실행합니다.
        /// CommandResult.FollowUpCommands 까지 포함해 모두 처리합니다.
        /// </summary>
        public void ExecuteCommands(List<TcgBattleCommand> commands)
        {
            if (IsBattleEnded) return;
            if (commands == null || commands.Count == 0) return;

            _executionQueue.Clear();
            _executionQueue.AddRange(commands);

            ProcessExecutionQueue();
        }

        /// <summary>
        /// 여러 개의 커맨드를 순차적으로 실행하며,
        /// 실행된 커맨드들의 Trace를 수집합니다.
        /// </summary>
        public void ExecuteCommandsWithTrace(List<TcgBattleCommand> commands, List<TcgBattleCommandTrace> traces)
        {
            if (IsBattleEnded) return;
            if (commands == null || commands.Count == 0) return;
            traces?.Clear();

            _executionQueue.Clear();
            _executionQueue.AddRange(commands);

            ProcessExecutionQueue(traces);
        }


        /// <summary>
        /// _executionQueue 에 들어있는 커맨드를 CommandResult 기반으로 모두 처리합니다.
        /// </summary>
        private void ProcessExecutionQueue()
        {
            while (_executionQueue.Count > 0 && !IsBattleEnded)
            {
                var cmd = _executionQueue[0];
                _executionQueue.RemoveAt(0);

                if (!_commandHandlers.TryGetValue(cmd.CommandType, out var handler))
                {
                    GcLogger.LogError(
                        $"[{nameof(TcgBattleSession)}] 핸들러가 등록되지 않은 커맨드 타입: {cmd.CommandType}");
                    continue;
                }

                var result = handler.Execute(Context, in cmd);
                HandleCommandResult(result);
            }
        }

        /// <summary>
        /// _executionQueue 에 들어있는 커맨드를 처리하면서 Trace를 수집합니다.
        /// </summary>
        private void ProcessExecutionQueue(List<TcgBattleCommandTrace> traces)
        {
            while (_executionQueue.Count > 0 && !IsBattleEnded)
            {
                var cmd = _executionQueue[0];
                _executionQueue.RemoveAt(0);

                if (!_commandHandlers.TryGetValue(cmd.CommandType, out var handler))
                {
                    GcLogger.LogError(
                        $"[{nameof(TcgBattleSession)}] 핸들러가 등록되지 않은 커맨드 타입: {cmd.CommandType}");
                    continue;
                }

                var result = handler.Execute(Context, in cmd);
                traces?.Add(new TcgBattleCommandTrace(in cmd, result));

                HandleCommandResult(result);
            }
        }

        /// <summary>
        /// CommandResult 를 해석해서 메시지/후속 커맨드/전투 종료를 처리합니다.
        /// </summary>
        private void HandleCommandResult(CommandResult result)
        {
            if (result == null)
                return;

            if (!result.Success)
            {
                if (!string.IsNullOrEmpty(result.MessageKey))
                {
                    _systemMessageManager?.ShowMessageError(result.MessageKey);
                }

                return;
            }

            // 성공 + 후속 커맨드 처리
            if (result.HasFollowUps)
            {
                _executionQueue.AddRange(result.FollowUpCommands);
            }

            // 전투 종료 여부 확인
            TryCheckBattleEnd();
        }
        #endregion
    }
}
