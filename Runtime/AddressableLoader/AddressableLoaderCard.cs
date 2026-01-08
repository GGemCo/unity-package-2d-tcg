using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GGemCo2DTcg;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GGemCo2DCore
{
    /// <summary>
    /// Addressables 라벨을 기반으로 카드 관련 스프라이트(일러스트/테두리)를 비동기로 로드하고 캐싱하는 로더입니다.
    /// </summary>
    /// <remarks>
    /// - 싱글톤(MonoBehaviour) 형태로 유지되며, 로드된 핸들을 추적해 일괄 해제합니다.
    /// - 로딩 진행률은 라벨 단위(일러스트/테두리) 로딩 과정에서 갱신됩니다.
    /// </remarks>
    public class AddressableLoaderCard : MonoBehaviour
    {
        /// <summary>
        /// 현재 씬에 존재하는 로더 인스턴스를 반환합니다.
        /// </summary>
        public static AddressableLoaderCard Instance { get; private set; }

        /// <summary>
        /// 카드 일러스트 스프라이트 캐시(키: Addressables PrimaryKey).
        /// </summary>
        private readonly Dictionary<string, Sprite> _dicImageArt = new Dictionary<string, Sprite>();

        /// <summary>
        /// 카드 테두리 스프라이트 캐시(키: Addressables PrimaryKey).
        /// </summary>
        private readonly Dictionary<string, Sprite> _dicImageBorder = new Dictionary<string, Sprite>();

        /// <summary>
        /// 로드/조회에 사용된 Addressables 핸들을 추적하여 해제 누락을 방지합니다.
        /// </summary>
        private readonly HashSet<AsyncOperationHandle> _activeHandles = new HashSet<AsyncOperationHandle>();

        /// <summary>
        /// 프리팹(스프라이트) 로딩 진행률(0~1).
        /// </summary>
        private float _prefabLoadProgress;

        /// <summary>
        /// 싱글톤을 초기화하고 파괴되지 않도록 설정합니다.
        /// </summary>
        /// <remarks>
        /// 동일 타입 오브젝트가 중복 생성되면 이후 생성된 오브젝트는 파괴됩니다.
        /// </remarks>
        private void Awake()
        {
            _prefabLoadProgress = 0f;

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
        /// 오브젝트 파괴 시점에 로드된 Addressables 리소스를 모두 해제합니다.
        /// </summary>
        private void OnDestroy()
        {
            ReleaseAll();
        }

        /// <summary>
        /// 지금까지 추적된 모든 Addressables 핸들을 해제합니다.
        /// </summary>
        /// <remarks>
        /// 핸들 누적이 계속되면 메모리 점유가 증가할 수 있으므로, 로더 생명주기 종료 시 호출됩니다.
        /// </remarks>
        private void ReleaseAll()
        {
            AddressableLoaderController.ReleaseByHandles(_activeHandles);
        }

        /// <summary>
        /// 카드 관련 스프라이트(일러스트/테두리)를 Addressables 라벨로 모두 로드하여 캐시에 저장합니다.
        /// </summary>
        /// <remarks>
        /// - 로딩 진행률은 각 라벨의 로딩 루프에서 (로드 완료 개수 + 현재 항목 진행률) / 전체 개수로 계산됩니다.
        /// - 로딩 실패 시 로그를 남기고 조기 반환합니다.
        /// </remarks>
        /// <returns>비동기 로딩 작업을 나타내는 <see cref="Task"/>입니다.</returns>
        /// <exception cref="Exception">Addressables 로딩 과정에서 예외가 발생한 경우 throw될 수 있습니다(try/catch로 로깅 처리).</exception>
        public async Task LoadPrefabsAsync()
        {
            try
            {
                // -----------------------------
                // 1) 카드 일러스트 스프라이트 로드
                // -----------------------------
                _dicImageArt.Clear();
                var label = ConfigAddressableLabelTcg.Card.ImageArt;

                var locationHandle = Addressables.LoadResourceLocationsAsync(label);
                await locationHandle.Task;

                if (!locationHandle.IsValid() || locationHandle.Status != AsyncOperationStatus.Succeeded)
                {
                    GcLogger.LogError($"{label} 레이블을 가진 리소스를 찾을 수 없습니다.");
                    return;
                }

                int totalCount = locationHandle.Result.Count;
                int loadedCount = 0;

                foreach (var location in locationHandle.Result)
                {
                    string address = location.PrimaryKey;

                    var loadHandle = Addressables.LoadAssetAsync<Sprite>(address);

                    // 프레임을 양보하면서 진행률을 갱신(메인 스레드 프리즈 방지)
                    while (!loadHandle.IsDone)
                    {
                        _prefabLoadProgress = (loadedCount + loadHandle.PercentComplete) / totalCount;
                        await Task.Yield();
                    }

                    _activeHandles.Add(loadHandle);

                    Sprite prefab = await loadHandle.Task;
                    if (!prefab) continue;

                    _dicImageArt[address] = prefab;
                    loadedCount++;
                }

                // locations 핸들도 추적(Release 대상)
                _activeHandles.Add(locationHandle);

                // -----------------------------
                // 2) 카드 테두리 스프라이트 로드
                // -----------------------------
                {
                    _dicImageBorder.Clear();

                    label = ConfigAddressableLabelTcg.Card.ImageBorder;
                    locationHandle = Addressables.LoadResourceLocationsAsync(label);
                    await locationHandle.Task;

                    if (!locationHandle.IsValid() || locationHandle.Status != AsyncOperationStatus.Succeeded)
                    {
                        GcLogger.LogError($"{label} 레이블을 가진 리소스를 찾을 수 없습니다.");
                        return;
                    }

                    totalCount = locationHandle.Result.Count;
                    loadedCount = 0;

                    foreach (var location in locationHandle.Result)
                    {
                        string address = location.PrimaryKey;

                        var loadHandle = Addressables.LoadAssetAsync<Sprite>(address);

                        while (!loadHandle.IsDone)
                        {
                            _prefabLoadProgress = (loadedCount + loadHandle.PercentComplete) / totalCount;
                            await Task.Yield();
                        }

                        _activeHandles.Add(loadHandle);

                        Sprite prefab = await loadHandle.Task;
                        if (!prefab) continue;

                        _dicImageBorder[address] = prefab;
                        loadedCount++;
                    }

                    _activeHandles.Add(locationHandle);
                }

                _prefabLoadProgress = 1f; // 100%
            }
            catch (Exception ex)
            {
                GcLogger.LogError($"프리팹 로딩 중 오류 발생: {ex.Message}");
            }
        }

        /// <summary>
        /// 캐시된 카드 일러스트 스프라이트를 Addressables 키(PrimaryKey)로 조회합니다.
        /// </summary>
        /// <param name="prefabName">조회할 Addressables PrimaryKey(주소)입니다.</param>
        /// <returns>해당 키의 스프라이트가 있으면 반환하고, 없으면 <c>null</c>을 반환합니다.</returns>
        public Sprite GetImageArtByName(string prefabName)
        {
            if (_dicImageArt.TryGetValue(prefabName, out var sprite))
            {
                return sprite;
            }

            GcLogger.LogError($"Addressables에서 {prefabName} 스프라이트를 찾을 수 없습니다.");
            return null;
        }

        /// <summary>
        /// 캐시된 카드 테두리 스프라이트를 Addressables 키(PrimaryKey)로 조회합니다.
        /// </summary>
        /// <param name="prefabName">조회할 Addressables PrimaryKey(주소)입니다.</param>
        /// <returns>해당 키의 스프라이트가 있으면 반환하고, 없으면 <c>null</c>을 반환합니다.</returns>
        public Sprite GetImageBorderByName(string prefabName)
        {
            if (_dicImageBorder.TryGetValue(prefabName, out var sprite))
            {
                return sprite;
            }

            GcLogger.LogError($"Addressables에서 {prefabName} 스프라이트를 찾을 수 없습니다.");
            return null;
        }

        /// <summary>
        /// 현재 프리팹(스프라이트) 로딩 진행률을 반환합니다.
        /// </summary>
        /// <returns>0~1 범위의 진행률 값입니다.</returns>
        public float GetPrefabLoadProgress() => _prefabLoadProgress;
    }
}
