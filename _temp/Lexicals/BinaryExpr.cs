using CPUT.Polyglot.NoSql.Parser.Expressions.Matches;
using CPUT.Polyglot.NoSql.Parser.QueryBuilder;
using Superpower.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPUT.Polyglot.NoSql.Parser.Builder.Lexicals
{
    /// <summary>
    /// Represents a binary expression between two child expressions.
    /// </summary>
    /// <param name="LeftHandSide">The left-hand operand of the expression</param>
    /// <param name="RightHandSide">The right-hand operand of the expression</param>
    /// <param name="Operator">The operation of the expression</param>
    /// <param name="VectorMatching">The matching behavior for the operation to be applied if both operands are Vectors.</param>
    public record BinaryExpr(Expr LeftHandSide, Expr RightHandSide, Operators11.Binary Operator,
        VectorMatching? VectorMatching = null, TextSpan? Span = null) : Expr
    {
        public Expr LeftHandSide { get; set; } = LeftHandSide;
        public Expr RightHandSide { get; set; } = RightHandSide;
        public Operators11.Binary Operator { get; set; } = Operator;
        public VectorMatching? VectorMatching { get; set; } = VectorMatching;
        public void Accept(IVisitor visitor) => visitor.Visit(this);

        public ValueType Type
        {
            get
            {
                if (RightHandSide.Type == ValueType.Scalar && LeftHandSide.Type == ValueType.Scalar)
                    return ValueType.Scalar;

                return ValueType.Vector;
            }
        }

        public Expr DeepClone() => this with { LeftHandSide = LeftHandSide.DeepClone(), RightHandSide = RightHandSide.DeepClone() };
    }
}
