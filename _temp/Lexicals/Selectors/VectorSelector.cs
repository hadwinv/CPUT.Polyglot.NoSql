using CPUT.Polyglot.NoSql.Parser.Builder.Lexicals;
using CPUT.Polyglot.NoSql.Parser.Expressions.Matches;
using CPUT.Polyglot.NoSql.Parser.QueryBuilder;
using Superpower.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPUT.Polyglot.NoSql.Parser.Expressions.Selectors
{

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

}
