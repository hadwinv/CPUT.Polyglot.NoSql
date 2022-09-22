using CPUT.Polyglot.NoSql.Parser.QueryBuilder;
using Superpower.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPUT.Polyglot.NoSql.Parser.Builder.Lexicals
{
    public record SubqueryExpr(Expr Expr, Duration Range, Duration? Step = null, TextSpan? Span = null) : Expr
    {
        public Expr Expr { get; set; } = Expr;
        public Duration Range { get; set; } = Range;
        public Duration? Step { get; set; } = Step;
        public ValueType Type => ValueType.Matrix;

        public void Accept(IVisitor visitor) => visitor.Visit(this);
        public Expr DeepClone() => this with { Expr = Expr.DeepClone() };
    }
}
