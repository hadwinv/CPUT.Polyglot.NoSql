using CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using static CPUT.Polyglot.NoSql.Common.Helpers.Utils;

namespace CPUT.Polyglot.NoSql.Models.Translator
{
    public class ConstructPayload
    {
        public BaseExpr BaseExpr { get; set; }
        public Command Command { get; set; }
    }
}
