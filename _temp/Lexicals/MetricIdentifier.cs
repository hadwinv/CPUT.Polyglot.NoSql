using CPUT.Polyglot.NoSql.Parser.QueryBuilder;
using Superpower.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPUT.Polyglot.NoSql.Parser.Builder.Lexicals
{
    public record MetricIdentifier(string Value, TextSpan? Span = null) : IPromQlNode
    {
        public void Accept(IVisitor visitor) => visitor.Visit(this);
    }
}
