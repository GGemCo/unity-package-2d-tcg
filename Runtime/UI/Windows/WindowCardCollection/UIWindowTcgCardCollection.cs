using System.Collections.Generic;
using GGemCo2DCore;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 카드 콜렉션 윈도우
    /// </summary>
    public class UIWindowTcgCardCollection : UIWindow
    {
        [Header(UIWindowConstants.TitleHeaderIndividual)]
        [Tooltip("선택 타입")]
        public TMP_Dropdown dropdownType;
        [Tooltip("선택 등급")]
        public TMP_Dropdown dropdownGrade;
        [Tooltip("선택 소모 비용")]
        public TMP_Dropdown dropdownCost;
        [Tooltip("필터링 버튼")]
        public Button buttonFiltering;
        [Tooltip("필터링 리셋 버튼")]
        public Button buttonResetFiltering;

        public UnityEvent onFiltering;
        
        private UIWindowTcgMyDeck _uiWindowTcgTcgMyDeck;
        private TableTcgCard _tableTcgCard;
                        
        // GC 할당 줄이기용 버퍼
        private static readonly List<int> BufferTypeIndices  = new List<int>(8);
        private static readonly List<int> BufferGradeIndices = new List<int>(8);
        private static readonly List<int> BufferCostIndices  = new List<int>(8);
        
        protected override void Awake()
        {
            uid = UIWindowConstants.WindowUid.TcgCardCollection;
            if (TableLoaderManager.Instance == null) return;
            _tableTcgCard = TableLoaderManagerTcg.Instance.TableTcgCard;            maxCountIcon = _tableTcgCard.GetCount();
            
            // 순서 중요. IconPoolManager 에서 사용한다.
            SlotIconBuildStrategyRegistry.Register(
                UIWindowConstants.WindowUid.TcgCardCollection,
                window => new SlotIconBuildStrategyCollection(_tableTcgCard)
            );
            base.Awake();
            
            IconPoolManager.SetSetIconHandler(new SetIconHandlerCardCollection());
            DragDropHandler.SetStrategy(new DragDropStrategyCardCollection());

            InitializeDropDown();
            InitializeButton();
        }

        private void InitializeButton()
        {
            buttonFiltering?.onClick.AddListener(OnClickFiltering);
            buttonResetFiltering?.onClick.AddListener(OnClickResetFiltering);
        }

        private void InitializeDropDown()
        {
            dropdownType?.ClearOptions();
            // 표시명 정렬(원하는 정렬 기준으로 변경 가능)
            var options = new List<TMP_Dropdown.OptionData>();
            foreach (var type in EnumCache<CardConstants.Type>.Values)
            {
                options.Add(new TMP_Dropdown.OptionData($"{type}"));   
            }
            dropdownType?.AddOptions(options);
            
            dropdownGrade?.ClearOptions();
            // 표시명 정렬(원하는 정렬 기준으로 변경 가능)
            options = new List<TMP_Dropdown.OptionData>();
            foreach (var type in EnumCache<CardConstants.Grade>.Values)
            {
                options.Add(new TMP_Dropdown.OptionData($"{type}"));   
            }
            dropdownGrade?.AddOptions(options);
            
            dropdownCost?.ClearOptions();
            // 표시명 정렬(원하는 정렬 기준으로 변경 가능)
            options = new List<TMP_Dropdown.OptionData>();
            for (int i = 0; i <= 10; ++i)
            {
                options.Add(new TMP_Dropdown.OptionData($"{i}"));   
            }
            dropdownCost?.AddOptions(options);
        }

        private void OnDestroy()
        {
            buttonFiltering?.onClick.RemoveAllListeners();
        }

        protected override void Start()
        {
            base.Start();
            _uiWindowTcgTcgMyDeck =
                SceneGame.uIWindowManager.GetUIWindowByUid<UIWindowTcgMyDeck>(UIWindowConstants.WindowUid.TcgMyDeck);
        }

        public void SetPositionUiSlot(UISlot uiSlot, int index)
        {
        }

        private void OnClickResetFiltering()
        {
            if (dropdownType != null)
                dropdownType.value = 0;
            if (dropdownGrade != null)
                dropdownGrade.value = 0;
            if (dropdownCost != null)
                dropdownCost.value = 0;
            OnClickFiltering();
        }

        private void OnClickFiltering()
        {
            // 1) 드롭다운에서 선택 인덱스 가져오기 (단일/멀티 공통)
            GetSelectedIndices(dropdownType, BufferTypeIndices);
            GetSelectedIndices(dropdownGrade, BufferGradeIndices);
            GetSelectedIndices(dropdownCost, BufferCostIndices);

            // 2) 0 인덱스(전체) 제거 → 실제 필터 조건 여부 확인
            bool hasTypeFilter = NormalizeFilterIndices(BufferTypeIndices);
            bool hasGradeFilter = NormalizeFilterIndices(BufferGradeIndices);
            bool hasCostFilter = NormalizeFilterIndices(BufferCostIndices);

            // GcLogger.Log(
            //     $"[CardFilter] Type: [{string.Join(",", BufferTypeIndices)}], " +
            //     $"Grade: [{string.Join(",", BufferGradeIndices)}], " +
            //     $"Cost: [{string.Join(",", BufferCostIndices)}]"
            // );

            foreach (var icon in icons)
            {
                if (!icon) continue;

                var iconCard = icon.GetComponent<UIIconCard>();
                if (!iconCard) continue;
                var slot = GetSlotByIndex(iconCard.index);
                if (!slot) continue;

                bool isVisible = true;

                // 3) Type 필터 (여러 개 중 하나라도 맞으면 통과)
                if (hasTypeFilter)
                {
                    bool matchType = false;
                    foreach (var idx in BufferTypeIndices)
                    {
                        if (!iconCard.IsType(idx)) continue; // 기존에 사용하던 int 인덱스 기반 메서드라고 가정
                        matchType = true;
                        break;
                    }

                    if (!matchType)
                        isVisible = false;
                }

                // 4) Grade 필터
                if (isVisible && hasGradeFilter)
                {
                    bool matchGrade = false;
                    foreach (var idx in BufferGradeIndices)
                    {
                        if (!iconCard.IsGrade(idx)) continue; // UIElementCard에 이 메서드만 추가해 주면 됨
                        matchGrade = true;
                        break;
                    }

                    if (!matchGrade)
                        isVisible = false;
                }

                // 5) Cost 필터
                if (isVisible && hasCostFilter)
                {
                    bool matchCost = false;
                    foreach (var idx in BufferCostIndices)
                    {
                        if (!iconCard.IsCost(idx)) continue; // Cost 드롭다운이 0~10 그대로라면 인덱스 == 코스트
                        matchCost = true;
                        break;
                    }

                    if (!matchCost)
                        isVisible = false;
                }
                slot.isFiltering = isVisible;
                slot.gameObject.SetActive(isVisible);
            }
            onFiltering?.Invoke();
        }
        private static bool TryGetMultiSelect(TMP_Dropdown dropdown, out bool isMultiSelect)
        {
            isMultiSelect = false;
            if (dropdown == null) return false;

            var prop = dropdown.GetType().GetProperty("MultiSelect");
            if (prop == null || prop.PropertyType != typeof(bool))
                return false;

            isMultiSelect = (bool)prop.GetValue(dropdown);
            return true;
        }
        /// <summary>
        /// TMP_Dropdown에서 선택된 인덱스 목록을 얻는다.
        /// - MultiSelect == false : dropdown.value 하나만 반환
        /// - MultiSelect == true  : value 비트마스크를 풀어서 여러 인덱스 반환
        /// </summary>
        private static void GetSelectedIndices(TMP_Dropdown dropdown, List<int> result)
        {
            result.Clear();

            if (dropdown == null)
                return;

            // Multi Select 모드
            if (TryGetMultiSelect(dropdown, out var isMultiSelect) && isMultiSelect)
            {
                int mask = dropdown.value; // 0..255 비트마스크

                // 아무 것도 선택하지 않은 경우(또는 내부적으로 None 등)
                if (mask == 0)
                {
                    // 여기서는 "필터 없음" 취급을 위해 0 인덱스를 하나 넣어둔다.
                    // Multi Select의 None/All 동작을 다르게 쓰고 싶다면 이 부분만 조정하면 됨.
                    result.Add(0);
                    return;
                }

                // 옵션 개수만큼 비트를 읽되, 비트 수(0~31)도 한계로 둔다.
                int optionCount = Mathf.Min(dropdown.options.Count, 32);
                for (int i = 0; i < optionCount; ++i)
                {
                    if ((mask & (1 << i)) != 0)
                        result.Add(i);
                }
            }
            else
            {
                // 단일 선택 모드
                if (dropdown.value >= 0)
                    result.Add(dropdown.value);
            }
        }
        /// <summary>
        /// 인덱스 리스트에서 0(전체)을 제거하고,
        /// 실제 필터 조건이 존재하는지 여부를 반환한다.
        /// </summary>
        private static bool NormalizeFilterIndices(List<int> indices)
        {
            for (int i = indices.Count - 1; i >= 0; --i)
            {
                // 0 → "전체" 또는 "필터 없음" 의미로 사용
                if (indices[i] <= 0)
                    indices.RemoveAt(i);
            }

            // true면 실제 필터 조건이 있음
            return indices.Count > 0;
        }

        public string GetAbilityDescription(int cardUid)
        {
            var info = _tableTcgCard.GetDataByUid(cardUid);
            return info?.description ?? string.Empty;
        }
    }
}