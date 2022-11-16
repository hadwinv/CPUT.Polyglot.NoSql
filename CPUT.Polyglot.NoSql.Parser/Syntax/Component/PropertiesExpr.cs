using CPUT.Polyglot.NoSql.Parser.Syntax.Base;

namespace CPUT.Polyglot.NoSql.Parser.Syntax.Component
{
    public class PropertiesExpr : BaseExpr
    {
        public BaseExpr[] Value { get; set; }

        public PropertiesExpr(BaseExpr[] value)
        {
            Value = value;
        }
    }
}
