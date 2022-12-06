using CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CPUT.Polyglot.NoSql.Common.Parsers.Operators;

namespace CPUT.Polyglot.NoSql.Parser.Syntax.Component
{
    public class OrderByExpr : BaseExpr
    {
        public string Value { get; set; }

        public string AliasIdentifier { get; set; }

        public DirectionType Direction { get; set; }

        public OrderByExpr(string value, string aliasIdentifier, DirectionType direction)
        {
            Value = value;
            AliasIdentifier = aliasIdentifier;

            if (direction == DirectionType.None)
                Direction = DirectionType.Asc;
            else
                Direction = direction;
        }
    }
}
