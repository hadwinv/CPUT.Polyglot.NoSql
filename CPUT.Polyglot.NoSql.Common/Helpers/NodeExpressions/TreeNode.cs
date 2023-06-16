using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPUT.Polyglot.NoSql.Common.Helpers.NodeExpressions
{
    public class TreeNode
    {
        public string Name { get; set; }
        public string Cardinality { get; set; }
        public string Parent { get; set; }
    }
}
