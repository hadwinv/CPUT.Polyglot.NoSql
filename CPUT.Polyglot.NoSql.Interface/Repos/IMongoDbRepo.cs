using CPUT.Polyglot.NoSql.Models._data.prep;
using System.Collections.Generic;

namespace CPUT.Polyglot.NoSql.Interface.Repos
{
    public interface IMongoDbRepo
    {
        void Load(List<UDataset> dataset);
    }
}
