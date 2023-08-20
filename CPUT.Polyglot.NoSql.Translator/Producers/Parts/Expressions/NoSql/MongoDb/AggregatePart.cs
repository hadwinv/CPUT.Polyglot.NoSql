using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Base;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.MongoDb
{
    public class AggregatePart : IExpression
    {
        internal string CollectionName { get; set; }

        internal MatchPart? Match { get; set; }

        internal UnwindGroupPart? Unwind { get; set; }

        internal ProjectPart? Project { get; set; }

        internal GroupByPart? GroupBy { get; set; }

        internal OrderByPart? OrderBy { get; set; }

        internal RestrictPart? Restrict { get; set; }

        public AggregatePart()
        {
        }

        public void Accept(INeo4jVisitor visitor)
        {
            throw new NotImplementedException();
        }

        public void Accept(ICassandraVisitor visitor)
        {
            throw new NotImplementedException();
        }

        public void Accept(IRedisVisitor visitor)
        {
            throw new NotImplementedException();
        }

        public void Accept(IMongoDbVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}

