using CPUT.Polyglot.NoSql.Parser.Syntax.Base;

namespace CPUT.Polyglot.NoSql.Parser.Syntax.Component
{
    public class RestrictExpr : BaseExpr
    {
        public BaseExpr Value { get; set; }

        public RestrictExpr(BaseExpr value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return $"FetchExpression {{ Value = {Value} }}";
        }
    }
}
