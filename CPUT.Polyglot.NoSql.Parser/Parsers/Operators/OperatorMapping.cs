using CPUT.Polyglot.NoSql.Common.Parsers;

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

        public static IReadOnlyDictionary<Lexicons, Common.Parsers.Operators.OperatorType> OperatorMap = new Dictionary<Lexicons, Common.Parsers.Operators.OperatorType>()
        {
            [Lexicons.None] = Common.Parsers.Operators.OperatorType.None,
            [Lexicons.EQL] = Common.Parsers.Operators.OperatorType.Eql,
            [Lexicons.GTE] = Common.Parsers.Operators.OperatorType.Gte,
            [Lexicons.GTR] = Common.Parsers.Operators.OperatorType.Gtr,
            [Lexicons.LSS] = Common.Parsers.Operators.OperatorType.Lss,
            [Lexicons.LTE] = Common.Parsers.Operators.OperatorType.Lte
        };

        public static IReadOnlyDictionary<Lexicons, Common.Parsers.Operators.CompareType> CompareMap = new Dictionary<Lexicons, Common.Parsers.Operators.CompareType>()
        {
            [Lexicons.None] = Common.Parsers.Operators.CompareType.None,
            [Lexicons.LAND] = Common.Parsers.Operators.CompareType.And,
            [Lexicons.LOR] = Common.Parsers.Operators.CompareType.Or,
            [Lexicons.COMMA] = Common.Parsers.Operators.CompareType.Comma
        };


        public static IReadOnlyDictionary<Lexicons, Common.Parsers.Operators.DirectionType> DirectionMap = new Dictionary<Lexicons, Common.Parsers.Operators.DirectionType>()
        {
            [Lexicons.None] = Common.Parsers.Operators.DirectionType.None,
            [Lexicons.ASC] = Common.Parsers.Operators.DirectionType.Asc,
            [Lexicons.DESC] = Common.Parsers.Operators.DirectionType.Desc,
        };
    }
}
