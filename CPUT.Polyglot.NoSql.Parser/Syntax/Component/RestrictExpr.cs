using CPUT.Polyglot.NoSql.Parser.Syntax.Base;

namespace CPUT.Polyglot.NoSql.Parser.Syntax.Component
{
    public class RestrictExpr : BaseExpr
    {
        public int Value { get; set; }

        public RestrictExpr(int value)
        {
            Value = value;
        }
    }
}
