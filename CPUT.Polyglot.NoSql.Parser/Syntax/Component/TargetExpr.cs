using CPUT.Polyglot.NoSql.Parser.Syntax.Base;

namespace CPUT.Polyglot.NoSql.Parser.Syntax.Component
{
    public class TargetExpr : BaseExpr
    {
        public BaseExpr[] Value { get; set; }

        public TargetExpr(BaseExpr[] value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return $"FetchExpression {{ Value = {Value} }}";
        }
    }
}
