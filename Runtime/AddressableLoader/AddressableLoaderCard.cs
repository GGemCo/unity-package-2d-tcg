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
    /// 아이템 이미지 로드
    /// </summary>
    public class AddressableLoaderCard : MonoBehaviour
    {
        public static AddressableLoaderCard Instance { get; private set; }
        private readonly Dictionary<string, Sprite> _dicImageArt = new Dictionary<string, Sprite>();
        private readonly Dictionary<string, Sprite> _dicImageBorder = new Dictionary<string, Sprite>();
        private readonly HashSet<AsyncOperationHandle> _activeHandles = new HashSet<AsyncOperationHandle>();
        private float _prefabLoadProgress;

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

        private void OnDestroy()
        {
            ReleaseAll();
        }

        /// <summary>
        /// 모든 로드된 리소스를 해제합니다.
        /// </summary>
        private void ReleaseAll()
        {
            AddressableLoaderController.ReleaseByHandles(_activeHandles);
        }
        public async Task LoadPrefabsAsync()
        {
            try
            {
                // 일러스트 이미지
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
                _activeHandles.Add(locationHandle);
                
                // 핸드, 필드 카드 테두리 이미지
                {
                    _dicImageBorder.Clear();
                    // 핸드 카드 테두리 이미지
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
                // GcLogger.Log($"총 {loadedCount}/{totalCount}개의 프리팹을 성공적으로 로드했습니다.");
            }
            catch (Exception ex)
            {
                GcLogger.LogError($"프리팹 로딩 중 오류 발생: {ex.Message}");
            }
        }

        public Sprite GetImageArtByName(string prefabName)
        {
            if (_dicImageArt.TryGetValue(prefabName, out var sprite))
            {
                return sprite;
            }

            GcLogger.LogError($"Addressables에서 {prefabName} 스프라이트를 찾을 수 없습니다.");
            return null;
        }
        public Sprite GetImageBorderByName(string prefabName)
        {
            if (_dicImageBorder.TryGetValue(prefabName, out var sprite))
            {
                return sprite;
            }

            GcLogger.LogError($"Addressables에서 {prefabName} 스프라이트를 찾을 수 없습니다.");
            return null;
        }
        public float GetPrefabLoadProgress() => _prefabLoadProgress;

    }
}
