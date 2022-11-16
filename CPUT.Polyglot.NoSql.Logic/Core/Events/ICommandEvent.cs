using CPUT.Polyglot.NoSql.Logic.Core.Handler;
using CPUT.Polyglot.NoSql.Models;
using CPUT.Polyglot.NoSql.Models.Translator;

namespace CPUT.Polyglot.NoSql.Logic.Core.Events
{
    public interface ICommandEvent
    {
        void Add(int index, CommandHandler handler);
        Output Run(Query request);
    }
}
