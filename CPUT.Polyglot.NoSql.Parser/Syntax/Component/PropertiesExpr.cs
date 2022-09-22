using CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPUT.Polyglot.NoSql.Parser.Syntax.Component
{
    public class PropertiesExpr : BaseExpr
    {
        public BaseExpr[] Value { get; set; }

        public PropertiesExpr(BaseExpr[] value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return $"FilterExpr {{ Value = {Value} }}";
        }
    }
}
