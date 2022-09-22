using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using CPUT.Polyglot.NoSql.Parser.Expressions.Functions;
using CPUT.Polyglot.NoSql.Parser.Expressions.Selectors;
using CPUT.Polyglot.NoSql.Parser.QueryBuilder;
using ExhaustiveMatching;
using Superpower.Model;

namespace CPUT.Polyglot.NoSql.Parser.Builder.Lexicals
{
    /// <summary>
    /// Base of all PromQL syntactic components.
    /// </summary>
    public interface IPromQlNode
    {
        void Accept(IVisitor visitor);
        TextSpan? Span { get; }
    }

    [Closed(
        typeof(AggregateExpr),
        typeof(BinaryExpr),
        typeof(FunctionCall),
        typeof(MatrixSelector),
        typeof(NumberLiteral),
        typeof(OffsetExpr),
        typeof(ParenExpression),
        typeof(StringLiteral),
        typeof(SubqueryExpr),
        typeof(UnaryExpr),
        typeof(VectorSelector)
    )]
    public interface Expr : IPromQlNode
    {
        ValueType Type { get; }

        /// <summary>
        /// Makes a deep clone of an expression.
        /// </summary>
        /// <returns></returns>
        public Expr DeepClone();
    }


    /// <summary>
    /// VectorMatching describes how elements from two Vectors in a binary operation are supposed to be matched.
    /// </summary>
    /// <param name = "MatchCardinality" > The cardinality of the two Vectors.</param>
    /// <param name = "MatchingLabels" > Contains the labels which define equality of a pair of elements from the Vectors.</param>
    /// <param name = "On" > When true, includes the given label names from matching, rather than excluding them.</param>
    /// <param name = "Include" > Contains additional labels that should be included in the result from the side with the lower cardinality.</param>
    /// <param name = "ReturnBool" > If a comparison operator, return 0/1 rather than filtering.</param>
    public record VectorMatching(Operators11.VectorMatchCardinality MatchCardinality, ImmutableArray<string> MatchingLabels,
        bool On, ImmutableArray<string> Include, bool ReturnBool, TextSpan? Span = null) : IPromQlNode
    {
        public static Operators11.VectorMatchCardinality DefaultMatchCardinality { get; } = Operators11.VectorMatchCardinality.OneToOne;

        public VectorMatching() : this(DefaultMatchCardinality, ImmutableArray<string>.Empty, false,
            ImmutableArray<string>.Empty, false)
        {
        }

        public VectorMatching(bool returnBool) : this(DefaultMatchCardinality, ImmutableArray<string>.Empty, false, ImmutableArray<string>.Empty, returnBool)
        {
        }

        public Operators11.VectorMatchCardinality MatchCardinality { get; internal set; } = MatchCardinality;
        public bool On { get; } = On;
        public ImmutableArray<string> Include { get; internal set; } = Include;
        public bool ReturnBool { get; } = ReturnBool;

        public void Accept(IVisitor visitor) => visitor.Visit(this);
    };


    public record MatrixSelector(VectorSelector Vector, Duration Duration, TextSpan? Span = null) : Expr
    {
        public VectorSelector Vector { get; set; } = Vector;
        public Duration Duration { get; set; } = Duration;
        public void Accept(IVisitor visitor) => visitor.Visit(this);
        public ValueType Type => ValueType.Matrix;
        public Expr DeepClone() => this with { };
    }

    public record VectorSelector : Expr
    {
        public VectorSelector(MetricIdentifier metricIdentifier, TextSpan? span = null)
        {
            MetricIdentifier = metricIdentifier;
            Span = span;
        }

        public VectorSelector(LabelMatchers labelMatchers, TextSpan? span = null)
        {
            LabelMatchers = labelMatchers;
            Span = span;
        }

        public VectorSelector(MetricIdentifier metricIdentifier, LabelMatchers labelMatchers, TextSpan? span = null)
        {

            MetricIdentifier = metricIdentifier;
            LabelMatchers = labelMatchers;
            Span = span;
        }

        public MetricIdentifier? MetricIdentifier { get; set; }
        public LabelMatchers? LabelMatchers { get; set; }
        public TextSpan? Span { get; }
        public ValueType Type => ValueType.Vector;

        public void Accept(IVisitor visitor) => visitor.Visit(this);
        public Expr DeepClone() => this with { };
    }

    public record LabelMatchers(ImmutableArray<LabelMatcher> Matchers, TextSpan? Span = null) : IPromQlNode
    {
        protected virtual bool PrintMembers(StringBuilder builder)
        {
            builder.Append($"{nameof(Matchers)} = ");
            Matchers.PrintArray(builder);

            return true;
        }

        public ImmutableArray<LabelMatcher> Matchers { get; set; } = Matchers;

        public void Accept(IVisitor visitor) => visitor.Visit(this);
    }

    public record LabelMatcher(string LabelName, Operators11.LabelMatch Operator, StringLiteral Value, TextSpan? Span = null) : IPromQlNode
    {
        public void Accept(IVisitor visitor) => visitor.Visit(this);
    }


    public record SubqueryExpr(Expr Expr, Duration Range, Duration? Step = null, TextSpan? Span = null) : Expr
    {
        public Expr Expr { get; set; } = Expr;
        public Duration Range { get; set; } = Range;
        public Duration? Step { get; set; } = Step;
        public ValueType Type => ValueType.Matrix;

        public void Accept(IVisitor visitor) => visitor.Visit(this);
        public Expr DeepClone() => this with { Expr = Expr.DeepClone() };
    }

    //internal static class Extensions
    //{
    //    internal static void PrintArray<T>(this ImmutableArray<T> arr, StringBuilder sb)
    //        where T : notnull
    //    {
    //        sb.Append("[ ");
    //        for (int i = 0; i < arr.Length; i++)
    //        {
    //            sb.Append(arr[i]);
    //            if (i < arr.Length - 1)
    //                sb.Append(", ");
    //        }

    //        sb.Append(" ]");
    //    }
    //}
}
