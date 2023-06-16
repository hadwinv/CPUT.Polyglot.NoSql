using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPUT.Polyglot.NoSql.Common.Helpers.NodeExpressions
{
    public static class TreeExtensions
    {
        public static IEnumerable<Tree> Descendants(this Tree value)
        {
            // a descendant is the node self and any descendant of the children
            if (value == null) yield break;
            yield return value;
            // depth-first pre-order tree walker
            foreach (var child in value.Children)
                foreach (var descendantOfChild in child.Descendants())
                {
                    yield return descendantOfChild;
                }
        }

        public static IEnumerable<Tree> Ancestors(this Tree value)
        {
            // an ancestor is the node self and any ancestor of the parent
            var ancestor = value;
            // post-order tree walker
            while (ancestor != null)
            {
                yield return ancestor;
                ancestor = ancestor.Parent;
            }
        }
    }

}
