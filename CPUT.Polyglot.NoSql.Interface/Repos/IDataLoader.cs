using CPUT.Polyglot.NoSql.Schema._data.prep;
using System.Collections.Generic;

namespace CPUT.Polyglot.NoSql.Interface.Repos
{
    public interface IDataLoader
    {
        List<UDataset> MockFullDataset();
    }
}
