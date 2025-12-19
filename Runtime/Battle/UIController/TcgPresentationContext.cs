using System.Collections;
using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 프레젠테이션(연출) 스텝을 실행하는 핸들러 인터페이스.
    /// </summary>
    public interface ITcgPresentationHandler
    {
        /// <summary>
        /// 이 핸들러가 처리할 스텝 타입.
        /// </summary>
        TcgPresentationStepType Type { get; }

        /// <summary>
        /// 주어진 스텝에 대한 연출 코루틴을 실행합니다.
        /// </summary>
        /// <param name="ctx">연출 실행에 필요한 UI/세션 컨텍스트.</param>
        /// <param name="step">실행할 프레젠테이션 스텝.</param>
        /// <returns>코루틴 이터레이터.</returns>
        IEnumerator Play(TcgPresentationContext ctx, TcgPresentationStep step);
    }

    /// <summary>
    /// 연출 핸들러들이 공통으로 참조하는 전투 세션 및 UI 윈도우 묶음 컨텍스트.
    /// </summary>
    public sealed class TcgPresentationContext
    {
        /// <summary>현재 전투 세션.</summary>
        public TcgBattleSession Session { get; }

        /// <summary>적 필드 윈도우.</summary>
        public UIWindowTcgFieldEnemy FieldEnemy { get; }

        /// <summary>플레이어 필드 윈도우.</summary>
        public UIWindowTcgFieldPlayer FieldPlayer { get; }

        /// <summary>플레이어 손패 윈도우.</summary>
        public UIWindowTcgHandPlayer HandPlayer { get; }

        /// <summary>적 손패 윈도우.</summary>
        public UIWindowTcgHandEnemy HandEnemy { get; }

        /// <summary>전투 HUD 윈도우(코루틴 호스트로도 사용).</summary>
        public UIWindowTcgBattleHud BattleHud { get; }

        /// <summary>
        /// 연출 실행에 필요한 세션 및 UI 참조를 구성합니다.
        /// </summary>
        /// <param name="session">현재 전투 세션.</param>
        /// <param name="fieldEnemy">적 필드 윈도우.</param>
        /// <param name="fieldPlayer">플레이어 필드 윈도우.</param>
        /// <param name="handPlayer">플레이어 손패 윈도우.</param>
        /// <param name="handEnemy">적 손패 윈도우.</param>
        /// <param name="battleHud">전투 HUD 윈도우.</param>
        public TcgPresentationContext(
            TcgBattleSession session,
            UIWindowTcgFieldEnemy fieldEnemy,
            UIWindowTcgFieldPlayer fieldPlayer,
            UIWindowTcgHandPlayer handPlayer,
            UIWindowTcgHandEnemy handEnemy,
            UIWindowTcgBattleHud battleHud)
        {
            Session = session;
            FieldEnemy = fieldEnemy;
            FieldPlayer = fieldPlayer;
            HandPlayer = handPlayer;
            HandEnemy = handEnemy;
            BattleHud = battleHud;
        }

        /// <summary>
        /// 진영에 맞는 손패 윈도우를 반환합니다.
        /// </summary>
        /// <param name="side">플레이어/적 진영.</param>
        public UIWindowTcgHandBase GetHandWindow(ConfigCommonTcg.TcgPlayerSide side)
            => side == ConfigCommonTcg.TcgPlayerSide.Player ? HandPlayer : HandEnemy;

        /// <summary>
        /// 진영에 맞는 필드 윈도우를 반환합니다.
        /// </summary>
        /// <param name="side">플레이어/적 진영.</param>
        public UIWindowTcgFieldBase GetFieldWindow(ConfigCommonTcg.TcgPlayerSide side)
            => side == ConfigCommonTcg.TcgPlayerSide.Player ? FieldPlayer : FieldEnemy;

        /// <summary>
        /// 연출 중 아이콘을 최상단에 올리기 위해 사용하는 UI 루트 트랜스폼.
        /// </summary>
        public Transform UIRoot => SceneGame.Instance.canvasUI.transform;

        /// <summary>
        /// 매 스텝 종료 시 전투 종료 여부를 확인하고,
        /// 종료되었다면 진행 중인 연출 코루틴을 중지/정리합니다.
        /// </summary>
        /// <param name="coroutineHost">코루틴을 실행/중지할 호스트.</param>
        /// <param name="running">현재 실행 중인 코루틴 참조(중지 시 null로 설정).</param>
        public void CheckBattleEndAndStop(MonoBehaviour coroutineHost, ref Coroutine running)
        {
            Session.TryCheckBattleEnd();
            if (!Session.IsBattleEnded) return;

            if (running != null)
            {
                coroutineHost.StopCoroutine(running);
                running = null;
            }
        }
    }
}
