using CPUT.Polyglot.NoSql.Parser.Syntax.Base;

namespace CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Simple
{
    public class PropertyExpr : BaseExpr
    {
        public string Value { get; set; }

        public PropertyExpr(string value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return $"PropertyExpr {{ Value = {Value} }}";
        }
    }
}
