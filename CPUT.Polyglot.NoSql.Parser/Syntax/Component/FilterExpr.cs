using CPUT.Polyglot.NoSql.Parser.Syntax.Base;

namespace CPUT.Polyglot.NoSql.Parser.Syntax.Component
{
    public class FilterExpr : BaseExpr
    {
        public BaseExpr[] Value { get; set; }

        public FilterExpr(BaseExpr[] value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return $"FilterExpr {{ Value = {Value} }}";
        }
    }
}
