using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Base;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared
{
    public class UpdatePart : IExpression
    {
        internal IExpression[] Properties { get; set; }

        public UpdatePart(IExpression[] properties)
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
            visitor.Visit(this);
        }
    }
}
