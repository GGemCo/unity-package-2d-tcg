using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 전투 커맨드 핸들러 실행 결과.
    /// - Success: 성공 여부
    /// - MessageKey / Message: 실패 또는 안내 메시지
    /// - FollowUpCommands: 트리거/연쇄로 인해 발생하는 추가 커맨드 목록
    /// - PresentationSteps: UI 연출(애니메이션/이펙트)용 단계 정보
    ///
    /// 설계 목표:
    /// 1) 핸들러는 UI/MonoBehaviour에 의존하지 않는다.
    /// 2) 세션/매니저 계층에서 메시지/연출을 처리한다.
    /// 3) 복수의 후속 커맨드/연출 단계 반환을 지원한다.
    /// </summary>
    public sealed class CommandResult
    {
        /// <summary>커맨드가 정상적으로 처리되었는지 여부.</summary>
        public bool Success { get; }

        /// <summary>
        /// 메시지 키(로컬라이징용). 실패 시 또는 안내 시 사용.
        /// </summary>
        public string MessageKey { get; }

        /// <summary>
        /// 커스텀 SystemMessage 객체가 필요한 경우 사용. 메시지 키보다 우선한다.
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

        /// <summary>
        /// UI 연출(애니메이션/이펙트) 단계 목록.
        /// 도메인은 "무슨 일이 일어났는지"만 기술하고,
        /// 실제 연출은 UI 레이어(TcgBattleUiController)에서 재생한다.
        /// </summary>
        public IReadOnlyList<TcgPresentationStep> PresentationSteps => _presentationSteps;
        private readonly List<TcgPresentationStep> _presentationSteps;

        /// <summary>연출 단계가 있는지 여부.</summary>
        public bool HasPresentation => _presentationSteps != null && _presentationSteps.Count > 0;

        // ------------------------------
        // 정적 팩토리 메서드
        // ------------------------------

        /// <summary>성공 결과 (추가 커맨드/연출 없음)</summary>
        public static CommandResult Ok() =>
            new CommandResult(true, null, null, null, null);

        /// <summary>성공 결과 + 후속 커맨드</summary>
        public static CommandResult Ok(params TcgBattleCommand[] followUps) =>
            new CommandResult(true, null, null, followUps, null);

        /// <summary>성공 결과 + 연출 단계(후속 커맨드 없음)</summary>
        public static CommandResult OkPresentation(params TcgPresentationStep[] presentationSteps) =>
            new CommandResult(true, null, null, null, presentationSteps);

        /// <summary>성공 결과 + 연출 단계 + 후속 커맨드</summary>
        public static CommandResult OkPresentation(IEnumerable<TcgPresentationStep> presentationSteps,
            params TcgBattleCommand[] followUps) =>
            new CommandResult(true, null, null, followUps, presentationSteps);

        /// <summary>문자 기반 메시지를 포함한 실패 결과 (로컬라이징 키)</summary>
        public static CommandResult Fail(string messageKey) =>
            new CommandResult(false, messageKey, null, null, null);

        /// <summary>SystemMessage 기반 실패 결과</summary>
        public static CommandResult Fail(SystemMessage message) =>
            new CommandResult(false, null, message, null, null);

        // ------------------------------
        // 생성자 (외부에서 직접 호출 X)
        // ------------------------------

        private CommandResult(
            bool success,
            string messageKey,
            SystemMessage message,
            IEnumerable<TcgBattleCommand> followUps,
            IEnumerable<TcgPresentationStep> presentationSteps)
        {
            Success = success;
            MessageKey = messageKey;
            Message = message;

            if (followUps != null)
                _followUpCommands = new List<TcgBattleCommand>(followUps);

            if (presentationSteps != null)
                _presentationSteps = new List<TcgPresentationStep>(presentationSteps);
        }
    }
}
