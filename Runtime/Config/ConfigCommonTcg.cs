using System.Collections.Generic;
using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// TCG 전투/UI/규칙 전반에서 공유하는 공통 상수 및 열거형 정의 모음입니다.
    /// </summary>
    public static class ConfigCommonTcg
    {
        /// <summary>
        /// 영웅 슬롯을 식별하기 위한 특수 인덱스 값입니다.
        /// 일반 카드 슬롯 인덱스와 충돌하지 않도록 큰 값으로 예약됩니다.
        /// </summary>
        public const int IndexHeroSlot = 99999;

        /// <summary>
        /// 덱 셔플 동작 모드입니다.
        /// </summary>
        public enum ShuffleMode
        {
            /// <summary>
            /// 셔플을 수행하지 않습니다.
            /// 고정된 순서가 필요한 테스트/디버그 용도로 사용됩니다.
            /// </summary>
            None = 0,

            /// <summary>
            /// 완전 랜덤 셔플입니다(Fisher–Yates).
            /// </summary>
            PureRandom,

            /// <summary>
            /// 코스트 등 가중치를 고려한 셔플입니다.
            /// 초기 손패 쪽에 저코스트가 조금 더 잘 배치되도록 조정하는 등의 용도입니다.
            /// </summary>
            Weighted,

            /// <summary>
            /// 고정 시드를 사용하여 결과를 재현 가능한 셔플입니다.
            /// 리플레이, PVP 검증 등에 사용됩니다.
            /// </summary>
            SeededReplay,

            /// <summary>
            /// 페이즈(초기/중반/후반) 진행을 고려해 가중치를 적용하는 셔플입니다.
            /// 예: 초반에는 저코스트 비중을 높이고, 후반으로 갈수록 고코스트 등장 확률을 완만히 증가.
            /// </summary>
            PhaseWeighted
        }

        /// <summary>
        /// 전투 내에서 플레이어 진영을 구분하는 열거형입니다.
        /// </summary>
        public enum TcgPlayerSide
        {
            /// <summary>
            /// 유효하지 않은 값 또는 미지정 상태입니다.
            /// </summary>
            None = -1,

            /// <summary>
            /// 실제 유저(로컬 플레이어) 진영입니다.
            /// </summary>
            Player = 0,

            /// <summary>
            /// 상대 진영입니다(AI 또는 네트워크 상대 등).
            /// </summary>
            Enemy = 1,

            /// <summary>
            /// 무승부/결과 없음 등 중립 결과를 표현합니다.
            /// </summary>
            Draw = 2
        }

        /// <summary>
        /// 카드/유닛이 존재할 수 있는 전투 내 영역(Zone)을 정의합니다.
        /// </summary>
        public enum TcgZone
        {
            /// <summary>
            /// 유효하지 않은 값 또는 미지정 상태입니다.
            /// </summary>
            None,

            /// <summary>
            /// 플레이어 손패 영역입니다.
            /// </summary>
            HandPlayer,

            /// <summary>
            /// 적 손패 영역입니다.
            /// </summary>
            HandEnemy,

            /// <summary>
            /// 플레이어 필드(전장) 영역입니다.
            /// </summary>
            FieldPlayer,

            /// <summary>
            /// 적 필드(전장) 영역입니다.
            /// </summary>
            FieldEnemy,

            /// <summary>
            /// 덱(남은 카드 더미) 영역입니다.
            /// </summary>
            Deck,

            /// <summary>
            /// 무덤(버려진/파괴된 카드) 영역입니다.
            /// </summary>
            Graveyard
        }

        /// <summary>
        /// UI 창(WindowUid)에서 대응되는 전투 Zone을 찾기 위한 매핑입니다.
        /// </summary>
        private static readonly Dictionary<UIWindowConstants.WindowUid, TcgZone> WindowToZone = new()
        {
            { UIWindowConstants.WindowUid.TcgHandPlayer,  TcgZone.HandPlayer },
            { UIWindowConstants.WindowUid.TcgHandEnemy,   TcgZone.HandEnemy },
            { UIWindowConstants.WindowUid.TcgFieldPlayer, TcgZone.FieldPlayer },
            { UIWindowConstants.WindowUid.TcgFieldEnemy,  TcgZone.FieldEnemy },
        };

        /// <summary>
        /// UI 창 식별자에서 대응되는 전투 Zone을 반환합니다.
        /// 매핑이 없으면 <see cref="TcgZone.None"/>을 반환합니다.
        /// </summary>
        /// <param name="windowUid">Zone을 조회할 UI 창 식별자입니다.</param>
        /// <returns>해당 UI 창이 대표하는 전투 Zone입니다.</returns>
        public static TcgZone GetZoneFromWindowUid(UIWindowConstants.WindowUid windowUid) =>
            WindowToZone.GetValueOrDefault(windowUid, TcgZone.None);

        /// <summary>
        /// 전투 Zone에서 대응되는 UI 창(WindowUid)을 찾기 위한 매핑입니다.
        /// </summary>
        private static readonly Dictionary<TcgZone, UIWindowConstants.WindowUid> ZoneToWindow = new()
        {
            { TcgZone.HandPlayer,  UIWindowConstants.WindowUid.TcgHandPlayer },
            { TcgZone.HandEnemy,   UIWindowConstants.WindowUid.TcgHandEnemy },
            { TcgZone.FieldPlayer, UIWindowConstants.WindowUid.TcgFieldPlayer },
            { TcgZone.FieldEnemy,  UIWindowConstants.WindowUid.TcgFieldEnemy },
        };

        /// <summary>
        /// 전투 Zone에서 대응되는 UI 창 식별자를 반환합니다.
        /// 매핑이 없으면 <see cref="UIWindowConstants.WindowUid.None"/>을 반환합니다.
        /// </summary>
        /// <param name="zone">UI 창을 조회할 전투 Zone입니다.</param>
        /// <returns>해당 Zone에 대응되는 UI 창 식별자입니다.</returns>
        public static UIWindowConstants.WindowUid GetWindowUidFromZone(TcgZone zone) =>
            ZoneToWindow.GetValueOrDefault(zone, UIWindowConstants.WindowUid.None);

        /// <summary>
        /// 플레이어 타입(사람/AI 난이도 등)을 나타내는 열거형입니다.
        /// </summary>
        public enum TcgPlayerKind
        {
            /// <summary>
            /// 사람 플레이어입니다.
            /// </summary>
            Human,

            /// <summary>
            /// 쉬움 난이도의 AI입니다.
            /// </summary>
            AiEasy,

            /// <summary>
            /// 보통 난이도의 AI입니다.
            /// </summary>
            AiNormal,

            /// <summary>
            /// 어려움 난이도의 AI입니다.
            /// </summary>
            AiHard,

            /// <summary>
            /// 커스텀 규칙/파라미터를 적용한 AI입니다.
            /// </summary>
            AiCustom
        }

        /// <summary>
        /// 전투 중 처리할 수 있는 명령의 종류입니다.
        /// 실제 구현 상황에 따라 세분화/확장 가능합니다.
        /// </summary>
        public enum TcgBattleCommandType
        {
            /// <summary>
            /// 유효하지 않은 값 또는 미지정 상태입니다.
            /// </summary>
            None = 0,

            /// <summary>
            /// 카드를 드로우하여 필드로 배치하는 명령입니다.
            /// </summary>
            DrawCardToField,

            /// <summary>
            /// 유닛이 유닛을 공격하는 명령입니다.
            /// </summary>
            AttackUnit,

            /// <summary>
            /// 유닛이 영웅을 공격하는 명령입니다.
            /// </summary>
            AttackHero,

            /// <summary>
            /// 턴을 종료하는 명령입니다.
            /// </summary>
            EndTurn,

            /// <summary>
            /// 스펠 카드를 사용하는 명령입니다.
            /// </summary>
            UseCardSpell,

            /// <summary>
            /// 장비 카드를 사용하는 명령입니다.
            /// </summary>
            UseCardEquipment,

            /// <summary>
            /// 영속 카드를 사용하는 명령입니다.
            /// </summary>
            UseCardPermanent,

            /// <summary>
            /// 이벤트 카드를 사용하는 명령입니다.
            /// </summary>
            UseCardEvent
        }

        /// <summary>
        /// 유닛/카드 키워드 종류입니다.
        /// </summary>
        public enum TcgKeyword
        {
            None = 0,

            /// <summary>
            /// 소환된 턴에도 공격할 수 있습니다.
            /// </summary>
            Rush,

            /// <summary>
            /// 도발: 상대는 먼저 이 유닛을 공격해야 합니다.
            /// (실제 로직은 타겟 선택 단계에서 처리)
            /// </summary>
            Taunt,

            /// <summary>
            /// 공격 시 피해량만큼 체력을 회복합니다.
            /// </summary>
            Lifesteal,

            /// <summary>
            /// 일정 횟수(또는 1회) 피해/공격을 무효화합니다.
            /// </summary>
            DivineShield,

            /// <summary>
            /// 은신 상태입니다: 적이 이 카드를 명시적으로 선택할 수 없습니다.
            /// 공격 또는 특정 행동 시 은신이 해제될 수 있습니다.
            /// </summary>
            Stealth,

            /// <summary>
            /// 턴 종료 시 일정량의 체력을 자동 회복합니다.
            /// 회복량은 카드 데이터 또는 규칙 엔진에서 결정됩니다.
            /// </summary>
            Regenerate,

            /// <summary>
            /// 공격 성공 시 지속 피해(DoT)를 부여합니다.
            /// 지속 피해 처리는 턴 트리거에 의해 수행됩니다.
            /// </summary>
            Burn,

            /// <summary>
            /// 공격 시 상대 방어를 일부 무시하고 피해를 전달합니다.
            /// 무시 수치는 규칙 또는 카드 능력에서 정의됩니다.
            /// </summary>
            Pierce,

            /// <summary>
            /// 특정 조건(전투 시작/은신 첫 공격 등)에서 추가 피해를 제공합니다.
            /// </summary>
            Ambush,

            /// <summary>
            /// 받는 물리 피해를 일정 비율 감소시킵니다.
            /// </summary>
            Fortified,

            /// <summary>
            /// 밀침, 속박, 기절 등 제어 효과에 대한 저항을 가집니다.
            /// 특정 제어 효과를 무시하거나 지속 시간을 단축합니다.
            /// </summary>
            Unstoppable,

            /// <summary>
            /// 턴 단위로 방어력이 증가하거나 보호막을 생성합니다.
            /// </summary>
            ShieldGrowth,

            /// <summary>
            /// 자연(Nature) 속성/시너지와 상호작용 시 강화됩니다.
            /// </summary>
            NatureBond,

            /// <summary>
            /// 공격 시 추가 번개 피해 또는 연쇄 번개(Chain Lightning) 효과를 발생시킵니다.
            /// </summary>
            LightningSurge,

            /// <summary>
            /// 비행 유닛입니다: 일부 지상 공격/지상 전용 능력의 영향을 받지 않을 수 있습니다.
            /// </summary>
            Flying,

            /// <summary>
            /// 공격력이 일시적으로 증가하지만, 조건에 따라 반동 피해(자가 피해)를 받을 수 있습니다.
            /// </summary>
            Overcharge
        }

        /// <summary>
        /// Permanent 카드의 수명(Lifetime) 모델입니다.
        /// - Indefinite: 전투 종료/명시적 제거 전까지 유지
        /// - Durability: 발동/사용 등 특정 이벤트마다 내구도가 감소
        /// - DurationTurns: 턴 트리거마다 남은 턴 수가 감소
        /// - TriggerCount: 능력 발동 횟수 기반으로 만료
        /// </summary>
        public enum TcgPermanentLifetimeType
        {
            /// <summary>
            /// 명시적으로 제거되거나 전투가 종료될 때까지 유지됩니다.
            /// </summary>
            Indefinite = 0,

            /// <summary>
            /// 사용/발동 등 이벤트마다 내구도가 감소하며, 0이 되면 만료됩니다.
            /// </summary>
            Durability = 1,

            /// <summary>
            /// 턴 단위로 남은 지속 턴 수가 감소하며, 0이 되면 만료됩니다.
            /// </summary>
            DurationTurns = 2,

            /// <summary>
            /// 능력 발동 횟수를 소모하며, 남은 횟수가 0이 되면 만료됩니다.
            /// </summary>
            TriggerCount = 3
        }
    }
}
