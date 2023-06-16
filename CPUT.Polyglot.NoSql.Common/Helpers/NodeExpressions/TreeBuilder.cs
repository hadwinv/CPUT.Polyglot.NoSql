using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CPUT.Polyglot.NoSql.Common.Helpers.NodeExpressions
{
    public static class TreeBuilder
    {
        public static Tree BuildTree(IEnumerable<TreeNode> nodes)
        {
            if (nodes == null) return new Tree();
            var nodeList = nodes.ToList();
            var tree = FindTreeRoot(nodeList);
            BuildTree(tree, nodeList);
            return tree;
        }

        private static void BuildTree(Tree tree, IList<TreeNode> descendants)
        {
            var children = descendants.Where(node => node.Parent == tree.Id).ToArray();
            foreach (var child in children)
            {
                var branch = Map(child);
                tree.Add(branch);
                descendants.Remove(child);
            }
            foreach (var branch in tree.Children)
            {
                BuildTree(branch, descendants);
            }
        }

        private static Tree FindTreeRoot(IList<TreeNode> nodes)
        {
            var rootNodes = nodes.Where(node => node.Parent == null);
            if (rootNodes.Count() != 1) return new Tree();
            var rootNode = rootNodes.Single();
            nodes.Remove(rootNode);
            return Map(rootNode);
        }

        private static Tree Map(TreeNode node)
        {
            return new Tree
            {
                Id = node.Name,
                Name = node.Name,
                Cardinality = node.Cardinality
            };
        }
    }

}
