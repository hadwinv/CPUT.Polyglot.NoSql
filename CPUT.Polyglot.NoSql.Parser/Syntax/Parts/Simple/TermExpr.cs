using CPUT.Polyglot.NoSql.Parser.Syntax.Base;

namespace CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Simple
{
    public class TermExpr : BaseExpr
    {
        public string Value { get; set; }

        public string AliasIdentifier { get; set; }

        public string AliasName { get; set; }

        public TermExpr(string value, string aliasIdentifier, string aliasName)
        {
            Value = value;
            AliasIdentifier = aliasIdentifier;
            AliasName = aliasName;
        }

        public override string ToString()
        {
            return $"TermExpr {{ Value = {Value} }}";
        }
    }
}