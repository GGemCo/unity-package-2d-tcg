using GGemCo2DCore;

namespace GGemCo2DTcg
{
    /// <summary>
    /// TCG(Tabletop Card Game)에서 사용되는 모든 카드 관련 테이블을 로드하고 관리하는 매니저 클래스입니다.
    /// Singleton 패턴으로 동작하며, 게임 전반에서 단일 인스턴스를 통해 테이블에 접근할 수 있도록 합니다.
    /// </summary>
    public class TableLoaderManagerTcg : TableLoaderBase
    {
        /// <summary>
        /// <see cref="TableLoaderManagerTcg"/>의 전역 접근을 위한 Singleton 인스턴스입니다.
        /// </summary>
        public static TableLoaderManagerTcg Instance;

        /// <summary>
        /// 모든 TCG 카드의 공통 정보 테이블입니다.
        /// </summary>
        public TableTcgCard TableTcgCard { get; private set; } = new TableTcgCard();

        /// <summary>
        /// 크리처(Card Creature) 타입 카드 전용 테이블입니다.
        /// </summary>
        public TableTcgCardCreature TableTcgCardCreature { get; private set; } = new TableTcgCardCreature();

        /// <summary>
        /// 스펠(Card Spell) 타입 카드 전용 테이블입니다.
        /// </summary>
        public TableTcgCardSpell TableTcgCardSpell { get; private set; } = new TableTcgCardSpell();

        /// <summary>
        /// 장비(Card Equipment) 타입 카드 전용 테이블입니다.
        /// </summary>
        public TableTcgCardEquipment TableTcgCardEquipment { get; private set; } = new TableTcgCardEquipment();

        /// <summary>
        /// 지속 효과(Card Permanent) 타입 카드 전용 테이블입니다.
        /// </summary>
        public TableTcgCardPermanent TableTcgCardPermanent { get; private set; } = new TableTcgCardPermanent();

        /// <summary>
        /// 이벤트(Card Event) 타입 카드 전용 테이블입니다.
        /// </summary>
        public TableTcgCardEvent TableTcgCardEvent { get; private set; } = new TableTcgCardEvent();

        /// <summary>
        /// 히어로(Card Hero) 타입 카드 전용 테이블입니다.
        /// </summary>
        public TableTcgCardHero TableTcgCardHero { get; private set; } = new TableTcgCardHero();

        /// <summary>
        /// Unity 생명주기 중 오브젝트가 초기화될 때 호출됩니다.
        /// Singleton 인스턴스를 설정하고, 카드 테이블들을 레지스트리에 등록합니다.
        /// </summary>
        protected void Awake()
        {
            if (!Instance)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                // 테이블 간 참조 및 의존성 해결을 위해 등록 순서가 중요합니다.
                registry = new TableRegistry();
                registry.Register(TableTcgCardHero);
                registry.Register(TableTcgCardCreature);
                registry.Register(TableTcgCardSpell);
                registry.Register(TableTcgCardEquipment);
                registry.Register(TableTcgCardPermanent);
                registry.Register(TableTcgCardEvent);
                registry.Register(TableTcgCard);
            }
            else
            {
                // 이미 Singleton 인스턴스가 존재하는 경우 중복 생성을 방지합니다.
                Destroy(gameObject);
            }
        }
    }
}
