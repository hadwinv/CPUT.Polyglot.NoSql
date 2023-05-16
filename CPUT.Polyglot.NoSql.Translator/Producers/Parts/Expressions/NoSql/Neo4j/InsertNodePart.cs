using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Base;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared.Operators;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Shared;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Neo4j
{
    public class InsertNodePart : IExpression
    {
        internal PropertyPart Left { get; set; }

        internal OperatorPart Operator { get; set; }

        internal PropertyPart Right { get; set; }

        public InsertNodePart(PropertyPart left, OperatorPart @operator, PropertyPart right)
        {
            Left = left;
            Right = right;
            Operator = @operator;
        }

        public void Accept(INeo4jVisitor visitor)
        {
            visitor.Visit(this);
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
            throw new NotImplementedException();
        }
    }
}
