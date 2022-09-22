namespace CPUT.Polyglot.NoSql.Parser.Parsers.Operators
{
    public class AggregateOperator
    {
        public string Name { get; set; }

        public ValueType? ParameterType { get; set; }
        public AggregateOperator(string name, ValueType? parameterType = null)
        {
            Name = name;
            ParameterType = parameterType;
        }
    }
}
