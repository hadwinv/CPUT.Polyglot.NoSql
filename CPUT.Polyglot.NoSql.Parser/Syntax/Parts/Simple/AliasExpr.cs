using CPUT.Polyglot.NoSql.Parser.Syntax.Base;

namespace CPUT.Polyglot.NoSql.Parser.Syntax.Parts.Simple
{
    public class AliasExpr : BaseExpr
    {
        public string Value { get; set; }

        public AliasExpr(string value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return $"AliasExpr {{ Value = {Value} }}";
        }
    }
}
