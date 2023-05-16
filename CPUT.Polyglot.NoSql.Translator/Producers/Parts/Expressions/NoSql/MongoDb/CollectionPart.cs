using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Base;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.MongoDb
{
    public class CollectionPart : IExpression
    {
        internal string Target { get; set; }

        internal string Source { get; set; }

        internal string Alias { get; set; }

        internal FindPart Find { get; set; }

        internal AggregatePart Aggregate { get; set; }

        internal UpdatePart Update { get; set; }

        internal InsertPart Insert { get; set; }

        public CollectionPart(string target, string source, string alias)
        {
            Target = target;
            Source = source;
            Alias = alias;
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
