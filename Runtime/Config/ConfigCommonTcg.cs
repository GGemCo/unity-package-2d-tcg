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
        /// 유닛/카드 키워드 종류. todo 정리 필요.
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
            DivineShield
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