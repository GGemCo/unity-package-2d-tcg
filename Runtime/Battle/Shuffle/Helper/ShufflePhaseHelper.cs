using UnityEngine;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 마나/드로우 곡선을 기반으로
    /// "최대 마나를 얻기 전까지 뽑을 카드 수"를 계산하는 헬퍼.
    /// </summary>
    public static class ShufflePhaseHelper
    {
        /// <summary>
        /// 최대 마나에 도달하기까지 필요한 턴 수를 계산합니다.
        /// 
        /// 가정:
        /// - 전투 시작 시 startMana 로 시작
        /// - 각 턴 종료 시 manaPerTurn 만큼 증가
        /// - maxMana 까지만 증가
        /// </summary>
        /// <param name="startMana">전투 시작 시 현재 마나.</param>
        /// <param name="maxMana">전투 중 얻을 수 있는 최대 마나.</param>
        /// <param name="manaPerTurn">턴 종료 시 증가하는 마나.</param>
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
        /// 최대 마나에 도달할 때까지 총 몇 장의 카드를 뽑는지 계산합니다.
        /// 
        /// 가정:
        /// - 전투 시작 시 initialDrawCount 장을 먼저 드로우
        /// - 이후 각 턴마다 drawPerTurn 장씩 드로우
        /// - 덱 사이즈를 넘어가지는 않음
        /// </summary>
        /// <param name="deckSize">현재 덱 카드 수.</param>
        /// <param name="initialDrawCount">게임 시작 시 한 번에 뽑는 카드 수.</param>
        /// <param name="drawPerTurn">각 턴마다 추가로 뽑는 카드 수.</param>
        /// <param name="turnsToReachMaxMana">최대 마나에 도달하는 데 필요한 턴 수.</param>
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
        /// "최대 마나를 얻기 전까지 뽑을 카드 수"를 한 번에 계산합니다.
        /// 이 값은 ShuffleConfig.FrontLoadedCount 로 설정해서 사용합니다.
        /// </summary>
        /// <param name="deckSize">현재 덱 카드 수.</param>
        /// <param name="startMana">전투 시작 시 현재 마나.</param>
        /// <param name="maxMana">전투 중 얻을 수 있는 최대 마나.</param>
        /// <param name="manaPerTurn">턴 종료 시 증가하는 마나.</param>
        /// <param name="initialDrawCount">게임 시작 시 한 번에 뽑는 카드 수.</param>
        /// <param name="drawPerTurn">각 턴마다 추가로 뽑는 카드 수.</param>
        /// <returns>최대 마나를 얻기 전까지 드로우될 카드 수.</returns>
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
