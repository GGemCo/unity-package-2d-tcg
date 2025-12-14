using System;
using System.Collections.Generic;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 전투 셔플(초/중/후반 가중치 + 최대 마나 도달 이후 랜덤) 설정 ScriptableObject
    /// </summary>
    [CreateAssetMenu(
        fileName = ConfigScriptableObjectTcg.TcgPhaseShuffleSettings.FileName,
        menuName = ConfigScriptableObjectTcg.TcgPhaseShuffleSettings.MenuName,
        order    = ConfigScriptableObjectTcg.TcgPhaseShuffleSettings.Ordering)]
    public sealed class GGemCoTcgPhaseShuffleSettings : ScriptableObject, ITcgShuffleSettingsAsset
    {
        [Header("Phase Split (within FrontLoadedCount)")]
        [Range(0f, 1f)] public float earlyPhaseRatio = 0.3f;
        [Range(0f, 1f)] public float midPhaseRatio = 0.4f;

        [Header("Cost Weights Per Phase")]
        public PhaseCostWeightTable early = new PhaseCostWeightTable(defaultWeight: 1f);
        public PhaseCostWeightTable mid   = new PhaseCostWeightTable(defaultWeight: 1f);
        public PhaseCostWeightTable late  = new PhaseCostWeightTable(defaultWeight: 1f);

        /// <summary>
        /// 덱 사이즈를 받아 ShuffleConfig 를 생성합니다.
        /// - FrontLoadedCount: 최대 마나 도달 전까지 뽑을 카드 수
        /// - FrontLoadedCount 이후: 순수 랜덤 유지 (전략에서 1차 셔플 결과 그대로)
        /// </summary>
        public ShuffleConfig BuildShuffleConfig(int deckSize, int startMana, int maxMana, int manaPerTurn,
            int initialDrawCount, int drawPerTurn)
        {
            var config = new ShuffleConfig
            {
                FrontLoadedCount = ShufflePhaseHelper.CalculateFrontLoadedCountByManaCurve(
                    deckSize: deckSize,
                    startMana: startMana,
                    maxMana: maxMana,
                    manaPerTurn: manaPerTurn,
                    initialDrawCount: initialDrawCount,
                    drawPerTurn: drawPerTurn),

                EarlyPhaseRatio = earlyPhaseRatio,
                MidPhaseRatio = midPhaseRatio
            };

            // 구간별 코스트 가중치 적용
            config.EarlyPhaseWeights.DefaultWeight = Mathf.Max(0f, early.defaultWeight);
            config.MidPhaseWeights.DefaultWeight = Mathf.Max(0f, mid.defaultWeight);
            config.LatePhaseWeights.DefaultWeight = Mathf.Max(0f, late.defaultWeight);

            early.ApplyTo(config.EarlyPhaseWeights);
            mid.ApplyTo(config.MidPhaseWeights);
            late.ApplyTo(config.LatePhaseWeights);

            return config;
        }

        private void OnValidate()
        {
            // 안전한 값 보정
            earlyPhaseRatio = Mathf.Clamp01(earlyPhaseRatio);
            midPhaseRatio = Mathf.Clamp01(midPhaseRatio);

            // 리스트 정리(중복 cost 병합, 음수 제거 등)
            early?.Normalize();
            mid?.Normalize();
            late?.Normalize();
        }
    }

    /// <summary>
    /// 구간별 코스트 가중치 테이블(직렬화 가능).
    /// </summary>
    [Serializable]
    public sealed class PhaseCostWeightTable
    {
        [Min(0f)] public float defaultWeight = 1f;

        [SerializeField] private List<CostWeightEntry> entries = new List<CostWeightEntry>();

        public PhaseCostWeightTable(float defaultWeight)
        {
            this.defaultWeight = defaultWeight;
        }

        public IReadOnlyList<CostWeightEntry> Entries => entries;

        public void ApplyTo(PhaseCostWeights target)
        {
            if (target == null) return;

            // target 은 내부 Dictionary 를 갖고 있으므로,
            // 여기서는 SetWeight 로 주입
            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                target.SetWeight(e.cost, e.weight);
            }
        }

        /// <summary>
        /// 중복 코스트 병합 / 음수 보정 등 에디터에서 안정적으로 쓰기 위한 정리.
        /// </summary>
        public void Normalize()
        {
            if (entries == null)
                entries = new List<CostWeightEntry>();

            // cost 기준으로 마지막 값을 우선 적용하도록 병합
            var map = new Dictionary<int, float>(entries.Count);
            for (int i = 0; i < entries.Count; i++)
            {
                int cost = entries[i].cost;
                float w = entries[i].weight;
                if (w < 0f) w = 0f;
                map[cost] = w;
            }

            entries.Clear();
            foreach (var kv in map)
                entries.Add(new CostWeightEntry { cost = kv.Key, weight = kv.Value });

            // 보기 좋게 cost 오름차순 정렬
            entries.Sort((a, b) => a.cost.CompareTo(b.cost));
        }
    }

    [Serializable]
    public struct CostWeightEntry
    {
        [Min(0)] public int cost;
        [Min(0f)] public float weight;
    }
}
