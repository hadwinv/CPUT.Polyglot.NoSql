using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Base;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared.Operators;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Shared;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared
{
    public class LogicalPart : IExpression
    {
        internal PropertyPart Left { get; set; }

        internal OperatorPart Operator { get; set; }

        internal PropertyPart Right { get; set; }

        internal ComparePart Compare { get; set; }

        public LogicalPart(PropertyPart left, OperatorPart @operator, PropertyPart right, ComparePart compare)
        {
            Left = left;
            Right = right;
            Operator = @operator;
            Compare = compare;
        }

        public void Accept(INeo4jVisitor visitor)
        {
            visitor.Visit(this);
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
