using CPUT.Polyglot.NoSql.Parser.QueryBuilder;
using Superpower.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPUT.Polyglot.NoSql.Parser.Builder.Lexicals
{
    public record NumberLiteral(double Value, TextSpan? Span = null) : Expr
    {
        public void Accept(IVisitor visitor) => visitor.Visit(this);
        public ValueType Type => ValueType.Scalar;
        public Expr DeepClone() => this with { };
    }

}
