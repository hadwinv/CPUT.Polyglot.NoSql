using CPUT.Polyglot.NoSql.Common.Parsers;
using CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using Superpower.Model;
using static CPUT.Polyglot.NoSql.Common.Parsers.Operators;

namespace CPUT.Polyglot.NoSql.Parser.SyntaxExpr.Parts.Complex
{
    public class OperatorExpr : BaseExpr
    {
        public BaseExpr Left { get; }
        
        public BaseExpr Right { get; }
       
        public OperatorType Operator { get; }

        public CompareType Compare { get; }

        public OperatorExpr(BaseExpr left, BaseExpr right, OperatorType @operator, CompareType compare)
        {
            Left = left ?? throw new ArgumentNullException(nameof(left));
            Right = right ?? throw new ArgumentNullException(nameof(right));
            Operator = @operator;
            Compare = compare;
        }
    }
}
