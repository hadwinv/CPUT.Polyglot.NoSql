using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPUT.Polyglot.NoSql.DataStores.Repos._data
{
    public interface IMockData
    {
        void GenerateData();

        void Load();
    }
}
