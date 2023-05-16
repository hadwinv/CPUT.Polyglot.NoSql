namespace CPUT.Polyglot.NoSql.Common.Parsers
{
    public class Operators
    {
        public enum AggregateType
        {
            None,
            Sum,
            Count,
            Avg,
            Max,
            Min,
            NSum,
            NCount,
            NAvg,
            NMax,
            NMin
        }

        public enum OperatorType
        {
            None,
            Eql,
            Gte,
            Gtr,
            Lte,
            Lss,
        }

        public enum CompareType
        {
            None,
            And,
            Or,
            Comma
        }

        public enum OrderType
        {
            None,
            Asc,
            Desc
        }

        public enum DirectionType
        {
            None,
            Forward,
            Backward,
            Left,
            Right
        }
        
        public enum MongoDBFormat
        {
            None,
            Aggregate_Order,
            Find_Order
        }
    }
}
