using CPUT.Polyglot.NoSql.Models._data.prep;
using CPUT.Polyglot.NoSql.Models.Translator;
using CPUT.Polyglot.NoSql.Models.Translator.Executors;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CPUT.Polyglot.NoSql.Interface.Repos
{
    public interface IMongoDbRepo
    {
        Models.Result Execute(QueryDirective query);
    }
}
