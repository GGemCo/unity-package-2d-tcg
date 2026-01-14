using GGemCo2DCoreEditor;
using GGemCo2DTcg;

namespace GGemCo2DTcgEditor
{
    public class TableLoaderManagerTcg : TableLoaderManagerBase
    {

        public static TableTcgCard LoadTableTcgCard()
        {
            return LoadTable<TableTcgCard>(ConfigAddressableTableTcg.TableTcgCard.Path);
        }
    }
}