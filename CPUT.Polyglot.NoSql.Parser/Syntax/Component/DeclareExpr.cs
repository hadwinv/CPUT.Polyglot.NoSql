using CPUT.Polyglot.NoSql.Parser.Syntax.Base;

namespace CPUT.Polyglot.NoSql.Parser.Syntax.Component
{
    public class DeclareExpr : BaseExpr
    {
        public BaseExpr[] Value { get; set; }

        public DeclareExpr(BaseExpr[] value)
        {
            Value = value;
        }
    }
}
