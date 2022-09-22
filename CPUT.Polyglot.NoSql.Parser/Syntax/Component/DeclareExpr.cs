using CPUT.Polyglot.NoSql.Parser.Syntax.Base;

namespace CPUT.Polyglot.NoSql.Parser.Syntax.Component
{
    public class DeclareExpr : BaseExpr
    {
        public BaseExpr[] Expressions { get; set; }

        public DeclareExpr()
        {
        }

        public DeclareExpr(BaseExpr[] value)
        {
            Expressions = value;
        }
        
        public override string ToString()
        {
            return $"FetchExpr {{ Value = {Expressions} }}";
        }
    }
}
