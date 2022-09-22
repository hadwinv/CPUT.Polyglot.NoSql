using CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using CPUT.Polyglot.NoSql.Parser.Tokenizers;
using Superpower.Model;
using static CPUT.Polyglot.NoSql.Common.Parsers.Operators;

namespace CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Complex
{
    public class OperatorExpr : BaseExpr
    {
        public BaseExpr Left { get; }
        public BaseExpr Right { get; }
        public OperatorType Type { get; }

        public Token<Lexicons> Test { get; }

        public Token<Lexicons>? CompareType { get; }

        public OperatorExpr(Token<Lexicons> @operator, BaseExpr left, BaseExpr right, Token<Lexicons>? x)
        {
            Left = left ?? throw new ArgumentNullException(nameof(left));
            Right = right ?? throw new ArgumentNullException(nameof(right));
            Test = @operator;
            CompareType = x;
        }

        public override string ToString()
        {
            return $"OperatorExpr";
        }
    }
}
