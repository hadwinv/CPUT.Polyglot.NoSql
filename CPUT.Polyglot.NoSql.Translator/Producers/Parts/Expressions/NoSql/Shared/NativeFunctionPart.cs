using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Base;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.MongoDb;
using static CPUT.Polyglot.NoSql.Common.Parsers.Operators;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared
{
    public class NativeFunctionPart : IExpression
    {
        internal IExpression Property { get; set; }

        internal string Alias { get; set; }

        internal string Type { get; set; }

        internal Dictionary<AggregateType, string> _keywords = new Dictionary<AggregateType, string>
        {
            [AggregateType.None] = "",
            [AggregateType.NSum] = "SUM",
            [AggregateType.NCount] = "COUNT",
            [AggregateType.NAvg] = "AVG",
            [AggregateType.NMax] = "MAX",
            [AggregateType.NMin] = "MIN"
        };

        public NativeFunctionPart(IExpression property, AggregateType type)
        {
            Property = property;
            Alias = property is FunctionFieldPart ? ((FunctionFieldPart)property).Alias : string.Empty; 
            Type = _keywords[type];
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
