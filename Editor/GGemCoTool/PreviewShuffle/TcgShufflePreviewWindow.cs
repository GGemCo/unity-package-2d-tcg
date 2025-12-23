using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using GGemCo2DCore;
using GGemCo2DTcg;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace GGemCo2DTcgEditor
{
    /// <summary>
    /// 셔플 결과 미리보기 전용 에디터 툴.
    ///
    /// 요구사항
    /// - 플레이어: MyDeck SaveData(=SaveDataTcg JSON)에서 덱을 선택
    /// - 적: EnemyDeckPreset ScriptableObject에서 프리셋 선택
    /// - ShuffleMode + ShuffleSettings(ScriptableObject) 선택
    /// - Preview 버튼을 누르면 셔플 결과를 출력
    ///
    /// 주의
    /// - 이 툴은 런타임 SceneGame/SaveDataManagerTcg에 의존하지 않도록,
    ///   SaveDataTcg JSON을 직접 로드/파싱합니다.
    /// - EnemyDeckPreset.BuildDeckUids는 카드 테이블 정보를 필요로 하므로,
    ///   카드 테이블 CSV(TextAsset) 또는 런타임 테이블 Dictionary를 입력받아야 합니다.
    /// </summary>
    public class TcgShufflePreviewWindow : EditorWindow
    {
        private enum DeckSource
        {
            PlayerSave,
            EnemyPreset
        }
        private const string Title = "카드 덱 미리보기";
        [MenuItem(ConfigEditorTcg.NameToolPreviewShuffle, false, (int)ConfigEditorTcg.ToolOrdering.PreviewShuffle)]
        public static void Open()
        {
            var w = GetWindow<TcgShufflePreviewWindow>();
            w.titleContent = new GUIContent(Title);
            w.minSize = new Vector2(760, 520);
            w.Show();
        }
        
        // ─────────────────────────────────────────────────────────────────────────────
        // Inputs
        // ─────────────────────────────────────────────────────────────────────────────
        private GGemCoTcgSettings tcgSettings;

        private TextAsset cardTableCsv;

        private TextAsset saveDataTcgJson;
        private string saveDataTcgPath;
        private bool includeHeroCardInDeck = true;

        private EnemyDeckPreset enemyPreset;
        private bool includeEnemyHeroCardInDeck = true;

        private ConfigCommonTcg.ShuffleMode shuffleMode = ConfigCommonTcg.ShuffleMode.PhaseWeighted;
        private GGemCoTcgPhaseShuffleSettings shuffleSettings;
        private bool useFixedSeed;
        private int fixedSeed;

        private DeckSource previewSource = DeckSource.PlayerSave;
        private int previewDeckIndex;
        
        // ─────────────────────────────────────────────────────────────────────────────
        // Runtime cache
        // ─────────────────────────────────────────────────────────────────────────────

        private Vector2 _scroll;
        private Vector2 _resultScroll;
        
        private SaveDataContainerTcg _saveCache;
        private List<int> _playerDeckIndices = new();
        private string[] _playerDeckLabels = Array.Empty<string>();

        private Dictionary<int, StruckTableTcgCard> _cardTableCache;
        private List<PreviewCard> _lastResult;
        private string _lastLog;
        
        private TableLoaderManagerTcg _tableLoaderManagerTcg;
        
        protected void OnEnable()
        {
            _tableLoaderManagerTcg = new TableLoaderManagerTcg();
            _cardTableCache = _tableLoaderManagerTcg.LoadTableTcgCard().GetDatas();
        }
        
        private void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            DrawHeader();
            EditorGUILayout.Space(6);
            DrawSettingsSection();
            EditorGUILayout.Space(10);
            DrawCardTableSection();
            EditorGUILayout.Space(10);
            DrawPlayerSaveSection();
            EditorGUILayout.Space(10);
            DrawEnemyPresetSection();
            EditorGUILayout.Space(10);
            DrawShuffleSection();
            EditorGUILayout.Space(10);
            DrawPreviewSection();
            EditorGUILayout.Space(10);
            DrawResultSection();
            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField(Title, EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "SaveDataTcg(JSON) 또는 EnemyDeckPreset을 선택한 뒤, ShuffleMode/ShuffleSettings를 지정하고 Preview를 실행하면 셔플 결과를 확인할 수 있습니다.",
                MessageType.Info);
        }
        private void DrawSettingsSection()
        {
            EditorGUILayout.LabelField("Battle Settings", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                tcgSettings = (GGemCoTcgSettings)EditorGUILayout.ObjectField(
                    new GUIContent("GGemCoTcgSettings", "마나/드로우 등 전투 기본 설정을 가져옵니다."),
                    tcgSettings,
                    typeof(GGemCoTcgSettings),
                    false);

                if (tcgSettings != null)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.LabelField($"StartMana: {tcgSettings.countManaBattleStart}, MaxMana: {tcgSettings.countMaxManaInBattle}, ManaPerTurn: {tcgSettings.countManaAfterTurn}");
                        EditorGUILayout.LabelField($"InitialDraw: {tcgSettings.startingHandCardCount}, DrawPerTurn: {tcgSettings.cardsDrawnPerTurn}");
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        "GGemCoTcgSettings를 지정하지 않으면, FrontLoadedCount 계산에 필요한 값들을 기본값(1/10/1/3/1)으로 가정합니다.",
                        MessageType.Warning);
                }
            }
        }
        private void DrawCardTableSection()
        {
            EditorGUILayout.LabelField("Card Table", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (_cardTableCache != null)
                {
                    EditorGUILayout.LabelField($"Loaded Cards: {_cardTableCache.Count}");
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        "EnemyDeckPreset 미리보기 및 코스트 기반 셔플을 위해 카드 테이블이 필요합니다.",
                        MessageType.Info);
                }
            }
        }
        
        #region Draw Player Section
        private void DrawPlayerSaveSection()
        {
            EditorGUILayout.LabelField("Player - MyDeck SaveData", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                saveDataTcgJson = (TextAsset)EditorGUILayout.ObjectField(
                    new GUIContent("SaveDataTcg JSON", "빌드/에디터에서 생성된 SaveDataTcg JSON 파일을 TextAsset으로 지정할 수 있습니다."),
                    saveDataTcgJson,
                    typeof(TextAsset),
                    false);

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(new GUIContent("File Path", "TextAsset이 없으면 파일 경로로 직접 로드할 수 있습니다."), GUILayout.Width(60));
                    saveDataTcgPath = EditorGUILayout.TextField(saveDataTcgPath);
                    if (GUILayout.Button("Browse", GUILayout.Width(80)))
                    {
                        var p = EditorUtility.OpenFilePanel("Select SaveDataTcg JSON", Application.persistentDataPath, "json");
                        if (!string.IsNullOrWhiteSpace(p))
                            saveDataTcgPath = p;
                    }
                }

                includeHeroCardInDeck = EditorGUILayout.ToggleLeft(
                    new GUIContent("Include Hero Card", "MyDeckSaveData.heroCardUid를 덱 리스트에 포함합니다."),
                    includeHeroCardInDeck);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Load Save", GUILayout.Height(22)))
                    {
                        LoadSaveData();
                    }

                    if (GUILayout.Button("Clear Save Cache", GUILayout.Height(22)))
                    {
                        _saveCache = null;
                        _playerDeckIndices.Clear();
                        _playerDeckLabels = Array.Empty<string>();
                    }
                }

                if (_saveCache?.MyDeckData?.myDeckSaveData != null && _saveCache.MyDeckData.myDeckSaveData.Count > 0)
                {
                    EditorGUILayout.LabelField($"Deck Count: {_saveCache.MyDeckData.myDeckSaveData.Count}");
                    previewDeckIndex = DrawDeckPopup("Preview Deck", previewDeckIndex);
                }
                else
                {
                    EditorGUILayout.HelpBox("SaveDataTcg를 로드하면 MyDeck 목록을 선택할 수 있습니다.", MessageType.Info);
                }
            }
        }
        
        private void LoadSaveData()
        {
            _saveCache = null;
            _playerDeckIndices.Clear();
            _playerDeckLabels = Array.Empty<string>();
            _lastLog = null;

            string json = null;
            if (saveDataTcgJson != null)
            {
                json = saveDataTcgJson.text;
            }
            else if (!string.IsNullOrWhiteSpace(saveDataTcgPath) && File.Exists(saveDataTcgPath))
            {
                json = File.ReadAllText(saveDataTcgPath);
            }

            if (string.IsNullOrWhiteSpace(json))
            {
                _lastLog = "[SaveData] JSON을 찾을 수 없습니다. TextAsset 또는 File Path를 지정하세요.";
                return;
            }

            try
            {
                _saveCache = JsonConvert.DeserializeObject<SaveDataContainerTcg>(json);
                BuildDeckPopupCache();
                _lastLog = $"[SaveData] Loaded. DeckCount={_playerDeckIndices.Count}";
            }
            catch (Exception e)
            {
                _saveCache = null;
                _lastLog = $"[SaveData] Deserialize failed: {e.Message}\n{e.StackTrace}";
            }
        }
        private void BuildDeckPopupCache()
        {
            _playerDeckIndices.Clear();
            if (_saveCache?.MyDeckData?.myDeckSaveData == null)
                return;

            var dict = _saveCache.MyDeckData.myDeckSaveData;
            foreach (var kv in dict.OrderBy(k => k.Key))
                _playerDeckIndices.Add(kv.Key);

            _playerDeckLabels = _playerDeckIndices
                .Select(i =>
                {
                    var d = dict.GetValueOrDefault(i);
                    var name = d?.deckName ?? "(NoName)";
                    return $"[{i}] {name}";
                })
                .ToArray();
        }
        private int DrawDeckPopup(string label, int current)
        {
            if (_playerDeckIndices.Count == 0)
                return current;

            int popupIndex = Mathf.Clamp(_playerDeckIndices.IndexOf(current), 0, _playerDeckIndices.Count - 1);
            popupIndex = EditorGUILayout.Popup(new GUIContent(label), popupIndex, _playerDeckLabels);
            return _playerDeckIndices[Mathf.Clamp(popupIndex, 0, _playerDeckIndices.Count - 1)];
        }
        #endregion

        #region Draw Enemy Section

        private void DrawEnemyPresetSection()
        {
            EditorGUILayout.LabelField("Enemy - AI Preset", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                enemyPreset = (EnemyDeckPreset)EditorGUILayout.ObjectField(
                    new GUIContent("EnemyDeckPreset", "적 덱 구성용 프리셋 ScriptableObject"),
                    enemyPreset,
                    typeof(EnemyDeckPreset),
                    false);

                includeEnemyHeroCardInDeck = EditorGUILayout.ToggleLeft(
                    new GUIContent("Include Hero Card", "EnemyDeckPreset.heroCardUid/heroCardUids를 덱 리스트에 포함합니다."),
                    includeEnemyHeroCardInDeck);

                if (enemyPreset != null)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUILayout.LabelField($"Preset: {enemyPreset.displayName} ({enemyPreset.presetId})");
                        EditorGUILayout.LabelField($"DeckSize: {enemyPreset.deckSize}, Difficulty: {enemyPreset.difficulty}");
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("EnemyDeckPreset을 지정하면 적 덱을 생성하여 셔플 미리보기를 할 수 있습니다.", MessageType.Info);
                }
            }
        }

        #endregion

        #region Draw Shuffle Section
        private void DrawShuffleSection()
        {
            EditorGUILayout.LabelField("Shuffle", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                shuffleMode = (ConfigCommonTcg.ShuffleMode)EditorGUILayout.EnumPopup(
                    new GUIContent("ShuffleMode"),
                    shuffleMode);

                shuffleSettings = (GGemCoTcgPhaseShuffleSettings)EditorGUILayout.ObjectField(
                    new GUIContent("ShuffleSettings", "PhaseWeighted 모드에서 사용되는 ScriptableObject"),
                    shuffleSettings,
                    typeof(GGemCoTcgPhaseShuffleSettings),
                    false);

                useFixedSeed = EditorGUILayout.ToggleLeft(new GUIContent("Use Fixed Seed"), useFixedSeed);
                using (new EditorGUI.DisabledScope(!useFixedSeed))
                {
                    fixedSeed = EditorGUILayout.IntField(new GUIContent("Fixed Seed"), fixedSeed);
                }

                if (shuffleMode == ConfigCommonTcg.ShuffleMode.PhaseWeighted && shuffleSettings == null)
                {
                    EditorGUILayout.HelpBox("PhaseWeighted 모드에서는 ShuffleSettings가 필요합니다.", MessageType.Warning);
                }
            }
        }

        #endregion
        
        private void DrawPreviewSection()
        {
            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                previewSource = (DeckSource)EditorGUILayout.EnumPopup(new GUIContent("Deck Source"), previewSource);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Preview Shuffle", GUILayout.Height(26)))
                    {
                        PreviewShuffle();
                    }
                    if (GUILayout.Button("Copy Log", GUILayout.Height(26)))
                    {
                        if (!string.IsNullOrEmpty(_lastLog))
                            EditorGUIUtility.systemCopyBuffer = _lastLog;
                    }
                }
            }
        }
        private void DrawResultSection()
        {
            EditorGUILayout.LabelField("Result", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (_lastResult == null || _lastResult.Count == 0)
                {
                    EditorGUILayout.HelpBox("Preview를 실행하면 결과가 표시됩니다.", MessageType.Info);
                    return;
                }

                EditorGUILayout.LabelField($"Cards: {_lastResult.Count}");

                _resultScroll = EditorGUILayout.BeginScrollView(_resultScroll, GUILayout.MinHeight(220));
                EditorGUILayout.TextArea(_lastLog ?? string.Empty, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();
            }
        }

        #region Preview

        private void PreviewShuffle()
        {
            _lastResult = null;
            _lastLog = null;

            if (_cardTableCache == null || _cardTableCache.Count == 0)
            {
                _lastLog = "[Preview] 카드 테이블이 없습니다. Card Table을 로드하세요.";
                return;
            }

            var deckUids = previewSource switch
            {
                DeckSource.PlayerSave => BuildPlayerDeckUids(previewDeckIndex),
                DeckSource.EnemyPreset => BuildEnemyDeckUids(),
                _ => null
            };

            if (deckUids == null || deckUids.Count == 0)
            {
                _lastLog = "[Preview] 덱 카드 목록이 비어 있습니다.";
                return;
            }

            // 카드 UID -> PreviewCard(Cost 포함)
            var deckCards = new List<PreviewCard>(deckUids.Count);
            int missing = 0;
            for (int i = 0; i < deckUids.Count; i++)
            {
                int uid = deckUids[i];
                if (_cardTableCache.TryGetValue(uid, out var row))
                {
                    deckCards.Add(new PreviewCard(uid, row.name, row.cost));
                }
                else
                {
                    missing++;
                    deckCards.Add(new PreviewCard(uid, "(Missing)", 0));
                }
            }

            // ShuffleMetaData 구성
            var seedManager = new SeedManager(useFixedSeed ? fixedSeed : (int?)null);
            var config = BuildShuffleConfig(deckCards.Count);
            
            var meta = new ShuffleMetaData(shuffleMode, seedManager, config);

            // 실제 셔플
            var backup = UnityEngine.Random.state;   // 저장 :contentReference[oaicite:3]{index=3}
            try
            {
                // meta.Strategy.Shuffle(...) 내부에서 SeedManager.ApplySeed()가 InitState 수행
                meta.Strategy.Shuffle(deckCards, meta);
            }
            finally
            {
                UnityEngine.Random.state = backup;   // 복원 :contentReference[oaicite:4]{index=4}
            }

            _lastResult = deckCards;

            // 로그 출력
            _lastLog = BuildResultLog(deckCards, meta, missing);
        }
        private List<int> BuildPlayerDeckUids(int deckIndex)
        {
            if (_saveCache?.MyDeckData?.myDeckSaveData == null)
                return null;

            if (!_saveCache.MyDeckData.myDeckSaveData.TryGetValue(deckIndex, out var deck) || deck == null)
                return null;

            var result = new List<int>(64);

            // if (includeHeroCardInDeck && deck.heroCardUid != 0)
            //     result.Add(deck.heroCardUid);

            if (deck.cardList != null)
            {
                foreach (var kv in deck.cardList)
                {
                    int uid = kv.Key;
                    int count = Mathf.Max(0, kv.Value);
                    for (int i = 0; i < count; i++)
                        result.Add(uid);
                }
            }

            return result;
        }
        private List<int> BuildEnemyDeckUids()
        {
            if (enemyPreset == null)
                return null;

            // EnemyDeckPreset.BuildDeckUids는 StruckTableTcgCard 딕셔너리를 요구합니다.
            var deck = enemyPreset.BuildDeckUids(_cardTableCache);

            if (!includeEnemyHeroCardInDeck)
                return deck;

            // 프리셋에서 heroCardUid 우선, 없으면 heroCardUids에서 하나 선택
            int heroUid = enemyPreset.heroCardUid;
            if (heroUid == 0 && enemyPreset.heroCardUids != null && enemyPreset.heroCardUids.Count > 0)
                heroUid = enemyPreset.heroCardUids[0]; // 미리보기: 첫 번째를 대표로 사용

            if (heroUid != 0)
                deck.Insert(0, heroUid);

            return deck;
        }
        private ShuffleConfig BuildShuffleConfig(int deckSize)
        {
            int startMana = tcgSettings != null ? tcgSettings.countManaBattleStart : 1;
            int maxMana = tcgSettings != null ? tcgSettings.countMaxManaInBattle : 10;
            int manaPerTurn = tcgSettings != null ? tcgSettings.countManaAfterTurn : 1;
            int initialDraw = tcgSettings != null ? tcgSettings.startingHandCardCount : 3;
            int drawPerTurn = tcgSettings != null ? tcgSettings.cardsDrawnPerTurn : 1;

            // PhaseWeighted는 ShuffleSettings에서 FrontLoadedCount/가중치 구성을 만든다.
            if (shuffleMode == ConfigCommonTcg.ShuffleMode.PhaseWeighted)
            {
                if (shuffleSettings == null)
                    return new ShuffleConfig { FrontLoadedCount = 0 };

                return shuffleSettings.BuildShuffleConfig(deckSize, startMana, maxMana, manaPerTurn, initialDraw, drawPerTurn);
            }

            // 설정 에셋 없이는 의미가 약하므로 PureRandom으로 안전 폴백
            var config = new ShuffleConfig();

            return config;
        }
        private static string BuildResultLog(List<PreviewCard> cards, ShuffleMetaData meta, int missing)
        {
            var sb = new System.Text.StringBuilder(4096);

            sb.AppendLine($"ShuffleMode: {meta.Mode}");
            sb.AppendLine($"Seed Fixed: {(meta.SeedManager.FixedSeed.HasValue ? meta.SeedManager.FixedSeed.Value.ToString() : "(runtime)")}, LastUsedSeed: {meta.SeedManager.LastUsedSeed}");
            sb.AppendLine($"FrontLoadedCount: {meta.Config?.FrontLoadedCount ?? 0}");
            sb.AppendLine($"Missing Uids: {missing}");
            sb.AppendLine();

            // 코스트 분포
            var costGroups = cards
                .GroupBy(c => c.Cost)
                .OrderBy(g => g.Key)
                .Select(g => $"Cost {g.Key}: {g.Count()}");
            sb.AppendLine("Cost Distribution:");
            foreach (var line in costGroups)
                sb.AppendLine("- " + line);

            sb.AppendLine();
            sb.AppendLine("Deck Order (Top -> Bottom):");

            for (int i = 0; i < cards.Count; i++)
            {
                var c = cards[i];
                sb.Append(i.ToString("00", CultureInfo.InvariantCulture));
                sb.Append(": ");
                sb.Append("Uid=");
                sb.Append(c.Uid);
                sb.Append(", Cost=");
                sb.Append(c.Cost);
                if (!string.IsNullOrEmpty(c.Name))
                {
                    sb.Append(", Name=");
                    sb.Append(c.Name);
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        #endregion
        // ─────────────────────────────────────────────────────────────────────────────
        // Preview card
        // ─────────────────────────────────────────────────────────────────────────────

        [Serializable]
        private sealed class PreviewCard : ICardInfo
        {
            public int Uid { get; }
            public string Name { get; }
            public int Cost { get; }

            public PreviewCard(int uid, string name, int cost)
            {
                Uid = uid;
                Name = name;
                Cost = cost;
            }
        }
    }
}