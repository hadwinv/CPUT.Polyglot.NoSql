using CPUT.Polyglot.NoSql.Parser.QueryBuilder;
using Superpower.Model;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPUT.Polyglot.NoSql.Parser.Builder.Lexicals
{
    /// <summary>
    /// Represents an aggregation operation on a Vector.
    /// </summary>
    /// <param name="Operator">The used aggregation operation.</param>
    /// <param name="Expr">The Vector expression over which is aggregated.</param>
    /// <param name="Param">Parameter used by some aggregators.</param>
    /// <param name="GroupingLabels">The labels by which to group the Vector.</param>
    /// <param name="Without"> Whether to drop the given labels rather than keep them.</param>
    public record AggregateExpr(AggregateOperator Operator, Expr Expr, Expr? Param,
        ImmutableArray<string> GroupingLabels, bool Without, TextSpan? Span = null) : Expr
    {
        public AggregateExpr(AggregateOperator @operator, Expr expr)
            : this(@operator, expr, null, ImmutableArray<string>.Empty, false)
        {
        }

        public AggregateExpr(AggregateOperator @operator, Expr expr, Expr param, bool without = false, params string[] groupingLabels)
            : this(@operator, expr, param, groupingLabels.ToImmutableArray(), without)
        {
        }

        public AggregateOperator Operator { get; set; } = Operator;
        public Expr Expr { get; set; } = Expr;
        public Expr? Param { get; set; } = Param;
        public ImmutableArray<string> GroupingLabels { get; set; } = GroupingLabels;
        public bool Without { get; set; } = Without;

        public ValueType Type => ValueType.Vector;

        public void Accept(IVisitor visitor) => visitor.Visit(this);
        public Expr DeepClone() => this with { Expr = Expr.DeepClone(), Param = Param?.DeepClone() };
    }
}
