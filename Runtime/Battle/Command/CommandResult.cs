using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 전투 커맨드 핸들러 실행 결과를 표현하는 클래스.
    /// - Success: 성공 여부
    /// - MessageKey / Message: 실패 또는 안내 메시지
    /// - FollowUpCommands: 트리거로 인해 발생하는 추가 커맨드 목록
    /// 
    /// 디자인 목표:
    /// 1) 핸들러는 UI/메시지 시스템에 의존하지 않는다.
    /// 2) 세션(TcgBattleSession) 또는 Manager 계층에서 메시지를 처리한다.
    /// 3) 복수의 후속 커맨드를 반환하여 Ability/Trigger 연쇄를 지원한다.
    /// </summary>
    public sealed class CommandResult
    {
        /// <summary>커맨드가 정상적으로 처리되었는지 여부.</summary>
        public bool Success { get; }

        /// <summary>
        /// 메시지 키(로컬라이징용).
        /// 실패 시 또는 안내 시 사용.
        /// </summary>
        public string MessageKey { get; }

        /// <summary>
        /// 커스텀 SystemMessage 객체가 필요한 경우 사용.
        /// 메시지 키보다 우선한다.
        /// </summary>
        public SystemMessage Message { get; }

        /// <summary>
        /// 커맨드 처리 이후 자동으로 실행해야 하는 후속 커맨드 목록.
        /// (소환 트리거, 죽음 트리거, OnDamage 등)
        /// </summary>
        public IReadOnlyList<TcgBattleCommand> FollowUpCommands => _followUpCommands;
        private readonly List<TcgBattleCommand> _followUpCommands;

        /// <summary>후속 커맨드가 있는지 여부.</summary>
        public bool HasFollowUps => _followUpCommands != null && _followUpCommands.Count > 0;


        // ------------------------------
        // 정적 팩토리 메서드
        // ------------------------------

        /// <summary>성공 결과 (추가 커맨드 없음)</summary>
        public static CommandResult Ok() =>
            new CommandResult(true, null, null, null);

        /// <summary>성공 결과 + 후속 커맨드</summary>
        public static CommandResult Ok(params TcgBattleCommand[] followUps) =>
            new CommandResult(true, null, null, followUps);

        /// <summary>문자 기반 메시지를 포함한 실패 결과 (로컬라이징 키)</summary>
        public static CommandResult Fail(string messageKey) =>
            new CommandResult(false, messageKey, null, null);

        /// <summary>SystemMessage 기반 실패 결과</summary>
        public static CommandResult Fail(SystemMessage message) =>
            new CommandResult(false, null, message, null);


        // ------------------------------
        // 생성자 (외부에서 직접 호출 X)
        // ------------------------------
        private CommandResult(
            bool success,
            string messageKey,
            SystemMessage message,
            IEnumerable<TcgBattleCommand> followUps)
        {
            Success     = success;
            MessageKey  = messageKey;
            Message     = message;

            if (followUps != null)
                _followUpCommands = new List<TcgBattleCommand>(followUps);
        }
    }
}
