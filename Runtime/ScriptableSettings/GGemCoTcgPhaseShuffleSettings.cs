using System;
using System.Collections.Generic;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 전투 셔플 설정(초/중/후반 가중치 + 최대 마나 도달 이후 랜덤 유지)을 정의하는 ScriptableObject입니다.
    /// </summary>
    /// <remarks>
    /// - 덱에서 “최대 마나 도달 전까지” 뽑힐 카드 구간(FrontLoadedCount)을 대상으로,
    ///   초/중/후반(Phase)으로 나누어 코스트별 가중치를 적용합니다.
    /// - FrontLoadedCount 이후 구간은 전략에서 생성된 1차 셔플 결과를 그대로 사용하여 “순수 랜덤”을 유지합니다.
    /// </remarks>
    [CreateAssetMenu(
        fileName = ConfigScriptableObjectTcg.TcgPhaseShuffleSettings.FileName,
        menuName = ConfigScriptableObjectTcg.TcgPhaseShuffleSettings.MenuName,
        order    = ConfigScriptableObjectTcg.TcgPhaseShuffleSettings.Ordering)]
    public sealed class GGemCoTcgPhaseShuffleSettings : ScriptableObject, ITcgShuffleSettingsAsset
    {
        [Header("Phase Split (within FrontLoadedCount)")]
        /// <summary>
        /// FrontLoadedCount 구간 중 ‘초반(Early)’에 할당할 비율입니다. (0~1)
        /// </summary>
        [Range(0f, 1f)] public float earlyPhaseRatio = 0.3f;

        /// <summary>
        /// FrontLoadedCount 구간 중 ‘중반(Mid)’에 할당할 비율입니다. (0~1)
        /// </summary>
        [Range(0f, 1f)] public float midPhaseRatio = 0.4f;

        [Header("Cost Weights Per Phase")]
        /// <summary>
        /// 초반(Early) 구간에서 코스트별로 적용할 가중치 테이블입니다.
        /// </summary>
        public PhaseCostWeightTable early = new PhaseCostWeightTable(defaultWeight: 1f);

        /// <summary>
        /// 중반(Mid) 구간에서 코스트별로 적용할 가중치 테이블입니다.
        /// </summary>
        public PhaseCostWeightTable mid   = new PhaseCostWeightTable(defaultWeight: 1f);

        /// <summary>
        /// 후반(Late) 구간에서 코스트별로 적용할 가중치 테이블입니다.
        /// </summary>
        public PhaseCostWeightTable late  = new PhaseCostWeightTable(defaultWeight: 1f);

        /// <summary>
        /// 덱/마나 커브(시작 마나, 턴당 증가, 최대 마나, 드로우 수치)를 바탕으로 셔플 설정(<see cref="ShuffleConfig"/>)을 생성합니다.
        /// </summary>
        /// <param name="deckSize">덱의 총 카드 수입니다.</param>
        /// <param name="startMana">전투 시작 시 마나입니다.</param>
        /// <param name="maxMana">최대 마나입니다.</param>
        /// <param name="manaPerTurn">턴당 증가 마나입니다.</param>
        /// <param name="initialDrawCount">초기 드로우 카드 수입니다.</param>
        /// <param name="drawPerTurn">턴당 드로우 카드 수입니다.</param>
        /// <returns>구간 분할 비율과 코스트 가중치가 반영된 셔플 설정입니다.</returns>
        /// <remarks>
        /// - FrontLoadedCount: 최대 마나 도달 전까지 플레이어가 보게 될(드로우될) 카드 수를 의미합니다.
        /// - FrontLoadedCount 이후 구간은 가중치를 적용하지 않고 기존 셔플 결과를 유지합니다.
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

            // 구간별 기본 가중치 보정(음수 방지)
            config.EarlyPhaseWeights.DefaultWeight = Mathf.Max(0f, early.defaultWeight);
            config.MidPhaseWeights.DefaultWeight = Mathf.Max(0f, mid.defaultWeight);
            config.LatePhaseWeights.DefaultWeight = Mathf.Max(0f, late.defaultWeight);

            // 구간별 코스트 가중치 주입
            early.ApplyTo(config.EarlyPhaseWeights);
            mid.ApplyTo(config.MidPhaseWeights);
            late.ApplyTo(config.LatePhaseWeights);

            return config;
        }

        /// <summary>
        /// 에디터에서 값이 변경될 때, 유효 범위를 보정하고 리스트를 정규화합니다.
        /// </summary>
        /// <remarks>
        /// - 비율 값은 0~1 범위로 클램프합니다.
        /// - 코스트 엔트리는 중복 병합/음수 보정/정렬 등 정리 작업을 수행합니다.
        /// </remarks>
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
    /// 구간(Phase)별 코스트 가중치 테이블입니다. (Unity 직렬화 지원)
    /// </summary>
    /// <remarks>
    /// - <see cref="defaultWeight"/>: 별도 엔트리가 없는 코스트에 적용되는 기본 가중치입니다.
    /// - <see cref="Entries"/>: 코스트별 개별 가중치 엔트리 목록입니다.
    /// </remarks>
    [Serializable]
    public sealed class PhaseCostWeightTable
    {
        /// <summary>
        /// 엔트리가 없는 코스트에 적용되는 기본 가중치입니다. (0 이상)
        /// </summary>
        [Min(0f)] public float defaultWeight = 1f;

        /// <summary>
        /// 코스트별 가중치 엔트리 목록입니다.
        /// </summary>
        [SerializeField] private List<CostWeightEntry> entries = new List<CostWeightEntry>();

        /// <summary>
        /// 기본 가중치를 지정하여 테이블을 생성합니다.
        /// </summary>
        /// <param name="defaultWeight">엔트리가 없는 코스트에 적용될 기본 가중치입니다.</param>
        public PhaseCostWeightTable(float defaultWeight)
        {
            this.defaultWeight = defaultWeight;
        }

        /// <summary>
        /// 직렬화된 코스트 가중치 엔트리 목록(읽기 전용 뷰)입니다.
        /// </summary>
        public IReadOnlyList<CostWeightEntry> Entries => entries;

        /// <summary>
        /// 지정한 대상(<see cref="PhaseCostWeights"/>)에 코스트 가중치를 주입합니다.
        /// </summary>
        /// <param name="target">가중치를 적용할 대상입니다.</param>
        /// <remarks>
        /// 대상은 내부적으로 Dictionary 등을 갖는 구조로 가정하며, SetWeight를 통해 값을 주입합니다.
        /// </remarks>
        public void ApplyTo(PhaseCostWeights target)
        {
            if (target == null) return;

            // target 은 내부 Dictionary 를 갖고 있으므로, 여기서는 SetWeight 로 주입
            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                target.SetWeight(e.cost, e.weight);
            }
        }

        /// <summary>
        /// 에디터에서 안정적으로 편집할 수 있도록 엔트리를 정규화합니다.
        /// </summary>
        /// <remarks>
        /// - entries가 null이면 새 리스트로 복구합니다.
        /// - 동일 cost가 중복될 경우 “마지막 값”을 우선하여 병합합니다.
        /// - weight가 음수이면 0으로 보정합니다.
        /// - cost 오름차순으로 정렬합니다.
        /// </remarks>
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

    /// <summary>
    /// 특정 코스트에 대한 가중치 엔트리입니다.
    /// </summary>
    [Serializable]
    public struct CostWeightEntry
    {
        /// <summary>
        /// 대상 카드 코스트 값입니다. (0 이상)
        /// </summary>
        [Min(0)] public int cost;

        /// <summary>
        /// 해당 코스트에 적용할 가중치 값입니다. (0 이상)
        /// </summary>
        [Min(0f)] public float weight;
    }
}
