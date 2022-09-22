using CPUT.Polyglot.NoSql.Parser.Builder.Lexicals;
using CPUT.Polyglot.NoSql.Parser.QueryBuilder;
using Superpower.Model;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPUT.Polyglot.NoSql.Parser.Expressions.Matches
{
    public record LabelMatchers(ImmutableArray<LabelMatcher> Matchers, TextSpan? Span = null) : IPromQlNode
    {
        protected virtual bool PrintMembers(StringBuilder builder)
        {
            builder.Append($"{nameof(Matchers)} = ");
            Matchers.PrintArray(builder);

            return true;
        }

        public ImmutableArray<LabelMatcher> Matchers { get; set; } = Matchers;

        public void Accept(IVisitor visitor) => visitor.Visit(this);
    }

}
