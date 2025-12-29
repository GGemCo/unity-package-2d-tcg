using AbilityType = GGemCo2DTcg.TcgAbilityConstants.TcgAbilityType;
using AbilityTargetType = GGemCo2DTcg.TcgAbilityConstants.TcgAbilityTargetType;
using AbilityTriggerType = GGemCo2DTcg.TcgAbilityConstants.TcgAbilityTriggerType;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 테이블 인라인 방식으로 정의되는 Ability의 최소 단위 정의입니다.
    /// 
    /// 각 카드 타입별 상세 테이블
    /// (tcg_card_spell / equipment / permanent / event 등)에
    /// Ability 파라미터를 직접 포함시키는 구조에서 사용됩니다.
    /// </summary>
    public readonly struct TcgAbilityDefinition
    {
        /// <summary>
        /// Localization StringTable에서 Ability 설명을 조회하기 위한 키입니다.
        /// 
        /// 기본 정책:
        /// - 카드 상세 테이블의 Uid 값과 동일한 값을 사용합니다.
        /// </summary>
        public readonly int uid;

        /// <summary>
        /// Ability의 실행 타입(행동 종류)입니다.
        /// </summary>
        public readonly AbilityType abilityType;

        /// <summary>
        /// Ability가 발동되는 트리거 타입입니다.
        /// </summary>
        public readonly AbilityTriggerType tcgAbilityTriggerType;

        /// <summary>
        /// Ability가 적용될 대상 타입입니다.
        /// </summary>
        public readonly AbilityTargetType tcgAbilityTargetType;

        /// <summary>
        /// Ability 실행에 사용되는 첫 번째 정수 파라미터입니다.
        /// (예: 피해량, 회복량, 증가 수치 등)
        /// </summary>
        public readonly int paramA;

        /// <summary>
        /// Ability 실행에 사용되는 두 번째 정수 파라미터입니다.
        /// </summary>
        public readonly int paramB;

        /// <summary>
        /// Ability 실행에 사용되는 세 번째 정수 파라미터입니다.
        /// </summary>
        public readonly int paramC;

        /// <summary>
        /// Ability 정의를 초기화합니다.
        /// </summary>
        /// <param name="uid">
        /// Localization 및 Ability 식별에 사용되는 고유 ID입니다.
        /// </param>
        /// <param name="abilityType">
        /// Ability의 실행 타입입니다.
        /// </param>
        /// <param name="triggerType">
        /// Ability가 발동되는 트리거 타입입니다.
        /// </param>
        /// <param name="targetType">
        /// Ability가 적용될 대상 타입입니다.
        /// </param>
        /// <param name="paramA">Ability 실행에 사용되는 첫 번째 파라미터입니다.</param>
        /// <param name="paramB">Ability 실행에 사용되는 두 번째 파라미터입니다.</param>
        /// <param name="paramC">Ability 실행에 사용되는 세 번째 파라미터입니다.</param>
        public TcgAbilityDefinition(
            int uid,
            AbilityType abilityType,
            AbilityTriggerType triggerType,
            AbilityTargetType targetType,
            int paramA,
            int paramB,
            int paramC)
        {
            this.uid = uid;
            this.abilityType = abilityType;
            this.tcgAbilityTriggerType = triggerType;
            this.tcgAbilityTargetType = targetType;
            this.paramA = paramA;
            this.paramB = paramB;
            this.paramC = paramC;
        }

        /// <summary>
        /// Ability 정의가 유효한지 여부를 반환합니다.
        /// </summary>
        /// <remarks>
        /// - uid는 0보다 커야 합니다.
        /// - abilityType은 <see cref="AbilityType.None"/>이 아니어야 합니다.
        /// </remarks>
        public bool IsValid => uid > 0 && abilityType != AbilityType.None;
    }
}
