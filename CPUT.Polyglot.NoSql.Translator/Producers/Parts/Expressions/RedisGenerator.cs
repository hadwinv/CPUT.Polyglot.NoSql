using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Redis;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Shared;
using System.Text;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions
{
    public class RedisGenerator : IRedisVisitor
    {
        private StringBuilder _query;

        public RedisGenerator(StringBuilder query) => this._query = query;

        public void Visit(QueryPart query)
        {
            foreach (var expr in query.Expressions)
            {
                if (expr is GetPart)
                {
                    ((GetPart)expr).Accept(this);
                }
                else if (expr is KeyPart)
                {
                    ((KeyPart)expr).Accept(this);
                }
                else if (expr is NoSql.Redis.SetKeyValuePart)
                {
                    if(_query.Length > 0)
                        _query.Append(";");

                    ((SetKeyValuePart)expr).Accept(this);
                }
            }
        }

        public void Visit(GetPart part)
        {
            part.Property.Accept(this);
        }

        public void Visit(KeyPart part)
        {
            _query.Append("KEYS " + part.Value);
        }

        public void Visit(SetKeyValuePart part)
        {
            _query.Append("SET " + part.Key + " " + part.Value);
        }

        public void Visit(PropertyPart part)
        {
            if (part.Type == "string")
                _query.Append("GET \"" + part.Name + "\"");
            else
                _query.Append("GET " + part.Name);
        }
    }
}
