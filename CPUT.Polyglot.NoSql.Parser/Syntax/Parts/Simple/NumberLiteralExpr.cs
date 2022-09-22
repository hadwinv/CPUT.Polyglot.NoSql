using CPUT.Polyglot.NoSql.Parser.Syntax.Base;

namespace CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Simple
{
    public class NumberLiteralExpr : BaseExpr
    {
        public int Value { get; set; }

        public NumberLiteralExpr(int value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return $"NumberExpr {{ Value = {Value} }}";
        }
    }
}
