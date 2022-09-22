using CPUT.Polyglot.NoSql.Parser.QueryBuilder;
using Superpower.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPUT.Polyglot.NoSql.Parser.Builder.Lexicals
{
    public record StringLiteral(char Quote, string Value, TextSpan? Span = null) : Expr
    {
        public void Accept(IVisitor visitor) => visitor.Visit(this);
        public ValueType Type => ValueType.String;
        public Expr DeepClone() => this with { };
    }

}
