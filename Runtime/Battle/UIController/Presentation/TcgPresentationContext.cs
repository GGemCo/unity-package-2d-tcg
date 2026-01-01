using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
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
        
        public GGemCoTcgSettings Settings { get; }

        /// <summary>
        /// 연출 실행에 필요한 세션 및 UI 참조를 구성합니다.
        /// </summary>
        /// <param name="session">현재 전투 세션.</param>
        /// <param name="fieldEnemy">적 필드 윈도우.</param>
        /// <param name="fieldPlayer">플레이어 필드 윈도우.</param>
        /// <param name="handPlayer">플레이어 손패 윈도우.</param>
        /// <param name="handEnemy">적 손패 윈도우.</param>
        /// <param name="battleHud">전투 HUD 윈도우.</param>
        /// <param name="settings">TCG 설정.</param>
        public TcgPresentationContext(
            TcgBattleSession session,
            UIWindowTcgFieldEnemy fieldEnemy,
            UIWindowTcgFieldPlayer fieldPlayer,
            UIWindowTcgHandPlayer handPlayer,
            UIWindowTcgHandEnemy handEnemy,
            UIWindowTcgBattleHud battleHud,
            GGemCoTcgSettings settings)
        {
            Session = session;
            FieldEnemy = fieldEnemy;
            FieldPlayer = fieldPlayer;
            HandPlayer = handPlayer;
            HandEnemy = handEnemy;
            BattleHud = battleHud;
            Settings = settings;
        }

        /// <summary>
        /// 연출 중 아이콘을 최상단에 올리기 위해 사용하는 UI 루트 트랜스폼.
        /// </summary>
        public Transform UIRoot => SceneGame.Instance.canvasUI.transform;

        public UIWindow GetUIWindow(ConfigCommonTcg.TcgZone zone)
        {
            if (zone == ConfigCommonTcg.TcgZone.HandPlayer)
                return HandPlayer;
            if (zone == ConfigCommonTcg.TcgZone.FieldPlayer)
                return FieldPlayer;
            if (zone == ConfigCommonTcg.TcgZone.HandEnemy)
                return HandEnemy;
            if (zone == ConfigCommonTcg.TcgZone.FieldEnemy)
                return FieldEnemy;
            GcLogger.LogError($"{nameof(UIWindow)}가 없습니다. zone: {zone}");
            return null;
        }
    }
}
