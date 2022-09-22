using CPUT.Polyglot.NoSql.Parser.Syntax.Base;

namespace CPUT.Polyglot.NoSql.Parser.Syntax.Component
{
    public class DataModelExpr : BaseExpr
    {
        public BaseExpr[] Value { get; set; }

        public DataModelExpr(BaseExpr[] value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return $"FetchExpression {{ Value = {Value} }}";
        }
    }
}
