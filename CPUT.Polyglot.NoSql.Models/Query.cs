using CPUT.Polyglot.NoSql.Common.Helpers;
using CPUT.Polyglot.NoSql.Common.Parsers;
using Superpower.Model;

namespace CPUT.Polyglot.NoSql.Models
{
    public class Query
    {
        public Utils.Command Command { get; set; }
        public TokenList<Lexicons> Tokens { get; set; }
        public string Message { get; set; }
    }
}
