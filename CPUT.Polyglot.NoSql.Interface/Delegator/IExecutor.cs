using CPUT.Polyglot.NoSql.Models.Translator;
using CPUT.Polyglot.NoSql.Models.Translator.Executors;
using System.Collections.Generic;
using System.Threading.Tasks;
using static CPUT.Polyglot.NoSql.Common.Helpers.Utils;

namespace CPUT.Polyglot.NoSql.Interface.Delegator
{
    public interface IExecutor
    {
        Task<List<Models.Result>> Forward(Command command, Output output);

        
    }
}
