using CPUT.Polyglot.NoSql.Schema._data.prep;
using System.Collections.Generic;

namespace CPUT.Polyglot.NoSql.Interface.Repos
{
    public interface IMongoRepo
    {
        void CreateDocumentDB(List<UDataset> dataset);
    }
}
