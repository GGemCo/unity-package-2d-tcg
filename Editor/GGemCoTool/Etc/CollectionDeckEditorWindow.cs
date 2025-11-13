// CollectionDeckEditorWindow.cs
// Unity 2021+ 기준 IMGUI 에디터 예시
// 기능 요약:
// - 상단: 덱 선택, 덱 이름, 저장/되돌리기 버튼
// - 좌측: 콜렉션(검색/필터 + 스크롤 리스트)
// - 중앙: 선택 카드 상세
// - 우측: 덱 리스트(현재 수량, 규칙 표시)
// 유지보수/확장을 위해 UI그리기와 데이터그리기를 분리

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GGemCo2DTcgEditor
{
    public class CollectionDeckEditorWindow : EditorWindow
    {
        #region Mock Data Types

        // 실제 프로젝트에서는 ScriptableObject나 Addressable에서 불러오세요.
        [Serializable]
        public class CardData
        {
            public string id;
            public string name;
            public int cost;
            public string type;   // ex) Unit, Spell, Item ...
            public string rarity; // ex) Common, Rare, Epic ...
            public string desc;
            public Texture2D preview;
        }

        [Serializable]
        public class DeckEntry
        {
            public string cardId;
            public int count;
        }

        [Serializable]
        public class DeckData
        {
            public string deckName;
            public List<DeckEntry> entries = new();
        }

        #endregion

        #region Fields

        private List<CardData> _collection = new();
        private List<DeckData> _allDecks = new();

        private int _selectedDeckIndex;
        private DeckData _currentDeck;

        // UI용
        private Vector2 _scrollCollection;
        private Vector2 _scrollDeck;
        private Vector2 _scrollDetail;
        private string _searchText = string.Empty;
        private int _selectedCollectionIndex = -1;

        // 필터 예시
        private readonly string[] _rarityOptions = { "All", "Common", "Rare", "Epic" };
        private readonly string[] _typeOptions = { "All", "Unit", "Spell", "Item" };
        private int _selectedRarity = 0;
        private int _selectedType = 0;

        // 덱 규칙
        private const int MaxDeckSize = 40;
        private const int MaxSameCardCount = 3;

        // 레이아웃 비율
        private float _leftWidth = 280f;
        private float _rightWidth = 260f;
        private float _middleWidth = 300f;

        #endregion

        [MenuItem("GGemCoToolTcg/Collection & Deck Editor")]
        public static void Open()
        {
            var window = GetWindow<CollectionDeckEditorWindow>("Collection & Deck");
            window.Show();
        }

        private void OnEnable()
        {
            // 예시 데이터 생성
            if (_collection.Count == 0)
                CreateMockCollection();

            if (_allDecks.Count == 0)
                CreateMockDecks();

            LoadDeck(0);
        }

        private void OnGUI()
        {
            DrawToolbar();

            EditorGUILayout.BeginHorizontal();

            DrawCollectionPanel();
            DrawDetailPanel();
            DrawDeckPanel();

            EditorGUILayout.EndHorizontal();
        }

        #region Toolbar

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            // 덱 선택
            var deckNames = _allDecks.Select(d => d.deckName).ToArray();
            int newDeckIndex = EditorGUILayout.Popup(_selectedDeckIndex, deckNames, EditorStyles.toolbarPopup, GUILayout.Width(180));
            if (newDeckIndex != _selectedDeckIndex)
            {
                LoadDeck(newDeckIndex);
            }

            // 덱 이름 편집
            if (_currentDeck != null)
            {
                _currentDeck.deckName = GUILayout.TextField(_currentDeck.deckName, EditorStyles.toolbarTextField, GUILayout.MinWidth(100));
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Save", EditorStyles.toolbarButton))
            {
                SaveDeck();
            }

            if (GUILayout.Button("Revert", EditorStyles.toolbarButton))
            {
                LoadDeck(_selectedDeckIndex);
            }

            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region Left - Collection

        private void DrawCollectionPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(_leftWidth));
            DrawCollectionFilter();
            DrawCollectionList();
            EditorGUILayout.EndVertical();
        }

        private void DrawCollectionFilter()
        {
            EditorGUILayout.LabelField("Collection", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            _searchText = EditorGUILayout.TextField(_searchText, "SearchTextField");
            if (GUILayout.Button("X", GUILayout.Width(22)))
            {
                _searchText = string.Empty;
                GUI.FocusControl(null);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _selectedRarity = EditorGUILayout.Popup(_selectedRarity, _rarityOptions);
            _selectedType = EditorGUILayout.Popup(_selectedType, _typeOptions);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);
        }

        private void DrawCollectionList()
        {
            _scrollCollection = EditorGUILayout.BeginScrollView(_scrollCollection);

            var filtered = GetFilteredCollection();
            for (int i = 0; i < filtered.Count; i++)
            {
                var card = filtered[i];
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                // 프리뷰 (간단)
                GUILayout.Box(card.preview, GUILayout.Width(40), GUILayout.Height(40));

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField($"{card.name}", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Cost: {card.cost} | {card.type} | {card.rarity}");
                EditorGUILayout.EndVertical();

                if (GUILayout.Button("+", GUILayout.Width(26)))
                {
                    AddCardToDeck(card.id);
                }

                // 더블클릭 처리
                var rect = GUILayoutUtility.GetLastRect();
                if (Event.current.type == EventType.MouseDown && Event.current.clickCount == 2 && rect.Contains(Event.current.mousePosition))
                {
                    AddCardToDeck(card.id);
                    Event.current.Use();
                }

                // 선택 카드 갱신
                if (GUILayout.Button(">", GUILayout.Width(22)))
                {
                    _selectedCollectionIndex = _collection.FindIndex(c => c.id == card.id);
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        private List<CardData> GetFilteredCollection()
        {
            IEnumerable<CardData> query = _collection;

            if (!string.IsNullOrEmpty(_searchText))
            {
                query = query.Where(c => c.name.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            if (_selectedRarity > 0)
            {
                var rarity = _rarityOptions[_selectedRarity];
                query = query.Where(c => c.rarity == rarity);
            }

            if (_selectedType > 0)
            {
                var type = _typeOptions[_selectedType];
                query = query.Where(c => c.type == type);
            }

            // 여기서 정렬 옵션 추가 가능
            return query.ToList();
        }

        #endregion

        #region Middle - Detail

        private void DrawDetailPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(_middleWidth));
            EditorGUILayout.LabelField("Card Detail", EditorStyles.boldLabel);

            _scrollDetail = EditorGUILayout.BeginScrollView(_scrollDetail);

            CardData selected = null;
            if (_selectedCollectionIndex >= 0 && _selectedCollectionIndex < _collection.Count)
            {
                selected = _collection[_selectedCollectionIndex];
            }

            if (selected != null)
            {
                // 이미지
                if (selected.preview != null)
                {
                    float aspect = (float)selected.preview.width / selected.preview.height;
                    float w = _middleWidth - 20;
                    float h = w / aspect;
                    GUILayout.Label(selected.preview, GUILayout.Width(w), GUILayout.Height(h));
                }
                else
                {
                    GUILayout.Box("No Preview", GUILayout.Height(120));
                }

                EditorGUILayout.LabelField(selected.name, EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Cost: {selected.cost}");
                EditorGUILayout.LabelField($"Type: {selected.type}");
                EditorGUILayout.LabelField($"Rarity: {selected.rarity}");
                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("Description:");
                EditorGUILayout.HelpBox(selected.desc, MessageType.None);

                EditorGUILayout.Space(8);
                if (GUILayout.Button("Add to Deck"))
                {
                    AddCardToDeck(selected.id);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("좌측에서 카드를 선택하면 상세 정보가 표시됩니다.", MessageType.Info);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        #endregion

        #region Right - Deck

        private void DrawDeckPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(_rightWidth));
            EditorGUILayout.LabelField("Deck", EditorStyles.boldLabel);

            if (_currentDeck == null)
            {
                EditorGUILayout.HelpBox("덱이 선택되지 않았습니다.", MessageType.Warning);
                EditorGUILayout.EndVertical();
                return;
            }

            int currentCount = _currentDeck.entries.Sum(e => e.count);
            bool isOver = currentCount > MaxDeckSize;

            // 덱 헤더
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Deck Size: {currentCount} / {MaxDeckSize}", isOver ? EditorStyles.boldLabel : EditorStyles.label);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField($"* 같은 카드 최대 {MaxSameCardCount}장", EditorStyles.miniLabel);

            _scrollDeck = EditorGUILayout.BeginScrollView(_scrollDeck);

            // 각 카드 표시
            foreach (var entry in _currentDeck.entries.ToList())
            {
                var card = _collection.FirstOrDefault(c => c.id == entry.cardId);
                string cardName = card != null ? card.name : entry.cardId;
                bool overLimit = entry.count > MaxSameCardCount;

                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUILayout.LabelField(cardName, overLimit ? EditorStyles.boldLabel : EditorStyles.label);

                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField($"x{entry.count}", GUILayout.Width(40));

                if (GUILayout.Button("+", GUILayout.Width(24)))
                {
                    AddCardToDeck(entry.cardId);
                }
                if (GUILayout.Button("-", GUILayout.Width(24)))
                {
                    RemoveCardFromDeck(entry.cardId);
                }
                if (GUILayout.Button("X", GUILayout.Width(24)))
                {
                    RemoveAllFromDeck(entry.cardId);
                }

                EditorGUILayout.EndHorizontal();

                // 규칙 위반 표시
                if (overLimit)
                {
                    EditorGUILayout.HelpBox($"'{cardName}' 카드가 최대 수량({MaxSameCardCount})을 초과했습니다.", MessageType.Warning);
                }
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }

        #endregion

        #region Deck Operations

        private void AddCardToDeck(string cardId)
        {
            if (_currentDeck == null) return;

            var entry = _currentDeck.entries.FirstOrDefault(e => e.cardId == cardId);
            if (entry == null)
            {
                entry = new DeckEntry { cardId = cardId, count = 1 };
                _currentDeck.entries.Add(entry);
            }
            else
            {
                entry.count++;
            }
            Repaint();
        }

        private void RemoveCardFromDeck(string cardId)
        {
            if (_currentDeck == null) return;

            var entry = _currentDeck.entries.FirstOrDefault(e => e.cardId == cardId);
            if (entry == null) return;

            entry.count--;
            if (entry.count <= 0)
            {
                _currentDeck.entries.Remove(entry);
            }
            Repaint();
        }

        private void RemoveAllFromDeck(string cardId)
        {
            if (_currentDeck == null) return;
            _currentDeck.entries.RemoveAll(e => e.cardId == cardId);
            Repaint();
        }

        private void SaveDeck()
        {
            // 실제 프로젝트에서는 ScriptableObject 저장/에셋 저장/JSON 쓰기 등으로 교체
            Debug.Log($"Deck '{_currentDeck.deckName}' saved (mock).");
        }

        private void LoadDeck(int index)
        {
            if (index < 0 || index >= _allDecks.Count) return;
            _selectedDeckIndex = index;
            // 깊은 복사 대신 예제에서는 참조 그대로 사용
            _currentDeck = _allDecks[index];
            Repaint();
        }

        #endregion

        #region Mock Data Create

        private void CreateMockCollection()
        {
            // 간단한 예시 20장
            for (int i = 0; i < 20; i++)
            {
                _collection.Add(new CardData
                {
                    id = $"card_{i}",
                    name = $"Sample Card {i}",
                    cost = UnityEngine.Random.Range(1, 7),
                    type = i % 3 == 0 ? "Unit" : (i % 3 == 1 ? "Spell" : "Item"),
                    rarity = (i % 4) switch
                    {
                        0 => "Common",
                        1 => "Rare",
                        2 => "Epic",
                        _ => "Common"
                    },
                    desc = "샘플 카드 설명입니다.\n이곳에 카드 효과를 적습니다.",
                    preview = Texture2D.grayTexture
                });
            }
        }

        private void CreateMockDecks()
        {
            var deck1 = new DeckData { deckName = "Starter Deck" };
            deck1.entries.Add(new DeckEntry { cardId = "card_0", count = 2 });
            deck1.entries.Add(new DeckEntry { cardId = "card_1", count = 1 });

            var deck2 = new DeckData { deckName = "Aggro Deck" };
            deck2.entries.Add(new DeckEntry { cardId = "card_3", count = 3 });
            deck2.entries.Add(new DeckEntry { cardId = "card_4", count = 2 });

            _allDecks.Add(deck1);
            _allDecks.Add(deck2);
        }

        #endregion
    }
}
