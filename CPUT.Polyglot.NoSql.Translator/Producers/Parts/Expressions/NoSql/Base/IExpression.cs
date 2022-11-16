namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Base
{
    public interface IExpression
    {
        void Accept(INeo4jVisitor visitor);

        void Accept(ICassandraVisitor visitor);

        void Accept(IRedisVisitor visitor);

        void Accept(IMongoDbVisitor visitor);
    }
}
