using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 마나 증가 곡선과 드로우 규칙을 기반으로,
    /// 최대 마나에 도달하기 전까지 드로우될 카드 수(Front-loaded count)를 계산하는 유틸리티입니다.
    /// </summary>
    /// <remarks>
    /// <para>
    /// 이 헬퍼는 셔플/초기 덱 구성 보정과 같이 “초반에 얼마나 많은 카드가 소비(드로우)되는지”를 추정하는 데 사용됩니다.
    /// </para>
    /// </remarks>
    public static class ShufflePhaseHelper
    {
        /// <summary>
        /// 시작 마나에서 최대 마나(<paramref name="maxMana"/>)에 도달하기까지 필요한 턴 수를 계산합니다.
        /// </summary>
        /// <param name="startMana">전투 시작 시점의 현재 마나입니다.</param>
        /// <param name="maxMana">전투 중 도달 가능한 최대 마나입니다.</param>
        /// <param name="manaPerTurn">각 턴 종료 시 증가하는 마나입니다.</param>
        /// <returns>
        /// 최대 마나에 도달하기까지 필요한 턴 수를 반환합니다.
        /// <para>NOTE: <paramref name="maxMana"/>가 이미 <paramref name="startMana"/> 이하인 경우에도 최소 1을 반환합니다.</para>
        /// </returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// <paramref name="manaPerTurn"/>가 0 이하인 경우 발생합니다.
        /// </exception>
        /// <remarks>
        /// <para><b>가정</b></para>
        /// <list type="bullet">
        ///   <item><description>전투 시작 시 <paramref name="startMana"/>로 시작합니다.</description></item>
        ///   <item><description>각 턴 종료 시 <paramref name="manaPerTurn"/>만큼 증가합니다.</description></item>
        ///   <item><description>마나는 <paramref name="maxMana"/>를 초과하여 증가하지 않습니다.</description></item>
        /// </list>
        /// <para>
        /// 구현상 “턴 종료 후 마나 증가”를 1회로 계산하며, 반환값은 최소 1로 보정됩니다.
        /// </para>
        /// </remarks>
        public static int CalculateTurnsToReachMaxMana(
            int startMana,
            int maxMana,
            int manaPerTurn)
        {
            if (manaPerTurn <= 0)
                throw new System.ArgumentOutOfRangeException(nameof(manaPerTurn));

            if (maxMana <= startMana)
                return 1;

            int currentMana = startMana;
            int turns = 0;

            while (currentMana < maxMana)
            {
                // 턴 종료 후 마나 증가
                currentMana = Mathf.Min(maxMana, currentMana + manaPerTurn);
                turns++;
            }

            return Mathf.Max(1, turns);
        }

        /// <summary>
        /// 최대 마나에 도달할 때까지 총 드로우되는 카드 수를 계산합니다.
        /// </summary>
        /// <param name="deckSize">현재 덱의 카드 수입니다.</param>
        /// <param name="initialDrawCount">전투 시작 시 한 번에 드로우하는 카드 수입니다.</param>
        /// <param name="drawPerTurn">각 턴마다 추가로 드로우하는 카드 수입니다.</param>
        /// <param name="turnsToReachMaxMana">최대 마나에 도달하는 데 필요한 턴 수입니다.</param>
        /// <returns>
        /// 최대 마나에 도달하기 전까지 드로우되는 카드 수를 반환합니다.
        /// 덱 크기를 초과하지 않도록 <paramref name="deckSize"/>로 상한 처리됩니다.
        /// </returns>
        /// <remarks>
        /// <para><b>가정</b></para>
        /// <list type="bullet">
        ///   <item><description>전투 시작 시 <paramref name="initialDrawCount"/>장을 먼저 드로우합니다.</description></item>
        ///   <item><description>이후 각 턴마다 <paramref name="drawPerTurn"/>장씩 추가 드로우합니다.</description></item>
        ///   <item><description>총 드로우 수는 덱 크기(<paramref name="deckSize"/>)를 초과하지 않습니다.</description></item>
        /// </list>
        /// <para>
        /// 음수 입력은 내부에서 0으로 보정되며, <paramref name="deckSize"/>가 0 이하인 경우 0을 반환합니다.
        /// </para>
        /// </remarks>
        public static int CalculateDrawCountUntilMaxMana(
            int deckSize,
            int initialDrawCount,
            int drawPerTurn,
            int turnsToReachMaxMana)
        {
            if (deckSize <= 0)
                return 0;

            initialDrawCount = Mathf.Max(0, initialDrawCount);
            drawPerTurn = Mathf.Max(0, drawPerTurn);

            int drawCount = Mathf.Clamp(initialDrawCount, 0, deckSize);

            // 최대 마나에 도달하는 턴까지 추가로 뽑는 카드 수
            int additionalDraws = drawPerTurn * turnsToReachMaxMana;
            drawCount = Mathf.Min(deckSize, drawCount + additionalDraws);

            return drawCount;
        }

        /// <summary>
        /// 전투 설정을 기반으로,
        /// 최대 마나에 도달하기 전까지 드로우될 카드 수(Front-loaded count)를 한 번에 계산합니다.
        /// </summary>
        /// <param name="deckSize">현재 덱의 카드 수입니다.</param>
        /// <param name="startMana">전투 시작 시점의 현재 마나입니다.</param>
        /// <param name="maxMana">전투 중 도달 가능한 최대 마나입니다.</param>
        /// <param name="manaPerTurn">각 턴 종료 시 증가하는 마나입니다.</param>
        /// <param name="initialDrawCount">전투 시작 시 한 번에 드로우하는 카드 수입니다.</param>
        /// <param name="drawPerTurn">각 턴마다 추가로 드로우하는 카드 수입니다.</param>
        /// <returns>최대 마나에 도달하기 전까지 드로우될 카드 수입니다.</returns>
        /// <remarks>
        /// 계산 결과는 셔플 설정의 <c>ShuffleConfig.FrontLoadedCount</c> 같은 값에 반영하여 사용할 수 있습니다.
        /// </remarks>
        public static int CalculateFrontLoadedCountByManaCurve(
            int deckSize,
            int startMana,
            int maxMana,
            int manaPerTurn,
            int initialDrawCount,
            int drawPerTurn)
        {
            int turnsToMaxMana = CalculateTurnsToReachMaxMana(
                startMana,
                maxMana,
                manaPerTurn);

            return CalculateDrawCountUntilMaxMana(
                deckSize,
                initialDrawCount,
                drawPerTurn,
                turnsToMaxMana);
        }
    }
}
