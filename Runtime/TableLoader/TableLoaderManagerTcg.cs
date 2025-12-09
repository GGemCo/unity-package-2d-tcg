using GGemCo2DCore;

namespace GGemCo2DTcg
{
    public class TableLoaderManagerTcg : TableLoaderBase
    {
        public static TableLoaderManagerTcg Instance;
        
        public TableTcgCard TableTcgCard { get; private set; } = new TableTcgCard();
        public TableTcgCardCreature TableTcgCardCreature { get; private set; } = new TableTcgCardCreature();
        public TableTcgCardSpell TableTcgCardSpell { get; private set; } = new TableTcgCardSpell();
        
        protected void Awake()
        {
            if (!Instance)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                registry = new TableRegistry();
                // 순서 중요.
                registry.Register(TableTcgCardCreature);
                registry.Register(TableTcgCardSpell);
                registry.Register(TableTcgCard);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}