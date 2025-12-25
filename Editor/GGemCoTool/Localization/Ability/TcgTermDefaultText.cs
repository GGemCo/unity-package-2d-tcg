using GGemCo2DTcg;

#if UNITY_EDITOR
namespace GGemCo2DTcgEditor
{
    /// <summary>
    /// Term 기본값(초기 생성용).
    /// - 운영 단계에서는 번역/표현 품질을 위해 테이블에서 직접 수정하세요.
    /// </summary>
    public static class TcgTermDefaultText
    {
        public static string KoTriggerPrefix(TcgAbilityConstants.TcgAbilityTriggerType tcgAbilityTriggerType)
        {
            return tcgAbilityTriggerType switch
            {
                TcgAbilityConstants.TcgAbilityTriggerType.OnPlay => "사용 시 ",
                TcgAbilityConstants.TcgAbilityTriggerType.OnTurnStart => "턴 시작마다 ",
                TcgAbilityConstants.TcgAbilityTriggerType.OnTurnEnd => "턴 종료마다 ",
                _ => ""
            };
        }

        public static string EnTriggerPrefix(TcgAbilityConstants.TcgAbilityTriggerType tcgAbilityTriggerType)
        {
            return tcgAbilityTriggerType switch
            {
                TcgAbilityConstants.TcgAbilityTriggerType.OnPlay => "When played, ",
                TcgAbilityConstants.TcgAbilityTriggerType.OnTurnStart => "At the start of the turn, ",
                TcgAbilityConstants.TcgAbilityTriggerType.OnTurnEnd => "At the end of the turn, ",
                _ => ""
            };
        }

        public static string KoTargetTo(TcgAbilityConstants.TcgAbilityTargetType tcgAbilityTargetType)
        {
            return tcgAbilityTargetType switch
            {
                TcgAbilityConstants.TcgAbilityTargetType.EnemyHero => "적 영웅에게",
                TcgAbilityConstants.TcgAbilityTargetType.AllyHero => "아군 영웅에게",
                _ => "대상에게"
            };
        }

        public static string EnTargetTo(TcgAbilityConstants.TcgAbilityTargetType tcgAbilityTargetType)
        {
            return tcgAbilityTargetType switch
            {
                TcgAbilityConstants.TcgAbilityTargetType.EnemyHero => "the enemy hero",
                TcgAbilityConstants.TcgAbilityTargetType.AllyHero => "your hero",
                TcgAbilityConstants.TcgAbilityTargetType.EnemyCreature => "an enemy creature",
                TcgAbilityConstants.TcgAbilityTargetType.AllyCreature => "an allied creature",
                TcgAbilityConstants.TcgAbilityTargetType.AnyCreature => "a creature",
                _ => "the target"
            };
        }

        public static string KoTargetObj(TcgAbilityConstants.TcgAbilityTargetType tcgAbilityTargetType)
        {
            return tcgAbilityTargetType switch
            {
                TcgAbilityConstants.TcgAbilityTargetType.EnemyHero => "적 영웅을",
                TcgAbilityConstants.TcgAbilityTargetType.AllyHero => "아군 영웅을",
                TcgAbilityConstants.TcgAbilityTargetType.EnemyCreature => "적 크리처 1체를",
                TcgAbilityConstants.TcgAbilityTargetType.AllyCreature => "아군 크리처 1체를",
                TcgAbilityConstants.TcgAbilityTargetType.AnyCreature => "대상 크리처 1체를",
                _ => "대상을"
            };
        }

        public static string EnTargetObj(TcgAbilityConstants.TcgAbilityTargetType tcgAbilityTargetType)
        {
            // 영어는 목적격 변화가 거의 없으므로 to와 동일하게 처리해도 무방합니다.
            return EnTargetTo(tcgAbilityTargetType);
        }

        public static string KoTargetNoun1(TcgAbilityConstants.TcgAbilityTargetType tcgAbilityTargetType)
        {
            return tcgAbilityTargetType switch
            {
                TcgAbilityConstants.TcgAbilityTargetType.EnemyCreature => "적 크리처 1체",
                TcgAbilityConstants.TcgAbilityTargetType.AllyCreature => "아군 크리처 1체",
                TcgAbilityConstants.TcgAbilityTargetType.AnyCreature => "대상 크리처 1체",
                TcgAbilityConstants.TcgAbilityTargetType.EnemyHero => "적 영웅",
                TcgAbilityConstants.TcgAbilityTargetType.AllyHero => "아군 영웅",
                _ => "대상"
            };
        }

        public static string EnTargetNoun1(TcgAbilityConstants.TcgAbilityTargetType tcgAbilityTargetType)
        {
            return tcgAbilityTargetType switch
            {
                TcgAbilityConstants.TcgAbilityTargetType.EnemyCreature => "an enemy creature",
                TcgAbilityConstants.TcgAbilityTargetType.AllyCreature => "an allied creature",
                TcgAbilityConstants.TcgAbilityTargetType.AnyCreature => "a creature",
                TcgAbilityConstants.TcgAbilityTargetType.EnemyHero => "the enemy hero",
                TcgAbilityConstants.TcgAbilityTargetType.AllyHero => "your hero",
                _ => "the target"
            };
        }
    }
}
#endif
