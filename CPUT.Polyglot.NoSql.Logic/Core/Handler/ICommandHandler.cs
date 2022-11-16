using CPUT.Polyglot.NoSql.Common.Parsers;
using CPUT.Polyglot.NoSql.Models.Translator;
using Superpower.Model;

namespace CPUT.Polyglot.NoSql.Logic.Core.Handler
{
    public interface ICommandHandler
    {
        Output Execute(TokenList<Lexicons> request);
    }
}
