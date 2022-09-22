using CPUT.Polyglot.NoSql.Parser.Syntax.Base;

namespace CPUT.Polyglot.NoSql.Parser.Syntax.Parts.Simple
{
    public class StringLiteralExpr : BaseExpr
    {
        public string Value { get; set; }

        public StringLiteralExpr(string value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return $"StringExpr {{ Value = {Value} }}";
        }
    }
}
