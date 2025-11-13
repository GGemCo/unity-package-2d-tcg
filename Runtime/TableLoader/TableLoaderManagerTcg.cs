using GGemCo2DCore;

namespace GGemCo2DTcg
{
    public class TableLoaderManagerTcg : TableLoaderBase
    {
        public static TableLoaderManagerTcg Instance;
        
        public TableTcgCard TableTcgCard { get; private set; } = new TableTcgCard();
        
        protected void Awake()
        {
            if (!Instance)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                registry = new TableRegistry();
                registry.Register(TableTcgCard);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}