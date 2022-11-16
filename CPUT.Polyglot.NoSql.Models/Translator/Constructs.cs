using CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using static CPUT.Polyglot.NoSql.Common.Helpers.Utils;

namespace CPUT.Polyglot.NoSql.Models.Translator
{
    public class Constructs
    {
        public Database Target { get; set; }

        public string DataModel { get; set; }

        public dynamic Query { get; set; }

        public BaseExpr Expression { get; set; }
    }
}
