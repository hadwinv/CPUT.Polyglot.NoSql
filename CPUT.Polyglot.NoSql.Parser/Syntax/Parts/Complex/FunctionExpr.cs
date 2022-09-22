using CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using static CPUT.Polyglot.NoSql.Common.Parsers.Operators;

namespace CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Complex
{
    public class FunctionExpr : BaseExpr
    {

        public BaseExpr[] Value { get; set; }

        public AggregateType Type { get; set; }

        public FunctionExpr(BaseExpr[] value, AggregateType type)
        {
            Value = value;
            Type = type;
        }
    }
}
