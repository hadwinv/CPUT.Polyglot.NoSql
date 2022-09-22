using CPUT.Polyglot.NoSql.Parser.QueryBuilder;
using Superpower.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPUT.Polyglot.NoSql.Parser.Builder.Lexicals
{
    public record UnaryExpr(Operators11.Unary Operator, Expr Expr, TextSpan? Span = null) : Expr
    {
        public Operators11.Unary Operator { get; set; } = Operator;
        public Expr Expr { get; set; } = Expr;

        public void Accept(IVisitor visitor) => visitor.Visit(this);
        public ValueType Type => Expr.Type;
        public Expr DeepClone() => this with { Expr = Expr.DeepClone() };
    }
}
