using CPUT.Polyglot.NoSql.Models._data.prep;
using System.Collections.Generic;

namespace CPUT.Polyglot.NoSql.Interface.Repos
{
    public interface INeo4jRepo
    {
        void Load(List<UDataset> dataset);
    }
}
