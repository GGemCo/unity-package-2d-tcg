using System.Collections.Generic;
using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// Fisher–Yates 알고리즘을 사용하여
    /// 덱 전체를 균등 확률로 섞는 완전 랜덤 셔플 전략입니다.
    /// </summary>
    /// <remarks>
    /// <para>
    /// 모든 가능한 카드 순열이 동일한 확률로 생성되며,
    /// 가중치나 페이즈 개념을 적용하지 않는 기본 셔플 방식입니다.
    /// </para>
    /// <para>
    /// 난수 시드는 <see cref="ShuffleMetaData.SeedManager"/>를 통해 일원화하여 적용됩니다.
    /// </para>
    /// </remarks>
    public class ShuffleStrategyPureRandom : IShuffleStrategy
    {
        /// <summary>
        /// 전달된 카드 리스트를 제자리(in-place)에서 완전 랜덤 셔플합니다.
        /// </summary>
        /// <typeparam name="TCard">셔플 대상 카드 타입입니다.</typeparam>
        /// <param name="cards">셔플할 카드 리스트입니다. 호출 후 순서가 변경됩니다.</param>
        /// <param name="metaData">셔플에 필요한 메타데이터(시드 관리자 등)를 포함합니다.</param>
        /// <remarks>
        /// <para>
        /// 카드 리스트가 <c>null</c>이거나 카드 수가 1 이하인 경우,
        /// 셔플을 수행하지 않고 즉시 반환합니다.
        /// </para>
        /// </remarks>
        public void Shuffle<TCard>(List<TCard> cards, ShuffleMetaData metaData)
            where TCard : GGemCo2DTcg.ICardInfo
        {
            if (cards == null || cards.Count <= 1)
                return;

            // 시드 적용 (SeedManager 를 통해 일원화)
            metaData.SeedManager.ApplySeed();

            int count = cards.Count;
            for (int i = 0; i < count; i++)
            {
                int randomIndex = Random.Range(i, count);
                (cards[i], cards[randomIndex]) = (cards[randomIndex], cards[i]);
            }
        }
    }
}