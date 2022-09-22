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
    public record VectorMatching(Operators11.VectorMatchCardinality MatchCardinality, ImmutableArray<string> MatchingLabels,
        bool On, ImmutableArray<string> Include, bool ReturnBool, TextSpan? Span = null) : IPromQlNode
    {
        public static Operators11.VectorMatchCardinality DefaultMatchCardinality { get; } = Operators11.VectorMatchCardinality.OneToOne;

        public VectorMatching() : this(DefaultMatchCardinality, ImmutableArray<string>.Empty, false,
            ImmutableArray<string>.Empty, false)
        {
        }

        public VectorMatching(bool returnBool) : this(DefaultMatchCardinality, ImmutableArray<string>.Empty, false, ImmutableArray<string>.Empty, returnBool)
        {
        }

        public Operators11.VectorMatchCardinality MatchCardinality { get; internal set; } = MatchCardinality;
        public bool On { get; } = On;
        public ImmutableArray<string> Include { get; internal set; } = Include;
        public bool ReturnBool { get; } = ReturnBool;

        public void Accept(IVisitor visitor) => visitor.Visit(this);
    };
}
