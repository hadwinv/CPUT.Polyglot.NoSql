using CPUT.Polyglot.NoSql.Parser.Syntax.Base;

namespace CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Simple
{
    public class StorageExpr : BaseExpr
    {
        public string Value { get; set; }

        public StorageExpr(string value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return $"StorageExpr {{ Value = {Value} }}";
        }
    }
}
