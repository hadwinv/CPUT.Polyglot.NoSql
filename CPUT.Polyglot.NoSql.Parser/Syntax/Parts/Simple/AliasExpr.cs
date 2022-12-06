using CPUT.Polyglot.NoSql.Parser.Syntax.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPUT.Polyglot.NoSql.Parser.Syntax.Parts.Simple
{
    public class AliasExpr : BaseExpr
    {
        public string Value { get; set; }

        public AliasExpr(string value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return $"AliasExpr {{ Value = {Value} }}";
        }
    }
}
