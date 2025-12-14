using System.Collections.Generic;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// Weighted 셔플용 설정 ScriptableObject.
    ///
    /// - WeightedShuffleStrategy에서 사용하는 ShuffleConfig(CostWeights, FrontLoadedCount, DefaultCostWeight)를 생성합니다.
    /// - FrontLoadedCount는 기본적으로 "마나 커브 기반"(최대 마나 도달 전까지)으로 계산할 수 있고,
    ///   필요 시 고정 값으로도 지정할 수 있습니다.
    /// </summary>
    [CreateAssetMenu(
        fileName = ConfigScriptableObjectTcg.TcgWeightShuffleSettings.FileName,
        menuName = ConfigScriptableObjectTcg.TcgWeightShuffleSettings.MenuName,
        order    = ConfigScriptableObjectTcg.TcgWeightShuffleSettings.Ordering)]
    public sealed class GGemCoTcgWeightedShuffleSettings : ScriptableObject, ITcgShuffleSettingsAsset
    {
        [Header("FrontLoadedCount")]
        [Tooltip("true: 마나 커브(최대 마나 도달 전까지)로 FrontLoadedCount를 자동 계산")]
        public bool useManaCurveFrontLoadedCount = true;

        [Tooltip("useManaCurveFrontLoadedCount가 false일 때 사용하는 고정 FrontLoadedCount")]
        [Min(0)]
        public int fixedFrontLoadedCount = 10;

        [Header("Cost Weights")]
        [Tooltip("설정되지 않은 코스트에 적용될 기본 가중치")]
        [Min(0f)]
        public float defaultCostWeight = 1f;

        [SerializeField]
        private List<CostWeightEntry> entries = new List<CostWeightEntry>();

        public IReadOnlyList<CostWeightEntry> Entries => entries;

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

            config.FrontLoadedCount = useManaCurveFrontLoadedCount
                ? ShufflePhaseHelper.CalculateFrontLoadedCountByManaCurve(
                    deckSize: deckSize,
                    startMana: startMana,
                    maxMana: maxMana,
                    manaPerTurn: manaPerTurn,
                    initialDrawCount: initialDrawCount,
                    drawPerTurn: drawPerTurn)
                : Mathf.Clamp(fixedFrontLoadedCount, 0, deckSize);

            // 딕셔너리에 주입
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

        private void OnValidate()
        {
            if (defaultCostWeight < 0f) defaultCostWeight = 0f;
            if (fixedFrontLoadedCount < 0) fixedFrontLoadedCount = 0;

            NormalizeEntries();
        }

        /// <summary>
        /// 중복 cost 병합, 음수/0 보정, 정렬.
        /// </summary>
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
