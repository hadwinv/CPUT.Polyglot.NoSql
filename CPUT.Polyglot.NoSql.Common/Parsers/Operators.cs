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
            Min
        }

        public enum OperatorType
        {
            Eql,
            Gte,
            Gtr,
            Lte,
            Lss,
        }

        
    }
}
