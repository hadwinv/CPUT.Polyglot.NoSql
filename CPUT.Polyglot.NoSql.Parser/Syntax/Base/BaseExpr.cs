namespace CPUT.Polyglot.NoSql.Parser.Syntax.Base
{
    public class BaseExpr : ICloneable
    {
        public List<object> ParseTree = new List<object>();

        public void Add<T>(T expression) {

            if(expression != null)
                ParseTree.Add(expression);
        }

        public static string FormatArray(IEnumerable<object> values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            return "[" + string.Join(", ", values.Select(c => c.ToString())) + "]";
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
