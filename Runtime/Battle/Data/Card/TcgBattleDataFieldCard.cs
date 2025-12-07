using System.Collections.Generic;

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
        public int Uid { get; }
        public ConfigCommonTcg.TcgPlayerSide OwnerSide { get; }

        /// <summary>
        /// 카드 테이블/런타임과 연결을 유지하기 위한 참조.
        /// 필요 없으면 제거하셔도 됩니다.
        /// </summary>
        public TcgBattleDataCard SourceTcgBattleDataCard { get; }

        public int Attack { get; private set; }
        public int Hp { get; private set; }
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

            Attack = attack;
            Hp = hp;
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
            Hp -= value;
            if (Hp < 0) Hp = 0;
        }

        public void Heal(int value)
        {
            if (value <= 0) return;
            Hp += value;
            if (Hp > MaxHp) Hp = MaxHp;
        }

        public void ModifyAttack(int value)
        {
            Attack += value;
            if (Attack < 0) Attack = 0;
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
