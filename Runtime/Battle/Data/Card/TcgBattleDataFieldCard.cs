using System.Collections.Generic;
using R3;

namespace GGemCo2DTcg
{
    /// <summary>
    /// 필드 위에 존재하는 유닛의 런타임 상태.
    /// - 뷰(게임오브젝트, 애니메이션 등)와는 분리된 순수 데이터 모델입니다.
    /// - UI나 연출 쪽에서는 이 런타임을 참조만 하고,
    ///   실제 숫자 변경은 BattleManager/이펙트 시스템에서만 처리하도록 합니다.
    /// </summary>
    public sealed class TcgBattleDataFieldCard
    {
        private int _index;
        public int Index => _index;
        public int Uid { get; }
        public ConfigCommonTcg.TcgPlayerSide OwnerSide { get; }

        /// <summary>
        /// 카드 테이블/런타임과 연결을 유지하기 위한 참조.
        /// 필요 없으면 제거하셔도 됩니다.
        /// </summary>
        public TcgBattleDataCard SourceTcgBattleDataCard { get; }

        /// <summary>
        /// 공격력(Observable). UI/연출은 이 값을 구독하여 자동 갱신할 수 있습니다.
        /// </summary>
        public readonly BehaviorSubject<int> attack = new(0);

        /// <summary>
        /// 현재 공격력 (편의용 프로퍼티)
        /// </summary>
        public int Attack => attack.Value;
        
        public void SetIndex(int index) => _index = index;
        /// <summary>
        /// 체력(Observable)
        /// </summary>
        public readonly BehaviorSubject<int> health = new(0);
        public int Health => health.Value;
        
        public int MaxHp { get; private set; }

        /// <summary>
        /// 이번 턴에 공격 가능한지 여부.
        /// - 소환된 턴에는 false, 턴 시작 시 true 로 리셋하도록 사용합니다.
        /// </summary>
        public bool CanAttack { get; set; }

        /// <summary>
        /// 키워드(도발, 돌진 등)를 위한 리스트.
        /// Enum 플래그 비트마스크 형태로 바꾸어도 됩니다.
        /// </summary>
        public List<ConfigCommonTcg.TcgKeyword> Keywords { get; } = new List<ConfigCommonTcg.TcgKeyword>(4);

        public TcgBattleDataFieldCard(
            int uid,
            ConfigCommonTcg.TcgPlayerSide ownerSide,
            TcgBattleDataCard sourceTcgBattleDataCard,
            int attack,
            int hp,
            IEnumerable<ConfigCommonTcg.TcgKeyword> keywords = null)
        {
            Uid = uid;
            OwnerSide = ownerSide;
            SourceTcgBattleDataCard = sourceTcgBattleDataCard;

            this.attack.OnNext(attack);
            this.health.OnNext(hp);
            
            MaxHp = hp;
            CanAttack = false;

            if (keywords != null)
            {
                Keywords.AddRange(keywords);
            }
        }

        public void ApplyDamage(int value)
        {
            if (value <= 0) return;
            var newValue = Health - value;
            if (newValue < 0) newValue = 0;
            // GcLogger.Log($"side:{OwnerSide}, ApplyDamage: {value}, old hp: {Hp} -> new hp: {newValue}");
            health.OnNext(newValue);
        }

        public void Heal(int value)
        {
            if (value <= 0) return;
            var newValue = Health + value;
            if (newValue > MaxHp) newValue = MaxHp;
            // GcLogger.Log($"side:{OwnerSide}, Heal: {value}, old hp: {Hp} -> new hp: {newValue}");
            health.OnNext(newValue);
        }

        public void ModifyAttack(int value)
        {
            var newValue = attack.Value + value;
            if (newValue < 0) newValue = 0;
            attack.OnNext(newValue);
        }

        /// <summary>
        /// 최대 체력과 현재 체력을 함께 증감합니다.
        /// - value가 양수면 최대/현재 체력 증가
        /// - value가 음수면 최대/현재 체력 감소(현재 체력은 0..MaxHp로 클램프)
        /// </summary>
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

        public bool HasKeyword(ConfigCommonTcg.TcgKeyword keyword)
        {
            return Keywords.Contains(keyword);
        }

        public void AddKeyword(ConfigCommonTcg.TcgKeyword keyword)
        {
            if (!Keywords.Contains(keyword))
                Keywords.Add(keyword);
        }

        public void RemoveKeyword(ConfigCommonTcg.TcgKeyword keyword)
        {
            Keywords.Remove(keyword);
        }
    }
}
