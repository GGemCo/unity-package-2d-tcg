using System;
using System.Collections.Generic;
using R3;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 필드 위에 존재하는 유닛(크리처)의 전투 런타임 상태를 나타내는 순수 데이터 모델입니다.
    /// 뷰(게임오브젝트/애니메이션 등)와 분리되어 있으며, UI/연출은 값을 구독(Observable)하여 표시만 갱신합니다.
    /// 실제 수치 변경은 BattleManager/이펙트 시스템에서만 수행하는 것을 전제로 합니다.
    /// </summary>
    public sealed class TcgBattleDataCardInField
    {
        private int _index;

        /// <summary>
        /// 필드 슬롯 인덱스입니다(배치 순서/위치).
        /// </summary>
        public int Index => _index;

        /// <summary>
        /// 전투 내에서 유닛을 식별하기 위한 고유 ID입니다.
        /// </summary>
        public int Uid { get; }

        /// <summary>
        /// 이 유닛의 소유 플레이어 진영입니다.
        /// </summary>
        public ConfigCommonTcg.TcgPlayerSide OwnerSide { get; }

        /// <summary>
        /// 원본 카드(손패 런타임)와의 연결을 유지하기 위한 참조입니다.
        /// 필요하지 않다면 제거해도 되며, 제거 시 생성/이펙트 로직도 함께 정리되어야 합니다.
        /// </summary>
        public TcgBattleDataCardInHand SourceTcgBattleDataCardInHand { get; }

        /// <summary>
        /// 공격력 값 스트림(Observable)입니다.
        /// UI/연출은 이 값을 구독하여 자동 갱신할 수 있습니다.
        /// </summary>
        public readonly BehaviorSubject<int> attack = new(0);

        /// <summary>
        /// 현재 공격력(편의용 프로퍼티)입니다.
        /// </summary>
        public int Attack => attack.Value;

        /// <summary>
        /// 필드 슬롯 인덱스를 설정합니다.
        /// </summary>
        /// <param name="index">설정할 슬롯 인덱스입니다.</param>
        public void SetIndex(int index) => _index = index;

        /// <summary>
        /// 체력 값 스트림(Observable)입니다.
        /// </summary>
        public readonly BehaviorSubject<int> health = new(0);

        /// <summary>
        /// 현재 체력(편의용 프로퍼티)입니다.
        /// </summary>
        public int Health => health.Value;

        /// <summary>
        /// 최대 체력입니다.
        /// </summary>
        public int MaxHp { get; private set; }

        /// <summary>
        /// 이번 턴에 공격 가능한지 여부입니다.
        /// 소환된 턴에는 보통 false이며, 턴 시작 처리에서 true로 리셋하는 용도로 사용합니다.
        /// </summary>
        public bool CanAttack { get; set; }

        /// <summary>
        /// 키워드(도발/돌진 등) 목록입니다.
        /// 필요에 따라 Enum 플래그(비트마스크)로 최적화할 수 있습니다.
        /// </summary>
        public List<ConfigCommonTcg.TcgKeyword> Keywords { get; } = new List<ConfigCommonTcg.TcgKeyword>(4);

        /// <summary>
        /// 필드 유닛 런타임을 생성합니다.
        /// </summary>
        /// <param name="uid">전투 내 고유 ID입니다.</param>
        /// <param name="ownerSide">소유 플레이어 진영입니다.</param>
        /// <param name="sourceTcgBattleDataCardInHand">원본 카드(손패 런타임) 참조입니다.</param>
        /// <param name="attack">초기 공격력입니다.</param>
        /// <param name="hp">초기 체력(최대 체력으로도 사용)입니다.</param>
        /// <param name="keywords">초기 키워드 목록입니다(없으면 null 가능).</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="uid"/>가 0 이하이거나, <paramref name="hp"/>가 0 이하인 값이 전달되는 경우(계약을 강제하고 싶을 때) 발생할 수 있습니다.
        /// </exception>
        /// <remarks>
        /// 현재 구현은 예외를 실제로 throw 하지는 않으며, 호출부에서 유효한 값이 들어온다는 전제를 두고 있습니다.
        /// </remarks>
        public TcgBattleDataCardInField(
            int uid,
            ConfigCommonTcg.TcgPlayerSide ownerSide,
            TcgBattleDataCardInHand sourceTcgBattleDataCardInHand,
            int attack,
            int hp,
            IEnumerable<ConfigCommonTcg.TcgKeyword> keywords = null)
        {
            Uid = uid;
            OwnerSide = ownerSide;
            SourceTcgBattleDataCardInHand = sourceTcgBattleDataCardInHand;

            this.attack.OnNext(attack);
            this.health.OnNext(hp);

            MaxHp = hp;
            CanAttack = false;

            if (keywords != null)
            {
                Keywords.AddRange(keywords);
            }
        }

        /// <summary>
        /// 피해를 적용하여 현재 체력을 감소시킵니다.
        /// </summary>
        /// <param name="value">적용할 피해량입니다. 0 이하이면 무시합니다.</param>
        /// <remarks>
        /// 체력은 0 미만으로 내려가지 않도록 클램프(clamp)됩니다.
        /// </remarks>
        public void ApplyDamage(int value)
        {
            if (value <= 0) return;

            var newValue = Health - value;
            if (newValue < 0) newValue = 0;

            // GcLogger.Log($"side:{OwnerSide}, ApplyDamage: {value}, old hp: {Hp} -> new hp: {newValue}");
            health.OnNext(newValue);
        }

        /// <summary>
        /// 회복을 적용하여 현재 체력을 증가시킵니다.
        /// </summary>
        /// <param name="value">회복량입니다. 0 이하이면 무시합니다.</param>
        /// <remarks>
        /// 체력은 <see cref="MaxHp"/>를 초과하지 않도록 클램프(clamp)됩니다.
        /// </remarks>
        public void Heal(int value)
        {
            if (value <= 0) return;

            var newValue = Health + value;
            if (newValue > MaxHp) newValue = MaxHp;

            // GcLogger.Log($"side:{OwnerSide}, Heal: {value}, old hp: {Hp} -> new hp: {newValue}");
            health.OnNext(newValue);
        }

        /// <summary>
        /// 공격력을 증감합니다.
        /// </summary>
        /// <param name="value">증감 값입니다. 음수면 감소, 양수면 증가입니다.</param>
        /// <remarks>
        /// 공격력은 0 미만으로 내려가지 않도록 클램프(clamp)됩니다.
        /// </remarks>
        public void ModifyAttack(int value)
        {
            var newValue = attack.Value + value;
            if (newValue < 0) newValue = 0;

            attack.OnNext(newValue);
        }

        /// <summary>
        /// 최대 체력과 현재 체력을 함께 증감합니다.
        /// </summary>
        /// <param name="value">증감 값입니다. 양수면 증가, 음수면 감소입니다.</param>
        /// <remarks>
        /// - <see cref="MaxHp"/>는 최소 1로 유지됩니다.
        /// - 현재 체력은 0..MaxHp 범위로 클램프(clamp)됩니다.
        /// </remarks>
        public void ModifyHealth(int value)
        {
            if (value == 0) return;

            MaxHp += value;
            if (MaxHp < 1) MaxHp = 1;

            var newHp = Health + value;
            if (newHp < 0) newHp = 0;
            if (newHp > MaxHp) newHp = MaxHp;

            health.OnNext(newHp);
        }

        /// <summary>
        /// 지정한 키워드를 보유하고 있는지 확인합니다.
        /// </summary>
        /// <param name="keyword">확인할 키워드입니다.</param>
        /// <returns>보유 중이면 true, 아니면 false입니다.</returns>
        public bool HasKeyword(ConfigCommonTcg.TcgKeyword keyword)
        {
            return Keywords.Contains(keyword);
        }

        /// <summary>
        /// 키워드를 추가합니다. 이미 존재하면 중복 추가하지 않습니다.
        /// </summary>
        /// <param name="keyword">추가할 키워드입니다.</param>
        public void AddKeyword(ConfigCommonTcg.TcgKeyword keyword)
        {
            if (!Keywords.Contains(keyword))
                Keywords.Add(keyword);
        }

        /// <summary>
        /// 키워드를 제거합니다.
        /// </summary>
        /// <param name="keyword">제거할 키워드입니다.</param>
        public void RemoveKeyword(ConfigCommonTcg.TcgKeyword keyword)
        {
            Keywords.Remove(keyword);
        }
    }
}
