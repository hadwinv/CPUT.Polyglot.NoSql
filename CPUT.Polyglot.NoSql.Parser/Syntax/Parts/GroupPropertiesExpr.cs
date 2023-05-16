using CPUT.Polyglot.NoSql.Parser.Syntax.Base;

namespace CPUT.Polyglot.NoSql.Parser.Syntax.Parts
{
    public class GroupPropertiesExpr : BaseExpr
    {
        public BaseExpr[] Value { get; set; }

        public GroupPropertiesExpr(BaseExpr[] expression)
        {
            Value = expression;
        }
}
}
