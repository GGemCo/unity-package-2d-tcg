using GGemCo2DCore;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 전투 연출(Presentation) 핸들러들이 공통으로 사용하는 전투 세션 및 UI 윈도우 참조 컨텍스트입니다.
    /// </summary>
    /// <remarks>
    /// 연출 시스템은 본 컨텍스트를 통해 전투 상태(<see cref="TcgBattleSession"/>)와
    /// 각 UI 윈도우(필드/손패/HUD), 설정 값을 일관되게 접근합니다.
    /// </remarks>
    public sealed class TcgPresentationContext
    {
        /// <summary>
        /// 현재 전투 세션입니다.
        /// </summary>
        public TcgBattleSession Session { get; }

        /// <summary>
        /// 적 필드(보드) UI 윈도우입니다.
        /// </summary>
        public UIWindowTcgFieldEnemy FieldEnemy { get; }

        /// <summary>
        /// 플레이어 필드(보드) UI 윈도우입니다.
        /// </summary>
        public UIWindowTcgFieldPlayer FieldPlayer { get; }

        /// <summary>
        /// 플레이어 손패 UI 윈도우입니다.
        /// </summary>
        public UIWindowTcgHandPlayer HandPlayer { get; }

        /// <summary>
        /// 적 손패 UI 윈도우입니다.
        /// </summary>
        public UIWindowTcgHandEnemy HandEnemy { get; }

        /// <summary>
        /// 전투 HUD UI 윈도우입니다(연출 코루틴 호스트로도 사용될 수 있습니다).
        /// </summary>
        public UIWindowTcgBattleHud BattleHud { get; }

        /// <summary>
        /// TCG 전반 설정(밸런스/연출 공통 값 등)입니다.
        /// </summary>
        public GGemCoTcgSettings Settings { get; }

        /// <summary>
        /// UI 컷씬/연출 관련 설정입니다.
        /// </summary>
        public GGemCoTcgUICutsceneSettings UICutsceneSettings { get; }

        /// <summary>
        /// 연출 실행에 필요한 세션 및 UI 참조, 설정을 묶어 컨텍스트를 구성합니다.
        /// </summary>
        /// <param name="session">현재 전투 세션입니다.</param>
        /// <param name="fieldEnemy">적 필드 UI 윈도우입니다.</param>
        /// <param name="fieldPlayer">플레이어 필드 UI 윈도우입니다.</param>
        /// <param name="handPlayer">플레이어 손패 UI 윈도우입니다.</param>
        /// <param name="handEnemy">적 손패 UI 윈도우입니다.</param>
        /// <param name="battleHud">전투 HUD UI 윈도우입니다.</param>
        /// <param name="settings">TCG 전반 설정입니다.</param>
        /// <param name="uiCutsceneSettings">UI 컷씬/연출 설정입니다.</param>
        /// <exception cref="System.ArgumentNullException">
        /// TODO: 호출 계약에 따라 필수 의존성이 null일 수 없다면, null 체크 후 예외를 던지도록 보강할 수 있습니다.
        /// </exception>
        public TcgPresentationContext(
            TcgBattleSession session,
            UIWindowTcgFieldEnemy fieldEnemy,
            UIWindowTcgFieldPlayer fieldPlayer,
            UIWindowTcgHandPlayer handPlayer,
            UIWindowTcgHandEnemy handEnemy,
            UIWindowTcgBattleHud battleHud,
            GGemCoTcgSettings settings,
            GGemCoTcgUICutsceneSettings uiCutsceneSettings)
        {
            Session = session;
            FieldEnemy = fieldEnemy;
            FieldPlayer = fieldPlayer;
            HandPlayer = handPlayer;
            HandEnemy = handEnemy;
            BattleHud = battleHud;
            Settings = settings;
            UICutsceneSettings = uiCutsceneSettings;
        }

        /// <summary>
        /// 연출 중 생성되는 아이콘/이펙트를 최상단에 올리기 위해 사용하는 UI 루트 트랜스폼입니다.
        /// </summary>
        /// <remarks>
        /// 현재 씬의 UI 캔버스 루트(<c>SceneGame.Instance.canvasUI</c>)를 반환합니다.
        /// </remarks>
        public Transform UIRoot => SceneGame.Instance.canvasUI.transform;

        /// <summary>
        /// 지정한 존(<paramref name="zone"/>)에 대응하는 UI 윈도우를 반환합니다.
        /// </summary>
        /// <param name="zone">대상 존(손패/필드 등)입니다.</param>
        /// <returns>존에 매핑된 <see cref="UIWindow"/>를 반환하며, 매핑이 없으면 null입니다.</returns>
        /// <remarks>
        /// 매핑되지 않은 존이 들어오면 에러 로그를 남기고 null을 반환합니다.
        /// 호출 측에서 null 처리가 필요합니다.
        /// </remarks>
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
