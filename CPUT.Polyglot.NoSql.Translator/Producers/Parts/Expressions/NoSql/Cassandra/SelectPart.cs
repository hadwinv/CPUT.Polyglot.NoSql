using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Base;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Cassandra
{
    public class SelectPart : IExpression
    {
        internal IExpression[] Properties { get; set; }

        public SelectPart(IExpression[] properties)
        {
            Properties = properties;
        }

        public void Accept(INeo4jVisitor visitor)
        {
            throw new NotImplementedException();
            
        }

        public void Accept(ICassandraVisitor visitor)
        {
            visitor.Visit(this);
        }

        public void Accept(IRedisVisitor visitor)
        {
            throw new NotImplementedException();
        }

        public void Accept(IMongoDbVisitor visitor)
        {
            throw new NotImplementedException();
        }
    }
}
