using CPUT.Polyglot.NoSql.Parser.Syntax.Base;

namespace CPUT.Polyglot.NoSql.Parser.Syntax.Statement
{
    public class Describe : BaseExpr
    {
        public List<BaseExpr> Test = new List<BaseExpr>();

        public Describe(BaseExpr fetch, BaseExpr dataModel, BaseExpr filter, BaseExpr groupby, BaseExpr restrict, BaseExpr target)
        {
            Test.Add(fetch);
            Test.Add(dataModel);
            Test.Add(filter);
            Test.Add(groupby);
            Test.Add(restrict);
            Test.Add(target);
        }

        public Describe(BaseExpr fetch)
        {
            Test.Add(fetch);
        }

        public static string FormatArray(IEnumerable<object> values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            return "[" + string.Join(", ", values.Select(c => c.ToString())) + "]";
        }
    }
}
