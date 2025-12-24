using GGemCo2DCore;

namespace GGemCo2DTcg
{
    public class TableLoaderManagerTcg : TableLoaderBase
    {
        public static TableLoaderManagerTcg Instance;
        
        public TableTcgCard TableTcgCard { get; private set; } = new TableTcgCard();
        public TableTcgCardCreature TableTcgCardCreature { get; private set; } = new TableTcgCardCreature();
        public TableTcgCardSpell TableTcgCardSpell { get; private set; } = new TableTcgCardSpell();
        public TableTcgCardEquipment TableTcgCardEquipment { get; private set; } = new TableTcgCardEquipment();
        public TableTcgCardPermanent TableTcgCardPermanent { get; private set; } = new TableTcgCardPermanent();
        public TableTcgCardEvent TableTcgCardEvent { get; private set; } = new TableTcgCardEvent();
        public TableTcgCardHero TableTcgCardHero { get; private set; } = new TableTcgCardHero();

        public TableTcgAbility TableTcgAbility { get; private set; } = new TableTcgAbility();
        
        protected void Awake()
        {
            if (!Instance)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                registry = new TableRegistry();
                // 순서 중요.
                registry.Register(TableTcgAbility);
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
                Destroy(gameObject);
            }
        }
    }
}