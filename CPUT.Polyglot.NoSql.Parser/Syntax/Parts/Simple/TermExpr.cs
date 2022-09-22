using CPUT.Polyglot.NoSql.Parser.Syntax.Base;

namespace CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Simple
{
    public class TermExpr : BaseExpr
    {
        public string Value { get; set; }

        public TermExpr(string value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return $"TermExpr {{ Value = {Value} }}";
        }
    }
}