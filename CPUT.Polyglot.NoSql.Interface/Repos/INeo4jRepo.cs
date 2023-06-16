using CPUT.Polyglot.NoSql.Models._data.prep;
using CPUT.Polyglot.NoSql.Models.Translator;
using System.Collections.Generic;

namespace CPUT.Polyglot.NoSql.Interface.Repos
{
    public interface INeo4jRepo
    {
        Models.Result Execute(Constructs construct);

        void Load(List<UDataset> dataset);
    }
}
