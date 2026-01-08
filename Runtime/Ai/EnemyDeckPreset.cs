using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// AI 덱 프리셋을 분류하기 위한 난이도 태그입니다.
    /// </summary>
    /// <remarks>
    /// 실제 게임 밸런스 로직과 별개로, 프리셋을 그룹핑/선택하는 용도로 사용할 수 있습니다.
    /// </remarks>
    public enum AiDeckDifficulty
    {
        Easy,
        Normal,
        Hard,
        Custom
    }

    /// <summary>
    /// 특정 카드 Uid를 덱에 고정 포함시키는 규칙(최소/최대 장수)을 정의합니다.
    /// </summary>
    /// <remarks>
    /// 예: 카드 1001은 최소 1장, 최대 2장 포함.
    /// </remarks>
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
    /// 카드 테이블에서 필터 조건에 맞는 후보를 찾고, 그 중 일부를 랜덤 선택해 덱에 추가하는 규칙입니다.
    /// </summary>
    /// <remarks>
    /// 예: Creature / Common / Cost 1~3 카드 중에서 10장 뽑기.
    /// </remarks>
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
        public int minCost;

        [Tooltip("최대 코스트")]
        public int maxCost = 10;

        [Header("선택 규칙")]
        [Tooltip("이 규칙으로 덱에 채울 카드 장수")]
        [Min(0)]
        public int count;

        [Tooltip("true이면 같은 카드를 중복 선택 허용, false이면 후보 카드에서 중복 선택하지 않음")]
        public bool allowDuplicateSameCard = true;
    }

    /// <summary>
    /// AI용 덱 프리셋을 정의하는 ScriptableObject입니다.
    /// </summary>
    /// <remarks>
    /// - 에디터에서 고정 카드 규칙/필터 규칙/덱 크기 등을 설정합니다.
    /// - 런타임에는 카드 테이블(<see cref="StruckTableTcgCard"/>)을 입력으로 받아 덱 Uid 목록을 생성합니다.
    /// </remarks>
    [CreateAssetMenu(
        fileName = ConfigScriptableObjectTcg.TcgAiPreset.FileName,
        menuName = ConfigScriptableObjectTcg.TcgAiPreset.MenuName,
        order    = ConfigScriptableObjectTcg.TcgAiPreset.Ordering)]
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
        public int heroCardUid;

        [Tooltip("여러 카드를 넣어놓고 랜덤하게 선택하게 합니다. 고정 카드 Uid가 우선순위가 높습니다.")]
        public List<int> heroCardUids = new List<int>();

        [Header("고정 카드 규칙")]
        [Tooltip("특정 카드 Uid를 미리 지정해 최소/최대 장수로 넣는 규칙")]
        public List<AiDeckFixedCardRule> fixedCardRules = new List<AiDeckFixedCardRule>();

        [Header("필터 기반 랜덤 카드 규칙")]
        [Tooltip("타입/등급/코스트 기준으로 랜덤 선택할 카드 규칙")]
        public List<AiDeckFilterRule> filterRules = new List<AiDeckFilterRule>();

        /// <summary>
        /// AI 덱 생성 시 사용할 난수 시드 설정 모드입니다.
        /// </summary>
        /// <remarks>
        /// - <see cref="None"/>: 외부에서 <see cref="System.Random"/>을 주입받아 사용할 때 선택합니다.
        /// - <see cref="UseFixedSeed"/>: 테스트/재현 목적의 고정 시드를 사용합니다.
        /// </remarks>
        public enum RandomSeedMode
        {
            None,
            UseFixedSeed
        }

        [Header("랜덤 시드 설정")]
        public RandomSeedMode randomSeedMode = RandomSeedMode.None;

        [Tooltip("RandomSeedMode가 UseFixedSeed일 때 사용할 시드 값")]
        public int fixedSeed;

        /// <summary>
        /// 이 프리셋 설정을 기반으로 덱에 포함될 카드 Uid 목록을 생성합니다.
        /// </summary>
        /// <param name="cardTableRows">카드 테이블(카드 UID → 카드 행)입니다.</param>
        /// <param name="externalRandom">
        /// 외부에서 주입하는 난수 생성기입니다.
        /// <see cref="randomSeedMode"/>가 <see cref="RandomSeedMode.UseFixedSeed"/>이면 무시될 수 있습니다.
        /// </param>
        /// <returns>덱에 포함될 카드 Uid 리스트입니다.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="cardTableRows"/>가 null인 경우 발생합니다.</exception>
        public List<int> BuildDeckUids(
            IReadOnlyDictionary<int, StruckTableTcgCard> cardTableRows,
            System.Random externalRandom = null)
        {
            if (cardTableRows == null)
                throw new ArgumentNullException(nameof(cardTableRows));

            System.Random rng = CreateRandom(externalRandom);

            // 결과 덱(카드 Uid)
            List<int> deck = new List<int>(deckSize);

            ApplyFixedCardRules(cardTableRows, deck, rng);
            ApplyFilterRules(cardTableRows, deck, rng);

            if (deck.Count < deckSize)
            {
                FillRemainingWithAny(cardTableRows, deck, rng);
            }

            if (deck.Count > deckSize)
            {
                deck = deck.Take(deckSize).ToList();
            }

            return deck;
        }

        /// <summary>
        /// 시드 설정 모드에 따라 <see cref="System.Random"/>을 생성하거나, 외부 주입 인스턴스를 사용합니다.
        /// </summary>
        /// <param name="externalRandom">외부에서 전달된 난수 생성기입니다.</param>
        /// <returns>덱 생성에 사용할 난수 생성기 인스턴스입니다.</returns>
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
        /// 고정 카드 규칙(<see cref="fixedCardRules"/>)을 적용하여 덱 리스트에 카드 Uid를 추가합니다.
        /// </summary>
        /// <param name="cardTableRows">카드 테이블(카드 UID → 카드 행)입니다.</param>
        /// <param name="deck">덱에 포함될 카드 Uid 리스트(누적 대상)입니다.</param>
        /// <param name="rng">난수 생성기입니다.</param>
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

                // min~max 중 실제로 몇 장 넣을지 결정
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
        /// 필터 기반 규칙(<see cref="filterRules"/>)을 적용하여 덱 리스트에 카드 Uid를 추가합니다.
        /// </summary>
        /// <param name="cardTableRows">카드 테이블(카드 UID → 카드 행)입니다.</param>
        /// <param name="deck">덱에 포함될 카드 Uid 리스트(누적 대상)입니다.</param>
        /// <param name="rng">난수 생성기입니다.</param>
        /// <remarks>
        /// 규칙마다 필터에 부합하는 후보를 구성한 뒤,
        /// 중복 허용 여부(<see cref="AiDeckFilterRule.allowDuplicateSameCard"/>)에 따라
        /// 랜덤 선택 또는 셔플 후 상위 N개를 채택합니다.
        /// </remarks>
        private void ApplyFilterRules(
            IReadOnlyDictionary<int, StruckTableTcgCard> cardTableRows,
            List<int> deck,
            System.Random rng)
        {
            if (filterRules == null || filterRules.Count == 0)
                return;

            List<StruckTableTcgCard> allCards = cardTableRows.Values.ToList();

            foreach (var rule in filterRules)
            {
                if (rule.count <= 0)
                    continue;

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
        /// 카드 행이 특정 필터 규칙에 부합하는지 판정합니다.
        /// </summary>
        /// <param name="row">검사할 카드 행 데이터입니다.</param>
        /// <param name="rule">적용할 필터 규칙입니다.</param>
        /// <returns>조건을 모두 만족하면 true, 하나라도 불만족하면 false를 반환합니다.</returns>
        private bool IsMatchFilter(StruckTableTcgCard row, AiDeckFilterRule rule)
        {
            if (rule.type != CardConstants.Type.Any && row.type != rule.type)
                return false;

            if (row.grade < rule.minGrade)
                return false;

            if (row.grade > rule.maxGrade)
                return false;

            if (row.cost < rule.minCost)
                return false;

            if (row.cost > rule.maxCost)
                return false;

            return true;
        }

        /// <summary>
        /// 덱이 목표 크기보다 작은 경우 남은 슬롯을 전체 카드 풀에서 랜덤으로 채웁니다.
        /// </summary>
        /// <param name="cardTableRows">카드 테이블(카드 UID → 카드 행)입니다.</param>
        /// <param name="deck">덱에 포함될 카드 Uid 리스트(누적 대상)입니다.</param>
        /// <param name="rng">난수 생성기입니다.</param>
        /// <remarks>
        /// 필터/고정 규칙만으로 덱이 완성되지 않을 때의 기본 보충 로직입니다.
        /// 필요에 따라 호출 자체를 제거하거나, 별도 옵션으로 제어할 수 있습니다.
        /// </remarks>
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
        /// 리스트를 Fisher–Yates 방식으로 제자리 셔플합니다.
        /// </summary>
        /// <typeparam name="T">셔플 대상 요소 타입입니다.</typeparam>
        /// <param name="list">셔플할 리스트입니다.</param>
        /// <param name="rng">난수 생성기입니다.</param>
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
