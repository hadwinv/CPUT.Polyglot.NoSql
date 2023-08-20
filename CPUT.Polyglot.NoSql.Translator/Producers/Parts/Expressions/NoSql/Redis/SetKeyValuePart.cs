using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Base;
using System.Collections.Specialized;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Redis
{
    public class SetKeyValuePart : IExpression
    {
        internal string Key { get; set; }

        internal List<string> Value { get; set; }

        public SetKeyValuePart(string key, List<string> value)
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
