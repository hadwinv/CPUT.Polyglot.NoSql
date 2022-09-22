using CPUT.Polyglot.NoSql.Parser.Tokenizers;

namespace CPUT.Polyglot.NoSql.Parser.Parsers.Operators
{
    public static class OperatorMapping
    {
        public static IReadOnlyDictionary<Lexicons, Common.Parsers.Operators.AggregateType> AggregateMap = new Dictionary<Lexicons, Common.Parsers.Operators.AggregateType>()
        {
            [Lexicons.SUM] = Common.Parsers.Operators.AggregateType.Sum,
            [Lexicons.COUNT] = Common.Parsers.Operators.AggregateType.Count,
            [Lexicons.AVG] = Common.Parsers.Operators.AggregateType.Avg,
            [Lexicons.MIN] = Common.Parsers.Operators.AggregateType.Min,
            [Lexicons.MAX] = Common.Parsers.Operators.AggregateType.Max

        };
    }
}
