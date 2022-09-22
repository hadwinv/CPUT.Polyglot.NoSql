using CPUT.Polyglot.NoSql.Parser.Builder.Lexicals;
using CPUT.Polyglot.NoSql.Parser.QueryBuilder;
using Superpower.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPUT.Polyglot.NoSql.Parser.Expressions.Selectors
{
    public record MatrixSelector(VectorSelector Vector, Duration Duration, TextSpan? Span = null) : Expr
    {
        public VectorSelector Vector { get; set; } = Vector;
        public Duration Duration { get; set; } = Duration;
        public void Accept(IVisitor visitor) => visitor.Visit(this);
        public ValueType Type => ValueType.Matrix;
        public Expr DeepClone() => this with { };
    }
}
