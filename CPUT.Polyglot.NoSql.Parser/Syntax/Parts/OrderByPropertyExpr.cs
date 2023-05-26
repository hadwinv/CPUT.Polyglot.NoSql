using CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CPUT.Polyglot.NoSql.Common.Parsers.Operators;

namespace CPUT.Polyglot.NoSql.Parser.Syntax.Parts
{
    public class OrderByPropertyExpr : BaseExpr
    {
        public string Value { get; set; }

        public string AliasIdentifier { get; set; }

        public OrderType Direction { get; set; }

        public OrderByPropertyExpr(string value, string aliasIdentifier, OrderType direction)
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
