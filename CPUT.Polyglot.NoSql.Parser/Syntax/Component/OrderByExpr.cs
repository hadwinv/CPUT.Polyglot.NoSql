using CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using static CPUT.Polyglot.NoSql.Common.Parsers.Operators;

namespace CPUT.Polyglot.NoSql.Parser.Syntax.Component
{
    public class OrderByExpr : BaseExpr
    {
        public string Value { get; set; }

        public string AliasIdentifier { get; set; }

        public OrderType Direction { get; set; }

        public OrderByExpr(string value, string aliasIdentifier, OrderType direction)
        {
            Value = value;
            AliasIdentifier = aliasIdentifier;

            if (direction == OrderType.None)
                Direction = OrderType.Asc;
            else
                Direction = direction;
        }
    }
}
