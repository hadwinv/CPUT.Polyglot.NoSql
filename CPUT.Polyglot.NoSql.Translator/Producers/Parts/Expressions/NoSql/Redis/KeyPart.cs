using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Base;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Shared;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Redis
{
    public class KeyPart : IExpression
    {
        internal string Value { get; set; }

        public KeyPart(string value)
        {
            Value = value;
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
            visitor.Visit(this);
        }

        public void Accept(IMongoDbVisitor visitor)
        {
            throw new NotImplementedException();
        }
    }
}
