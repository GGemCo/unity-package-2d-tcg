using System.Collections.Generic;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// Weighted 셔플(코스트 가중치 기반) 전략에서 사용할 설정 ScriptableObject입니다.
    /// </summary>
    /// <remarks>
    /// - WeightedShuffleStrategy에서 사용하는 <see cref="ShuffleConfig"/>를 생성합니다.
    /// - <see cref="ShuffleConfig.FrontLoadedCount"/>는 기본적으로 “마나 커브 기반(최대 마나 도달 전까지)”으로 계산할 수 있으며,
    ///   필요 시 고정 값으로도 지정할 수 있습니다.
    /// - 코스트별 가중치(<see cref="entries"/>)는 설정된 값이 우선하며, 미설정 코스트에는 <see cref="defaultCostWeight"/>가 적용됩니다.
    /// </remarks>
    [CreateAssetMenu(
        fileName = ConfigScriptableObjectTcg.TcgWeightShuffleSettings.FileName,
        menuName = ConfigScriptableObjectTcg.TcgWeightShuffleSettings.MenuName,
        order    = ConfigScriptableObjectTcg.TcgWeightShuffleSettings.Ordering)]
    public sealed class GGemCoTcgWeightedShuffleSettings : ScriptableObject, ITcgShuffleSettingsAsset
    {
        /// <summary>
        /// FrontLoadedCount(가중치 적용 대상 구간)를 계산/지정하는 옵션입니다.
        /// </summary>
        [Header("FrontLoadedCount")]
        [Tooltip("true: 마나 커브(최대 마나 도달 전까지)로 FrontLoadedCount를 자동 계산")]
        public bool useManaCurveFrontLoadedCount = true;

        /// <summary>
        /// <see cref="useManaCurveFrontLoadedCount"/>가 false일 때 사용하는 고정 FrontLoadedCount입니다.
        /// </summary>
        [Tooltip("useManaCurveFrontLoadedCount가 false일 때 사용하는 고정 FrontLoadedCount")]
        [Min(0)]
        public int fixedFrontLoadedCount = 10;

        /// <summary>
        /// 코스트 가중치 설정(미설정 코스트 기본 가중치 + 코스트별 엔트리 목록)입니다.
        /// </summary>
        [Header("Cost Weights")]
        [Tooltip("설정되지 않은 코스트에 적용될 기본 가중치")]
        [Min(0f)]
        public float defaultCostWeight = 1f;

        /// <summary>
        /// 코스트별 가중치 엔트리 목록입니다. (Unity 직렬화)
        /// </summary>
        [SerializeField]
        private List<CostWeightEntry> entries = new List<CostWeightEntry>();

        /// <summary>
        /// 코스트별 가중치 엔트리 목록(읽기 전용 뷰)입니다.
        /// </summary>
        public IReadOnlyList<CostWeightEntry> Entries => entries;

        /// <summary>
        /// 덱/마나 커브 파라미터를 바탕으로 Weighted 셔플용 <see cref="ShuffleConfig"/>를 생성합니다.
        /// </summary>
        /// <param name="deckSize">덱의 총 카드 수입니다.</param>
        /// <param name="startMana">전투 시작 마나입니다.</param>
        /// <param name="maxMana">최대 마나입니다.</param>
        /// <param name="manaPerTurn">턴당 증가 마나입니다.</param>
        /// <param name="initialDrawCount">초기 드로우 카드 수입니다.</param>
        /// <param name="drawPerTurn">턴당 드로우 카드 수입니다.</param>
        /// <returns>FrontLoadedCount 및 코스트 가중치가 반영된 셔플 설정입니다.</returns>
        /// <remarks>
        /// entries의 유효한 엔트리만 <see cref="ShuffleConfig.CostWeights"/> 딕셔너리에 주입합니다.
        /// (cost는 0 이상, weight는 0 초과)
        /// </remarks>
        public ShuffleConfig BuildShuffleConfig(
            int deckSize,
            int startMana,
            int maxMana,
            int manaPerTurn,
            int initialDrawCount,
            int drawPerTurn)
        {
            var config = new ShuffleConfig
            {
                DefaultCostWeight = Mathf.Max(0f, defaultCostWeight)
            };

            // FrontLoadedCount 결정: 마나 커브 기반 자동 계산 또는 고정 값 사용
            config.FrontLoadedCount = useManaCurveFrontLoadedCount
                ? ShufflePhaseHelper.CalculateFrontLoadedCountByManaCurve(
                    deckSize: deckSize,
                    startMana: startMana,
                    maxMana: maxMana,
                    manaPerTurn: manaPerTurn,
                    initialDrawCount: initialDrawCount,
                    drawPerTurn: drawPerTurn)
                : Mathf.Clamp(fixedFrontLoadedCount, 0, deckSize);

            // 코스트별 가중치 딕셔너리에 주입
            config.CostWeights.Clear();
            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                if (e.cost < 0) continue;
                if (e.weight <= 0f) continue;

                config.CostWeights[e.cost] = e.weight;
            }

            return config;
        }

        /// <summary>
        /// 에디터에서 값 변경 시, 유효 범위를 보정하고 엔트리를 정규화합니다.
        /// </summary>
        private void OnValidate()
        {
            if (defaultCostWeight < 0f) defaultCostWeight = 0f;
            if (fixedFrontLoadedCount < 0) fixedFrontLoadedCount = 0;

            NormalizeEntries();
        }

        /// <summary>
        /// 코스트 엔트리를 정규화합니다. (중복 병합, 음수/0 보정, 정렬)
        /// </summary>
        /// <remarks>
        /// - cost가 음수인 항목은 제거합니다.
        /// - weight가 음수인 항목은 0으로 보정합니다.
        /// - 동일 cost가 중복될 경우 “마지막 값”을 우선하여 병합합니다.
        /// - 마지막에 cost 오름차순으로 정렬합니다.
        /// </remarks>
        private void NormalizeEntries()
        {
            if (entries == null)
                entries = new List<CostWeightEntry>();

            var map = new Dictionary<int, float>(entries.Count);
            for (int i = 0; i < entries.Count; i++)
            {
                int cost = entries[i].cost;
                float w = entries[i].weight;
                if (cost < 0) continue;
                if (w < 0f) w = 0f;

                // 마지막 값 우선
                map[cost] = w;
            }

            entries.Clear();
            foreach (var kv in map)
                entries.Add(new CostWeightEntry { cost = kv.Key, weight = kv.Value });

            entries.Sort((a, b) => a.cost.CompareTo(b.cost));
        }
    }
}
