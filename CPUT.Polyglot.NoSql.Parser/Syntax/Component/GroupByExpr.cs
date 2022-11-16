using CPUT.Polyglot.NoSql.Parser.Syntax.Base;

namespace CPUT.Polyglot.NoSql.Parser.Syntax.Component
{
    public class GroupByExpr : BaseExpr
    {
        public BaseExpr[] Value { get; set; }

        public GroupByExpr(BaseExpr[] value)
        {
            Value = value;
        }
    }
}
