using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    public static class ConfigCommonTcg
    {
        // 영웅 슬롯 index
        public const int IndexHeroSlot = 99999;

        /// <summary>
        /// 덱 셔플 동작 모드.
        /// </summary>
        public enum ShuffleMode
        {
            /// <summary>
            /// 고정된 순서를 위해 추가
            /// </summary>
            None = 0,

            /// <summary>
            /// 완전 랜덤 셔플 (Fisher–Yates).
            /// </summary>
            PureRandom,

            /// <summary>
            /// 코스트 등 가중치를 고려한 셔플.
            /// 초기 손패 쪽에 저코스트가 조금 더 잘 배치되도록 조정하는 등의 용도.
            /// </summary>
            Weighted,

            /// <summary>
            /// 고정 시드를 사용하여 결과를 재현 가능한 셔플.
            /// 리플레이, PVP 검증 등에 사용.
            /// </summary>
            SeededReplay,
            PhaseWeighted
        }

        /// <summary>
        /// 전투 내에서 플레이어를 구분하는 열거형.
        /// </summary>
        public enum TcgPlayerSide
        {
            None = -1,
            Player = 0, // 실제 유저
            Enemy = 1, // AI 또는 네트워크 상대 등
            Draw = 2
        }

        public enum TcgZone
        {
            None,
            HandPlayer,
            HandEnemy,
            FieldPlayer,
            FieldEnemy,
            Deck,
            Graveyard,
        }

        private static readonly Dictionary<UIWindowConstants.WindowUid, TcgZone> WindowToZone = new Dictionary<UIWindowConstants.WindowUid, TcgZone>
        {
            { UIWindowConstants.WindowUid.TcgHandPlayer, TcgZone.HandPlayer },
            { UIWindowConstants.WindowUid.TcgHandEnemy, TcgZone.HandEnemy },
            { UIWindowConstants.WindowUid.TcgFieldPlayer, TcgZone.FieldPlayer },
            { UIWindowConstants.WindowUid.TcgFieldEnemy, TcgZone.FieldEnemy },
        };

        public static TcgZone GetZoneFromWindowUid(UIWindowConstants.WindowUid windowUid) =>
            WindowToZone.GetValueOrDefault(windowUid, TcgZone.None);

        private static readonly Dictionary<TcgZone, UIWindowConstants.WindowUid> ZoneToWindow = new Dictionary<TcgZone, UIWindowConstants.WindowUid>
        {
            { TcgZone.HandPlayer, UIWindowConstants.WindowUid.TcgHandPlayer },
            { TcgZone.HandEnemy, UIWindowConstants.WindowUid.TcgHandEnemy },
            { TcgZone.FieldPlayer, UIWindowConstants.WindowUid.TcgFieldPlayer },
            { TcgZone.FieldEnemy, UIWindowConstants.WindowUid.TcgFieldEnemy },
        };
        public static UIWindowConstants.WindowUid GetWindowUidFromZone(TcgZone zone) => 
            ZoneToWindow.GetValueOrDefault(zone, UIWindowConstants.WindowUid.None);

        /// <summary>
        /// 플레이어 타입(사람/AI 난이도 등)을 나타내는 열거형.
        /// </summary>
        public enum TcgPlayerKind
        {
            Human,
            AiEasy,
            AiNormal,
            AiHard,
            AiCustom
        }

        /// <summary>
        /// 전투 중 처리할 수 있는 명령의 종류.
        /// 실제 구현 상황에 따라 세분화/확장 가능합니다.
        /// </summary>
        public enum TcgBattleCommandType
        {
            None = 0,
            DrawCardToField,
            AttackUnit,        // 유닛 -> 유닛 공격
            AttackHero,        // 유닛 -> 영웅 공격
            EndTurn,           // 턴 종료
            UseCardSpell,
            UseCardEquipment,
            UseCardPermanent,
            UseCardEvent,
        }
        /// <summary>
        /// 유닛/카드 키워드 종류.
        /// </summary>
        public enum TcgKeyword
        {
            None = 0,

            /// <summary>
            /// 소환된 턴에도 공격할 수 있습니다.
            /// </summary>
            Rush,

            /// <summary>
            /// 도발. 상대는 먼저 이 유닛을 공격해야 합니다.
            /// (실제 로직은 타겟 선택 단계에서 처리)
            /// </summary>
            Taunt,

            /// <summary>
            /// 공격 시 생명력 흡수.
            /// </summary>
            Lifesteal,

            /// <summary>
            /// 공격을 1회 무효화 등(예시).
            /// </summary>
            DivineShield,
            
            /// <summary>
            /// 은신 상태: 적이 이 카드를 명시적으로 선택할 수 없습니다.
            /// 공격 또는 특정 행동 시 은신이 해제될 수 있습니다.
            /// </summary>
            Stealth,

            /// <summary>
            /// 재생: 자신의 턴 종료 시 일정량의 체력을 자동 회복합니다.
            /// 회복량은 카드 개별 데이터 또는 규칙 엔진에서 결정됩니다.
            /// </summary>
            Regenerate,

            /// <summary>
            /// 화상: 공격에 성공하면 적에게 지속 피해(DoT)를 부여합니다.
            /// 화상 데미지는 턴 종료 시 처리됩니다.
            /// </summary>
            Burn,

            /// <summary>
            /// 관통 공격: 공격 시 상대의 방어력을 일부 무시하고 직접 피해를 전달합니다.
            /// 방어력 무시 수치는 공통 규칙 또는 카드 능력에서 정의됩니다.
            /// </summary>
            Pierce,

            /// <summary>
            /// 기습: 전투 시작 또는 은신 상태에서 첫 공격 시 추가 피해를 제공합니다.
            /// 한 번 발동 후에는 일반 공격 규칙을 따릅니다.
            /// </summary>
            Ambush,

            /// <summary>
            /// 중갑: 받는 물리 피해를 일정 비율 감소시킵니다.
            /// Stone Golem과 같은 탱커형 Creature에 적합합니다.
            /// </summary>
            Fortified,

            /// <summary>
            /// 경직 무시: 밀침, 속박, 기절과 같은 방해 효과에 대한 저항을 가집니다.
            /// 특정 제어 효과를 무시하거나 지속 시간을 단축합니다.
            /// </summary>
            Unstoppable,

            /// <summary>
            /// 성장 보호막: 한 턴마다 방어력이 증가하거나 보호막을 생성합니다.
            /// Verdant Forest Guardian과 같은 자연계 Creature에서 자주 사용됩니다.
            /// </summary>
            ShieldGrowth,

            /// <summary>
            /// 자연 강화: 아군 자연(Nature) 속성 카드와 상호작용 시 강화됩니다.
            /// 속성별 시너지 시스템에서 사용됩니다.
            /// </summary>
            NatureBond,
            
            /// <summary>
            /// 번개 충격: 공격 시 추가 번개 피해가 적용되거나,
            /// 주변 적에게 연쇄 번개 효과(Chain Lightning)를 발생시킵니다.
            /// </summary>
            LightningSurge,

            /// <summary>
            /// 비행: 비행 유닛으로서 일부 지상 공격이나 지상 대상 전용 능력의 영향을 받지 않습니다.
            /// 전장 충돌 규칙에서 특별한 우선순위를 갖습니다.
            /// </summary>
            Flying,

            /// <summary>
            /// 과충전: 공격력이 일시적으로 증가하나,
            /// 일정 조건에서 반동 피해(자가 피해)를 받을 수 있습니다.
            /// 고위험·고화력 특성을 가진 비행/마법 생물의 전형적인 능력입니다.
            /// </summary>
            Overcharge
        }
        /// <summary>
        /// Permanent 카드의 수명(Lifetime) 모델.
        /// 
        /// - Indefinite: 전투 종료/명시적 제거 전까지 유지
        /// - Durability: 발동/사용 등 특정 이벤트마다 내구도가 감소
        /// - DurationTurns: 특정 턴 트리거마다 남은 턴 수가 감소
        /// - TriggerCount: 능력 발동 횟수 기반으로 만료
        /// </summary>
        public enum TcgPermanentLifetimeType
        {
            Indefinite = 0,
            Durability = 1,
            DurationTurns = 2,
            TriggerCount = 3,
        }
    }
}