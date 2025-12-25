using System.Collections;
using System.Collections.Generic;
using GGemCo2DCore;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace GGemCo2DTcg
{
    /// <summary>
    /// <para>Localization 매니저</para>
    /// </summary>
    public class LocalizationManagerTcg : LocalizationManagerBase
    {
        public static LocalizationManagerTcg Instance;
        // 사용자 언어 테이블 존재 여부
        private readonly Dictionary<string, bool> _userTableExistsMap = new();

        protected override void Awake()
        {
            base.Awake();
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
        /// 사용자 언어 테이블 존재 체크
        /// </summary>
        /// <returns></returns>
        protected override IEnumerator CheckUserTablesExist()
        {
            foreach (string baseTable in LocalizationConstantsTcg.Tables.All)
            {
                string userTableName = $"{baseTable}_User";
                var handle = stringDatabase.GetTableAsync(userTableName, LocalizationSettings.SelectedLocale);
                yield return handle;

                bool exists = false;

                if (handle.IsValid())
                {
                    if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
                    {
                        exists = true;
                        // GcLogger.Log($"table: {userTableName} / exist: true");
                    }
                    else
                    {
                        // GcLogger.Log($"table: {userTableName} / exist: false");
                    }
                }
                else
                {
                    GcLogger.LogWarning($"Invalid handle for table: {userTableName}");
                }

                _userTableExistsMap[baseTable] = exists;

                // handle이 Release 가능한 경우라면 아래 코드도 추가
                if (handle.IsValid())
                    Addressables.Release(handle);
            }
        }

        /// <summary>
        /// UI 에서 사용하는 공용 단어
        /// </summary>
        public string GetUIWindowCardInfoByKey(string key) => GetString(LocalizationConstantsTcg.Tables.UIWindowCardInfo, key);

        public string GetCardNameByKey(string key) => GetString(LocalizationConstantsTcg.Tables.CardName, key);

        public string GetUIWindowMyDeckByKey(string key) => GetString(LocalizationConstantsTcg.Tables.UIWindowMyDeck, key);

        public string GetAbilityDescriptionByKey(string key) => GetString(LocalizationConstantsTcg.Tables.AbilityDescription, key);

        public string GetAbilityTriggerByKey(string key) => GetString(LocalizationConstantsTcg.Tables.AbilityTrigger, key);
    }
}
