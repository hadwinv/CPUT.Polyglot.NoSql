using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Redis;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Shared;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql
{
    public interface IRedisVisitor 
    {
        void Visit(GetPart @get);
        void Visit(QueryPart query);
        void Visit(PropertyPart property);
        void Visit(KeyPart key);
        void Visit(SetKeyValuePart @set);
    }
}
