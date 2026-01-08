using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GGemCo2DCore;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GGemCo2DTcg
{
    /// <summary>
    /// Addressables에서 TCG 관련 Settings(ScriptableObject)을 로드하여 보관하고, 로드 완료 이벤트를 알리는 로더입니다.
    /// </summary>
    /// <remarks>
    /// - 싱글톤(MonoBehaviour)로 유지되며, 씬 전환 시 파괴되지 않습니다.
    /// - 로드 완료 후 <see cref="OnLoadSettings"/> 이벤트로 결과를 전달합니다.
    /// </remarks>
    public class AddressableLoaderSettingsTcg : MonoBehaviour
    {
        /// <summary>
        /// 현재 씬에 존재하는 로더 인스턴스를 반환합니다.
        /// </summary>
        public static AddressableLoaderSettingsTcg Instance { get; private set; }

        /// <summary>
        /// Addressables에서 로드된 TCG 공용 설정입니다.
        /// </summary>
        [HideInInspector] public GGemCoTcgSettings tcgSettings;

        /// <summary>
        /// Addressables에서 로드된 UI 컷씬 관련 설정입니다.
        /// </summary>
        [HideInInspector] public GGemCoTcgUICutsceneSettings uiCutsceneSettings;

        /// <summary>
        /// 설정 로드 완료 시 호출되는 델리게이트 타입입니다.
        /// </summary>
        /// <param name="tcgSettings">로드된 TCG 설정입니다.</param>
        /// <param name="uiCutsceneSettings">로드된 UI 컷씬 설정입니다.</param>
        public delegate void DelegateLoadSettings(GGemCoTcgSettings tcgSettings, GGemCoTcgUICutsceneSettings uiCutsceneSettings);

        /// <summary>
        /// 모든 설정 로드가 완료되었을 때 구독자에게 로드 결과를 전달합니다.
        /// </summary>
        public event DelegateLoadSettings OnLoadSettings;

        /// <summary>
        /// (현재 구현 기준) 로드 과정에서 생성된 Addressables 핸들을 추적하기 위한 컬렉션입니다.
        /// </summary>
        private readonly HashSet<AsyncOperationHandle> _activeHandles = new HashSet<AsyncOperationHandle>();

        /// <summary>
        /// 설정 로딩 진행률(0~1)입니다.
        /// </summary>
        /// <remarks>
        /// NOTE: 현재 코드에서는 진행률을 갱신하는 로직이 없어 항상 초기값(0)일 수 있습니다.
        /// </remarks>
        private float _loadProgress;

        /// <summary>
        /// 싱글톤을 초기화하고 파괴되지 않도록 설정합니다.
        /// </summary>
        private void Awake()
        {
            _loadProgress = 0f;

            if (!Instance)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 오브젝트 파괴 시점에 로드된 Addressables 리소스를 해제합니다.
        /// </summary>
        private void OnDestroy()
        {
            ReleaseAll();
        }

        /// <summary>
        /// 추적 중인 모든 Addressables 핸들을 해제합니다.
        /// </summary>
        private void ReleaseAll()
        {
            AddressableLoaderController.ReleaseByHandles(_activeHandles);
        }

        /// <summary>
        /// Addressables에서 모든 설정 파일을 병렬로 로드하고, 로드 완료 이벤트를 발생시킵니다.
        /// </summary>
        /// <returns>비동기 로딩 작업을 나타내는 <see cref="Task"/>입니다.</returns>
        /// <exception cref="Exception">Addressables 로딩 도중 예외가 발생할 수 있으며, try/catch로 로깅 처리합니다.</exception>
        public async Task LoadAllSettingsAsync()
        {
            try
            {
                // 여러 개의 설정을 병렬적으로 로드
                var taskTcgSettings = LoadSettingsAsync<GGemCoTcgSettings>(ConfigAddressableSettingTcg.TcgSettings.Key);
                var taskTcgUICutsceneSettings = LoadSettingsAsync<GGemCoTcgUICutsceneSettings>(ConfigAddressableSettingTcg.TcgUICutsceneSettings.Key);

                // 모든 작업이 완료될 때까지 대기
                await Task.WhenAll(taskTcgSettings, taskTcgUICutsceneSettings);

                // 결과 저장
                tcgSettings = taskTcgSettings.Result;
                uiCutsceneSettings = taskTcgUICutsceneSettings.Result;

                // 이벤트 호출
                OnLoadSettings?.Invoke(tcgSettings, uiCutsceneSettings);
            }
            catch (Exception ex)
            {
                GcLogger.LogError($"설정 로딩 중 오류 발생: {ex.Message}");
            }
        }

        /// <summary>
        /// Addressables 키로 설정(ScriptableObject)을 로드하여 반환합니다.
        /// </summary>
        /// <typeparam name="T">로드할 설정 타입(ScriptableObject 파생)입니다.</typeparam>
        /// <param name="key">Addressables에 등록된 설정 에셋의 키입니다.</param>
        /// <returns>로드된 설정 에셋을 반환하며, 실패 시 <c>null</c>을 반환합니다.</returns>
        /// <remarks>
        /// 1) 키에 대한 ResourceLocation 존재 여부를 먼저 확인한 뒤,
        /// 2) 실제 에셋을 로드합니다.
        /// </remarks>
        private async Task<T> LoadSettingsAsync<T>(string key) where T : ScriptableObject
        {
            // 키가 Addressables에 등록되어 있는지 확인
            var locationsHandle = Addressables.LoadResourceLocationsAsync(key);
            await locationsHandle.Task;

            if (!locationsHandle.Status.Equals(AsyncOperationStatus.Succeeded) || locationsHandle.Result.Count == 0)
            {
                GcLogger.LogError(
                    $"[AddressableSettingsLoader] '{key}' 가 Addressables에 등록되지 않았습니다. '{key}' 를 생성한 후 {ConfigDefine.NameSDK}Tool > 기본 셋팅하기 메뉴를 열고 Addressable 추가하기 버튼을 클릭해주세요.");

                Addressables.Release(locationsHandle);
                return null;
            }

            // 설정 로드
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(key);
            T asset = await handle.Task;

            // 핸들 해제(Locations)
            Addressables.Release(locationsHandle);

            return asset;
        }

        /// <summary>
        /// 현재 설정 로딩 진행률을 반환합니다.
        /// </summary>
        /// <returns>0~1 범위의 진행률 값입니다.</returns>
        public float GetLoadProgress() => _loadProgress;
    }
}
