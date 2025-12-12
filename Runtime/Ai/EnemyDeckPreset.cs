using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// AI 덱 난이도 구분용 enum.
    /// 필요에 따라 확장 가능합니다.
    /// </summary>
    public enum AiDeckDifficulty
    {
        Easy,
        Normal,
        Hard,
        Custom
    }

    /// <summary>
    /// 특정 카드 Uid를 기반으로 고정적으로 덱에 포함시키는 규칙.
    /// 예: 카드 1001은 최소 1장, 최대 2장 포함.
    /// </summary>
    [Serializable]
    public class AiDeckFixedCardRule
    {
        [Tooltip("TableTcgCard의 Uid 값")]
        public int cardUid;

        [Tooltip("이 카드가 덱에 최소 몇 장 포함되어야 하는지")]
        [Min(0)]
        public int minCopies = 1;

        [Tooltip("이 카드가 덱에 최대 몇 장 포함될 수 있는지")]
        [Min(0)]
        public int maxCopies = 2;
    }

    /// <summary>
    /// 필터 조건에 맞는 카드들 중에서 랜덤으로 골라 덱에 추가하는 규칙.
    /// 예: Creature / Common / Cost 1~3 카드 중에서 10장 뽑기.
    /// </summary>
    [Serializable]
    public class AiDeckFilterRule
    {
        [Header("기본 필터")]
        [Tooltip("카드 타입 필터. AnyType이면 타입을 필터링하지 않습니다.")]
        public CardConstants.Type type = CardConstants.Type.Any;

        [Tooltip("최소 등급. (예: Common 이상)")]
        public CardConstants.Grade minGrade = CardConstants.Grade.Common;

        [Tooltip("최대 등급. (예: Epic 이하)")]
        public CardConstants.Grade maxGrade = CardConstants.Grade.Legendary;

        [Header("코스트 범위 필터")]
        [Tooltip("최소 코스트")]
        public int minCost = 0;

        [Tooltip("최대 코스트")]
        public int maxCost = 10;

        [Header("선택 규칙")]
        [Tooltip("이 규칙으로 덱에 채울 카드 장수")]
        [Min(0)]
        public int count = 0;

        [Tooltip("true이면 같은 카드를 중복 선택 허용, false이면 후보 카드에서 중복 선택하지 않음")]
        public bool allowDuplicateSameCard = true;
    }

    /// <summary>
    /// AI용 덱 프리셋 ScriptableObject.
    /// - 에디터에서 AI 덱 구성을 설정합니다.
    /// - 런타임에서는 카드 테이블을 기반으로 실제 덱 리스트를 생성합니다.
    /// </summary>
    [CreateAssetMenu(
        fileName = "AiDeckPreset",
        menuName = "GGemCo/TCG/AI Deck Preset",
        order = 1000)]
    public class EnemyDeckPreset : ScriptableObject
    {
        [Header("기본 정보")]
        [Tooltip("이 AI 덱의 ID 또는 키 값. (저장/로딩, 로깅 용도)")]
        public string presetId;

        [Tooltip("에디터용, 디버깅용 표시 이름")]
        public string displayName;

        [Tooltip("AI 난이도 태그. 실제 로직과는 별도이지만, 난이도별로 프리셋을 분류하는 데 사용합니다.")]
        public AiDeckDifficulty difficulty = AiDeckDifficulty.Normal;

        [Header("덱 크기")]
        [Tooltip("목표 덱 카드 수. 예: 25, 30, 40 등")]
        [Min(1)]
        public int deckSize = 25;

        [Header("영웅 카드")]
        [Tooltip("영웅으로 사용할 고정 카드 Uid")]
        public int heroCardUid = 0;
        [Tooltip("여러 카드를 넣어놓고 랜덤하게 선택하게 합니다. 고정 카드 Uid가 우선순위가 높습니다.")]
        public List<int> heroCardUids = new List<int>();
        
        [Header("고정 카드 규칙")]
        [Tooltip("특정 카드 Uid를 미리 지정해 최소/최대 장수로 넣는 규칙")]
        public List<AiDeckFixedCardRule> fixedCardRules = new List<AiDeckFixedCardRule>();

        [Header("필터 기반 랜덤 카드 규칙")]
        [Tooltip("타입/등급/코스트 기준으로 랜덤 선택할 카드 규칙")]
        public List<AiDeckFilterRule> filterRules = new List<AiDeckFilterRule>();

        /// <summary>
        /// AI 덱을 구성할 때 사용할 랜덤 시드 모드.
        /// - None: 외부에서 Random을 주입받을 때 사용.
        /// - UseFixedSeed: 테스트/재현용 고정 시드 사용.
        /// </summary>
        public enum RandomSeedMode
        {
            None,
            UseFixedSeed
        }

        [Header("랜덤 시드 설정")]
        public RandomSeedMode randomSeedMode = RandomSeedMode.None;

        [Tooltip("RandomSeedMode가 UseFixedSeed일 때 사용할 시드 값")]
        public int fixedSeed = 0;

        /// <summary>
        /// 이 프리셋을 기반으로 덱 카드 Uid 리스트를 생성합니다.
        /// - cardTableRows는 TableTcgCard의 전체 행(카드 목록)을 전달하면 됩니다.
        /// - DeckRuntime으로의 변환은 호출 측에서 담당하도록 분리했습니다.
        /// </summary>
        /// <param name="cardTableRows">카드 테이블의 전체 카드 목록</param>
        /// <param name="externalRandom">
        /// 외부에서 주입하는 Random 인스턴스.
        /// randomSeedMode가 UseFixedSeed인 경우 이 값은 무시될 수 있습니다.
        /// </param>
        /// <returns>덱에 포함될 카드 Uid 리스트</returns>
        public List<int> BuildDeckUids(
            IReadOnlyDictionary<int, StruckTableTcgCard> cardTableRows,
            System.Random externalRandom = null)
        {
            if (cardTableRows == null)
                throw new ArgumentNullException(nameof(cardTableRows));

            // Random 설정
            System.Random rng = CreateRandom(externalRandom);

            // 결과 덱 리스트 (카드 Uid)
            List<int> deck = new List<int>(deckSize);

            // 1. 고정 카드 규칙 적용
            ApplyFixedCardRules(cardTableRows, deck, rng);

            // 2. 필터 기반 규칙 적용
            ApplyFilterRules(cardTableRows, deck, rng);

            // 3. 아직 덱이 모자라면, 전체 카드에서 랜덤으로 채우는 옵션
            if (deck.Count < deckSize)
            {
                FillRemainingWithAny(cardTableRows, deck, rng);
            }

            // 4. 덱 크기 초과 시 잘라내기
            if (deck.Count > deckSize)
            {
                // 단순히 앞에서부터 잘라내지만, 필요하다면 추가 로직으로 교체 가능
                deck = deck.Take(deckSize).ToList();
            }

            return deck;
        }

        /// <summary>
        /// Random 인스턴스를 생성하거나 외부에서 주입받은 인스턴스를 사용합니다.
        /// </summary>
        private System.Random CreateRandom(System.Random externalRandom)
        {
            switch (randomSeedMode)
            {
                case RandomSeedMode.UseFixedSeed:
                    return new System.Random(fixedSeed);
                case RandomSeedMode.None:
                default:
                    return externalRandom ?? new System.Random();
            }
        }

        /// <summary>
        /// 고정 카드 규칙을 적용하여 덱 리스트에 카드 Uid를 추가합니다.
        /// </summary>
        private void ApplyFixedCardRules(
            IReadOnlyDictionary<int, StruckTableTcgCard> cardTableRows,
            List<int> deck,
            System.Random rng)
        {
            if (fixedCardRules == null || fixedCardRules.Count == 0)
                return;

            foreach (var rule in fixedCardRules)
            {
                if (!cardTableRows.ContainsKey(rule.cardUid))
                {
                    Debug.LogWarning(
                        $"[AiDeckPreset] FixedCardRule의 cardUid({rule.cardUid})가 카드 테이블에 없습니다. presetId={presetId}");
                    continue;
                }

                int min = Mathf.Max(0, rule.minCopies);
                int max = Mathf.Max(min, rule.maxCopies);
                // min~max 중 실제로 몇 장 넣을지 랜덤 결정
                int count = rng.Next(min, max + 1);
                for (int i = 0; i < count; i++)
                {
                    if (deck.Count >= deckSize)
                        return;

                    deck.Add(rule.cardUid);
                }
            }
        }

        /// <summary>
        /// 필터 기반 랜덤 카드 규칙을 적용합니다.
        /// 각 규칙마다 조건에 맞는 카드 목록을 구한 뒤, 해당 목록에서 랜덤으로 선택하여 덱에 추가합니다.
        /// </summary>
        private void ApplyFilterRules(
            IReadOnlyDictionary<int, StruckTableTcgCard> cardTableRows,
            List<int> deck,
            System.Random rng)
        {
            if (filterRules == null || filterRules.Count == 0)
                return;

            // 미리 전체 리스트로 캐싱
            List<StruckTableTcgCard> allCards = cardTableRows.Values.ToList();

            foreach (var rule in filterRules)
            {
                if (rule.count <= 0)
                    continue;

                // 필터 조건에 맞는 후보 카드 목록 구축
                List<StruckTableTcgCard> candidates = allCards
                    .Where(row => IsMatchFilter(row, rule))
                    .ToList();

                if (candidates.Count == 0)
                {
                    Debug.LogWarning(
                        $"[AiDeckPreset] FilterRule에 해당하는 카드가 없습니다. presetId={presetId}, rule={rule.type}, cost[{rule.minCost}-{rule.maxCost}]");
                    continue;
                }

                if (rule.allowDuplicateSameCard)
                {
                    // 같은 카드를 여러 번 뽑는 것을 허용
                    for (int i = 0; i < rule.count; i++)
                    {
                        if (deck.Count >= deckSize)
                            return;

                        int index = rng.Next(0, candidates.Count);
                        deck.Add(candidates[index].uid);
                    }
                }
                else
                {
                    // 같은 카드를 한 번만 뽑도록 제한
                    // 후보를 섞은 뒤 상위 count개 사용 (또는 candidates.Count까지)
                    ShuffleInPlace(candidates, rng);

                    int takeCount = Mathf.Min(rule.count, candidates.Count);
                    for (int i = 0; i < takeCount; i++)
                    {
                        if (deck.Count >= deckSize)
                            return;

                        deck.Add(candidates[i].uid);
                    }
                }
            }
        }

        /// <summary>
        /// 필터 조건에 카드가 부합하는지 검사합니다.
        /// </summary>
        private bool IsMatchFilter(StruckTableTcgCard row, AiDeckFilterRule rule)
        {
            // 타입 필터: Any면 통과
            if (rule.type != CardConstants.Type.Any && row.type != rule.type)
                return false;

            // 등급 필터
            if (row.grade < rule.minGrade)
                return false;
            if (row.grade > rule.maxGrade)
                return false;

            // 코스트 필터
            if (row.cost < rule.minCost)
                return false;
            if (row.cost > rule.maxCost)
                return false;

            return true;
        }

        /// <summary>
        /// 덱이 목표 크기보다 작은 경우, 남은 칸을 전체 카드 목록에서 랜덤으로 채우는 기본 보충 로직.
        /// 필요 없으면 이 메서드를 비활성화하거나, 호출부에서 옵션으로 제어할 수 있습니다.
        /// </summary>
        private void FillRemainingWithAny(
            IReadOnlyDictionary<int, StruckTableTcgCard> cardTableRows,
            List<int> deck,
            System.Random rng)
        {
            List<StruckTableTcgCard> allCards = cardTableRows.Values.ToList();
            if (allCards.Count == 0)
                return;

            while (deck.Count < deckSize)
            {
                int index = rng.Next(0, allCards.Count);
                deck.Add(allCards[index].uid);
            }
        }

        /// <summary>
        /// Fisher–Yates 셔플.
        /// </summary>
        private void ShuffleInPlace<T>(IList<T> list, System.Random rng)
        {
            int n = list.Count;
            for (int i = n - 1; i > 0; i--)
            {
                int j = rng.Next(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
