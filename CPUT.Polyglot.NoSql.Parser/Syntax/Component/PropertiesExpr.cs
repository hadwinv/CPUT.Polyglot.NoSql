using CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using CPUT.Polyglot.NoSql.Parser.Syntax.Parts;

namespace CPUT.Polyglot.NoSql.Parser.Syntax.Component
{
    public class PropertiesExpr : BaseExpr
    {
        public BaseExpr[] Value { get; set; }

        public PropertiesExpr(BaseExpr[][] values)
        {
            var expr = new List<BaseExpr>();

            foreach(var value in values)
                expr.Add(new GroupPropertiesExpr(value));
            
            Value = expr.ToArray();
        }
    }
}
