using CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using static CPUT.Polyglot.NoSql.Common.Parsers.Operators;

namespace CPUT.Polyglot.NoSql.Parser.Syntax.Component
{
    public class OrderByExpr : BaseExpr
    {
        public BaseExpr[] Properties { get; set; }

        public OrderByExpr(BaseExpr[] properties)
        {
            Properties = properties;
        }
    }
}
