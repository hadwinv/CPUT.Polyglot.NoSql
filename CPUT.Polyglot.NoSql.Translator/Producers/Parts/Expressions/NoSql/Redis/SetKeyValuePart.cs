using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Base;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Redis
{
    public class SetKeyValuePart : IExpression
    {
        internal string Key { get; set; }

        internal string Value { get; set; }

        public SetKeyValuePart(string key, string value)
        {
            Key = key;
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
