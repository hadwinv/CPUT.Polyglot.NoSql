using CPUT.Polyglot.NoSql.Parser.QueryBuilder;
using Superpower.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPUT.Polyglot.NoSql.Parser.Builder.Lexicals
{
    public record OffsetExpr(Expr Expr, Duration Duration, TextSpan? Span = null) : Expr
    {
        public Expr Expr { get; set; } = Expr;
        public Duration Duration { get; set; } = Duration;
        public void Accept(IVisitor visitor) => visitor.Visit(this);
        public ValueType Type => Expr.Type;
        public Expr DeepClone() => this with { Expr = Expr.DeepClone() };
    }

}
