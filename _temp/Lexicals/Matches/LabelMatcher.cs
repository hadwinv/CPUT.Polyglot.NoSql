using CPUT.Polyglot.NoSql.Parser.Builder.Lexicals;
using CPUT.Polyglot.NoSql.Parser.QueryBuilder;
using Superpower.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPUT.Polyglot.NoSql.Parser.Expressions.Matches
{
    public record LabelMatcher(string LabelName, Operators11.LabelMatch Operator, StringLiteral Value, TextSpan? Span = null) : IPromQlNode
    {
        public void Accept(IVisitor visitor) => visitor.Visit(this);
    }
}
