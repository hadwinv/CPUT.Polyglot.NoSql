using CPUT.Polyglot.NoSql.Parser.Syntax.Base;

namespace CPUT.Polyglot.NoSql.Parser.Syntax.Component
{
    public class LinkExpr : BaseExpr
    {
        public BaseExpr[] Value { get; set; }

        public LinkExpr(BaseExpr[] value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return $"LinkExpr {{ Value = {Value} }}";
        }
    }
}
