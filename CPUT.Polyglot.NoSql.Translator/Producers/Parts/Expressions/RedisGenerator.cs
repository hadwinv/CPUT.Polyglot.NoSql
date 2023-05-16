using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Redis;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Shared;
using System.Text;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions
{
    public class RedisGenerator : IRedisVisitor
    {
        StringBuilder Query;

        public RedisGenerator(StringBuilder query) => this.Query = query;

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
                    if(Query.Length > 0)
                        Query.Append(";");

                    ((SetKeyValuePart)expr).Accept(this);
                }
            }
        }

        public void Visit(GetPart get)
        {
            get.Property.Accept(this);
        }

        public void Visit(KeyPart keyPart)
        {
            Query.Append("KEYS " + keyPart.Value);
        }

        public void Visit(SetKeyValuePart set)
        {
            Query.Append("SET " + set.Key + " " + set.Value);
        }

        public void Visit(PropertyPart property)
        {
            if (property.Type == "string")
                Query.Append("GET \"" + property.Name + "\"");
            else
                Query.Append("GET " + property.Name);
        }
    }
}
