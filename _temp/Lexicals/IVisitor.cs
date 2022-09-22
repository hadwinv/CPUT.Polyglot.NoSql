using CPUT.Polyglot.NoSql.Parser.Builder.Lexicals;
using CPUT.Polyglot.NoSql.Parser.Expressions.Functions;
using CPUT.Polyglot.NoSql.Parser.Expressions.Matches;
using CPUT.Polyglot.NoSql.Parser.Expressions.Selectors;
using System;

namespace CPUT.Polyglot.NoSql.Parser.QueryBuilder
{
    /// <summary>
    /// Visitor interface for PromQL.
    /// </summary>
    /// <remarks>
    /// Note: implementations are responsible for iterating over children. See <see cref="Printer"/> for an example
    /// implementation.
    /// </remarks>
    public interface IVisitor
    {
        void Visit(StringLiteral expr);
        void Visit(SubqueryExpr sq);
        void Visit(Duration d);
        void Visit(NumberLiteral n);
        void Visit(MetricIdentifier mi);
        void Visit(LabelMatcher expr);
        void Visit(UnaryExpr unary);
        void Visit(MatrixSelector ms);
        void Visit(OffsetExpr offset);
        void Visit(ParenExpression paren);
        void Visit(FunctionCall fnCall);
        void Visit(VectorMatching vm);
        void Visit(BinaryExpr expr);
        void Visit(AggregateExpr expr);
        void Visit(VectorSelector vs);
        void Visit(LabelMatchers lms);
    }
}