using System.Collections.Generic;
using GGemCo2DCore;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 카드 컬렉션 UI 윈도우입니다.
    /// - 카드 아이콘 풀/슬롯 빌드 전략을 등록하고,
    /// - 드롭다운(Type/Grade/Cost) 기반 필터링을 수행하며,
    /// - 필터 결과를 슬롯 활성/비활성으로 반영합니다.
    /// </summary>
    public class UIWindowTcgCardCollection : UIWindow
    {
        [Header(UIWindowConstants.TitleHeaderIndividual)]
        [Tooltip("선택 타입 드롭다운")]
        public TMP_Dropdown dropdownType;

        [Tooltip("선택 등급 드롭다운")]
        public TMP_Dropdown dropdownGrade;

        [Tooltip("선택 소모 비용 드롭다운")]
        public TMP_Dropdown dropdownCost;

        [Tooltip("필터링 적용 버튼")]
        public Button buttonFiltering;

        [Tooltip("필터링 리셋 버튼")]
        public Button buttonResetFiltering;

        /// <summary>
        /// 필터링 적용 후 호출되는 이벤트입니다.
        /// 외부에서 UI 갱신(스크롤 리빌드 등)을 연결할 수 있습니다.
        /// </summary>
        public UnityEvent onFiltering;

        /// <summary>
        /// 내 덱(UIWindowTcgMyDeck) 윈도우 참조입니다.
        /// (현재 코드에서는 캐싱만 하고 직접 사용하지 않음)
        /// </summary>
        private UIWindowTcgMyDeck _uiWindowTcgTcgMyDeck;

        /// <summary>
        /// 카드 테이블 참조입니다.
        /// </summary>
        private TableTcgCard _tableTcgCard;

        // GC 할당 줄이기용 버퍼(필터 인덱스 리스트 재사용)
        private static readonly List<int> BufferTypeIndices  = new List<int>(8);
        private static readonly List<int> BufferGradeIndices = new List<int>(8);
        private static readonly List<int> BufferCostIndices  = new List<int>(8);

        /// <summary>
        /// 컴포넌트 초기화 시 호출됩니다.
        /// 윈도우 UID/테이블/아이콘 풀 전략/드래그드롭 전략을 설정하고 UI를 초기화합니다.
        /// </summary>
        protected override void Awake()
        {
            uid = UIWindowConstants.WindowUid.TcgCardCollection;

            // 테이블 로더가 아직 준비되지 않았으면 초기화 불가(조기 반환)
            if (TableLoaderManager.Instance == null) return;

            _tableTcgCard = TableLoaderManagerTcg.Instance.TableTcgCard;
            maxCountIcon = _tableTcgCard.GetCount();

            // 순서 중요: IconPoolManager에서 사용 (슬롯 빌드 전략 등록 후 base.Awake 호출)
            SlotIconBuildStrategyRegistry.Register(
                UIWindowConstants.WindowUid.TcgCardCollection,
                window => new SlotIconBuildStrategyCollection(_tableTcgCard)
            );

            base.Awake();

            // 아이콘 셋업/드래그드롭 정책 설정(컬렉션 전용)
            IconPoolManager.SetSetIconHandler(new SetIconHandlerCardCollection());
            DragDropHandler.SetStrategy(new DragDropStrategyCardCollection());

            InitializeDropDown();
            InitializeButton();
        }

        /// <summary>
        /// 버튼 리스너를 등록합니다.
        /// </summary>
        private void InitializeButton()
        {
            buttonFiltering?.onClick.AddListener(OnClickFiltering);
            buttonResetFiltering?.onClick.AddListener(OnClickResetFiltering);
        }

        /// <summary>
        /// 드롭다운 옵션을 초기화합니다.
        /// Type/Grade는 EnumCache 값을 표시명으로 사용하고,
        /// Cost는 0~10 범위를 옵션으로 구성합니다.
        /// </summary>
        private void InitializeDropDown()
        {
            dropdownType?.ClearOptions();

            // NOTE: options 리스트는 매번 새로 만들고 있어, 필요 시 풀/캐싱으로 최적화 가능
            var options = new List<TMP_Dropdown.OptionData>();
            foreach (var type in EnumCache<CardConstants.Type>.Values)
            {
                options.Add(new TMP_Dropdown.OptionData($"{type}"));
            }
            dropdownType?.AddOptions(options);

            dropdownGrade?.ClearOptions();
            options = new List<TMP_Dropdown.OptionData>();
            foreach (var type in EnumCache<CardConstants.Grade>.Values)
            {
                options.Add(new TMP_Dropdown.OptionData($"{type}"));
            }
            dropdownGrade?.AddOptions(options);

            dropdownCost?.ClearOptions();
            options = new List<TMP_Dropdown.OptionData>();
            for (int i = 0; i <= 10; ++i)
            {
                options.Add(new TMP_Dropdown.OptionData($"{i}"));
            }
            dropdownCost?.AddOptions(options);
        }

        /// <summary>
        /// 오브젝트 파괴 시 호출됩니다.
        /// 등록한 리스너를 해제합니다.
        /// </summary>
        private void OnDestroy()
        {
            buttonFiltering?.onClick.RemoveAllListeners();
            buttonResetFiltering?.onClick.RemoveAllListeners();
        }

        /// <summary>
        /// 첫 프레임 시작 시 호출됩니다.
        /// 내 덱 윈도우 참조를 캐싱합니다.
        /// </summary>
        protected override void Start()
        {
            base.Start();

            _uiWindowTcgTcgMyDeck =
                SceneGame.uIWindowManager.GetUIWindowByUid<UIWindowTcgMyDeck>(UIWindowConstants.WindowUid.TcgMyDeck);
        }

        /// <summary>
        /// 슬롯의 위치/정렬을 외부에서 제어할 때 사용될 수 있는 훅입니다.
        /// </summary>
        /// <param name="uiSlot">대상 슬롯</param>
        /// <param name="index">슬롯 인덱스</param>
        public void SetPositionUiSlot(UISlot uiSlot, int index)
        {
        }

        /// <summary>
        /// 필터를 초기 상태(전체)로 되돌리고 필터링을 다시 적용합니다.
        /// </summary>
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

        /// <summary>
        /// 현재 드롭다운 선택값에 따라 카드 아이콘을 필터링합니다.
        /// - Type/Grade/Cost 각각에 대해 단일/멀티 선택을 지원합니다.
        /// - 필터는 OR(선택된 항목 중 하나라도 일치)로 통과시키고,
        /// - 슬롯의 활성/비활성을 변경하여 화면 표시를 갱신합니다.
        /// </summary>
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
                        // NOTE: UIIconCard.IsType(int)는 드롭다운 인덱스 기반 비교를 수행한다고 가정
                        if (!iconCard.IsType(idx)) continue;
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
                        if (!iconCard.IsGrade(idx)) continue;
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
                        // Cost 드롭다운이 0~10 그대로라면 인덱스 == 코스트로 취급
                        if (!iconCard.IsCost(idx)) continue;
                        matchCost = true;
                        break;
                    }

                    if (!matchCost)
                        isVisible = false;
                }

                // 필터 결과를 슬롯에 반영(표시/비표시)
                slot.isFiltering = isVisible;
                slot.gameObject.SetActive(isVisible);
            }

            onFiltering?.Invoke();
        }

        /// <summary>
        /// TMP_Dropdown이 멀티 셀렉트(MultiSelect) 모드인지 리플렉션으로 확인합니다.
        /// </summary>
        /// <param name="dropdown">대상 드롭다운</param>
        /// <param name="isMultiSelect">멀티 셀렉트 여부</param>
        /// <returns>MultiSelect 프로퍼티를 읽을 수 있으면 true, 아니면 false를 반환합니다.</returns>
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
        /// TMP_Dropdown에서 선택된 인덱스 목록을 얻습니다.
        /// - MultiSelect == false : dropdown.value 하나만 반환
        /// - MultiSelect == true  : dropdown.value를 비트마스크로 해석하여 여러 인덱스를 반환
        /// </summary>
        /// <param name="dropdown">대상 드롭다운</param>
        /// <param name="result">선택 인덱스를 채울 리스트(재사용)</param>
        private static void GetSelectedIndices(TMP_Dropdown dropdown, List<int> result)
        {
            result.Clear();

            if (dropdown == null)
                return;

            // Multi Select 모드
            if (TryGetMultiSelect(dropdown, out var isMultiSelect) && isMultiSelect)
            {
                int mask = dropdown.value; // 0..255 비트마스크(구현에 따라 다를 수 있음)

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
        /// 인덱스 리스트에서 0(전체/필터 없음)을 제거하고,
        /// 실제 필터 조건이 존재하는지 여부를 반환합니다.
        /// </summary>
        /// <param name="indices">선택 인덱스 리스트</param>
        /// <returns>실제 필터 조건이 존재하면 true, 아니면 false를 반환합니다.</returns>
        private static bool NormalizeFilterIndices(List<int> indices)
        {
            for (int i = indices.Count - 1; i >= 0; --i)
            {
                // 0 → "전체" 또는 "필터 없음" 의미로 사용
                if (indices[i] <= 0)
                    indices.RemoveAt(i);
            }

            return indices.Count > 0;
        }

        /// <summary>
        /// 카드 UID에 해당하는 능력/설명 문자열을 반환합니다.
        /// </summary>
        /// <param name="cardUid">카드 UID</param>
        /// <returns>설명 문자열(없으면 빈 문자열)</returns>
        public string GetAbilityDescription(int cardUid)
        {
            var info = _tableTcgCard.GetDataByUid(cardUid);
            return info?.description ?? string.Empty;
        }
    }
}
