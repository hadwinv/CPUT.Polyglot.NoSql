using CPUT.Polyglot.NoSql.Parser.Syntax.Base;

namespace CPUT.Polyglot.NoSql.Parser.Syntax.Parts
{
    public class GroupExpr : BaseExpr
    {
        public BaseExpr Value { get; set; }

        public GroupExpr(BaseExpr expression)
        {
            Value = expression;
        }
    }
}
