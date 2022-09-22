namespace CPUT.Polyglot.NoSql.Parser.Syntax.Base
{
    public class BaseExpr
    {
        public void Add<T>(T Expr) { }

        public static string FormatArray(IEnumerable<object> values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            return "[" + string.Join(", ", values.Select(c => c.ToString())) + "]";
        }
    }
}
