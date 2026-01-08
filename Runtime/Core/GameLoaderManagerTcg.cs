using UnityEngine;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 게임 시작 시 TCG 패키지에서 필요한 리소스(설정 등) 로딩 스텝을
    /// <see cref="GameLoaderManager"/>에 등록하는 브리지 컴포넌트입니다.
    /// </summary>
    public class GameLoaderManagerTcg : MonoBehaviour
    {
        /*
         * Unity 초기화 이벤트 개요(참고):
         * 1) SubsystemRegistration, AfterAssembliesLoaded: 로우 레벨/어셈블리 초기화
         * 2) BeforeSplashScreen: 입력 시스템 등 초기화
         * 3) BeforeSceneLoad: 첫 씬 로드 이전, Awake/OnEnable 호출
         * 4) AfterSceneLoad: Start 호출
         */

        /// <summary>
        /// 컴포넌트가 활성화될 때 로더 이벤트를 구독하여,
        /// 로딩 시작 직전에 TCG 로딩 스텝을 등록할 수 있도록 준비합니다.
        /// </summary>
        private void OnEnable()
        {
            // Pre-Intro(혹은 초기 진입) 씬에서 로딩이 시작되기 직전에 호출되는 이벤트에 연결합니다.
            GameLoaderManager.BeforeLoadStart += OnBeforeLoadStart;
        }

        /// <summary>
        /// 컴포넌트가 비활성화될 때 이벤트 구독을 해제합니다.
        /// 씬 전환/오브젝트 파괴 시 중복 구독으로 인한 다중 등록을 방지합니다.
        /// </summary>
        private void OnDisable()
        {
            GameLoaderManager.BeforeLoadStart -= OnBeforeLoadStart;
        }

        /// <summary>
        /// 로딩 시작 직전에 호출되며, TCG에서 필요한 Addressables 로딩 스텝을 등록합니다.
        /// </summary>
        /// <param name="sender">로딩 스텝을 등록할 대상 <see cref="GameLoaderManager"/>입니다.</param>
        /// <param name="e">로딩 시작 이벤트 인자입니다(현재 메서드에서는 사용하지 않음).</param>
        private void OnBeforeLoadStart(GameLoaderManager sender, GameLoaderManager.EventArgsBeforeLoadStart e)
        {
            // 설정 ScriptableObject(Addressables)들을 로딩하는 로더 인스턴스를 확보합니다.
            // - 씬에 이미 존재하면 재사용
            // - 없다면 런타임에 생성하여 로딩을 수행
            //
            // NOTE: FindFirstObjectByType는 씬에 로더가 없을 때만 1회 생성되는 것을 기대합니다.
            //       동일 씬에 본 컴포넌트가 여러 개 있거나 이벤트가 중복 구독되면 스텝이 중복 등록될 수 있으니 주의하세요.
            var addrSettings =
                Object.FindFirstObjectByType<AddressableLoaderSettingsTcg>() ??
                new GameObject("AddressableLoaderSettingsTcg")
                    .AddComponent<AddressableLoaderSettingsTcg>();

            // 로딩 UI에 "설정" 단계로 표시될 Addressables 로딩 스텝을 등록합니다.
            // order는 전체 로딩 파이프라인 내 실행 우선순위(낮을수록 먼저)로 사용됩니다(정책은 GameLoaderManager 구현에 따름).
            var step = new AddressableTaskStep(
                id: "tcg.settings",
                order: 240,
                localizedKey: LocalizationConstants.Keys.Loading.TextTypeSettings(),
                startTask: () => addrSettings.LoadAllSettingsAsync(),
                getProgress: () => addrSettings.GetLoadProgress()
            );

            sender.Register(step);
        }
    }
}
