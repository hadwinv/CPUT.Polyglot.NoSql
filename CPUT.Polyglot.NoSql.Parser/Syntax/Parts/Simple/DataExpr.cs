using CPUT.Polyglot.NoSql.Parser.Syntax.Base;

namespace CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Simple
{
    public class DataExpr : BaseExpr
    {
        public string Value { get; set; }

        public string AliasIdentifier { get; set; }

        public DataExpr(string value, string aliasIdentifier)
        {
            Value = value;
            AliasIdentifier = aliasIdentifier;
        }

        public override string ToString()
        {
            return $"DataExpr {{ Value = {Value} }}";
        }
    }
}
