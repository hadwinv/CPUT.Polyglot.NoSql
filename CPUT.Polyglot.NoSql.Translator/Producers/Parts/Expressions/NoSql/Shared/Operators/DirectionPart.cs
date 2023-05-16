using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Base;
using static CPUT.Polyglot.NoSql.Common.Parsers.Operators;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared.Operators
{
    public class DirectionPart : IExpression
    {
        internal string Type { get; set; }

        public DirectionPart(OrderType direction)
        {
            Dictionary<OrderType, string> keywords;

            keywords = new Dictionary<OrderType, string>
            {
                [OrderType.None] = string.Empty,
                [OrderType.Asc] = "ASC",
                [OrderType.Desc] = "DESC"
            };

            Type = keywords[direction];
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
