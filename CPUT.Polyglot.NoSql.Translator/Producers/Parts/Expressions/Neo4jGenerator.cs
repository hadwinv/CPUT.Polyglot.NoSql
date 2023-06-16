using CPUT.Polyglot.NoSql.Common.Helpers.NodeExpressions;
using CPUT.Polyglot.NoSql.Models._data.prep;
using CPUT.Polyglot.NoSql.Models.Views.Native;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.Neo4j;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Neo4j;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared.Operators;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Shared;
using MongoDB.Driver;
using Neo4jClient;
using Pipelines.Sockets.Unofficial.Arenas;
using System;
using System.Diagnostics;
using System.Text;
using static CPUT.Polyglot.NoSql.Common.Parsers.Operators;
using static CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.Neo4jGenerator;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions
{
    public class Neo4jGenerator : INeo4jVisitor
    {
        private StringBuilder _query;

        private List<UnwindGraphPart> _aliases { get; set; }

        public Neo4jGenerator(StringBuilder query) => this._query = query;

        public void Visit(QueryPart query)
        {
            //get unwinded part
            var group = (UnwindGroupPart?)query.Expressions.SingleOrDefault(x => x.GetType().Equals(typeof(UnwindGroupPart)));

            if(group != null)
                _aliases = group.Fields
                                        .Where(x => x.GetType().Equals(typeof(UnwindGraphPart)))
                                        .Select(x => (UnwindGraphPart)x).ToList();
                

            foreach (var expr in query.Expressions)
            {
                if(expr is MatchPart)
                    ((MatchPart)expr).Accept(this);
                else if (expr is UnwindGroupPart)
                    ((UnwindGroupPart)expr).Accept(this);
                else if (expr is WithPart)
                    ((WithPart)expr).Accept(this);
                else if (expr is InsertPart)
                    ((InsertPart)expr).Accept(this);
                else if (expr is ConditionPart)
                    ((ConditionPart)expr).Accept(this);
                else if (expr is SetPart)
                    ((SetPart)expr).Accept(this);
                else if (expr is ReturnPart)
                    ((ReturnPart)expr).Accept(this);
                else if (expr is OrderByPart)
                    ((OrderByPart)expr).Accept(this);
                else if (expr is RestrictPart)
                    ((RestrictPart)expr).Accept(this);
            }
        }

        public void Visit(MatchPart part)
        {
            var nodes = part.Properties.Where(x => x.GetType().Equals(typeof(NodePart))).Cast<NodePart>().ToList();
            
            //create object tree
            var tree = CreateTree(nodes);

            _query.Append("MATCH ");

            var parent = new List<NodePart>();

            var names = new List<string>();

            foreach (var node in UnwindTree(tree))
            {
                if (names.Contains(node.Name))
                    continue;//skip iteration

                if (node.Parent.Id == "root")
                {
                    if (node.Children.Count() > 1)
                        parent.Add(nodes.First(x => x.Name == node.Name));
                    else
                        DoNodePart(nodes.First(x => x.Name == node.Name));
                }
                else
                {    
                    if (parent.Count > 0)
                    {
                        foreach (var child in node.Children)
                        {
                            DoNodePart(nodes.First(x => x.Name == child.Name));

                            var childCardinality = nodes.SelectMany(x => x.Relations.Where(x => x.Reference == child.Id)).First();

                            DoRelationshipPart(childCardinality, DirectionType.Backward);

                            names.Add(child.Name);
                        }

                        DoNodePart(nodes.First(x => x.Name == node.Name));

                        var cardinality = nodes.SelectMany(x => x.Relations.Where(x => x.Reference == node.Id)).First();

                        DoRelationshipPart(cardinality, DirectionType.Backward);

                        if (parent.Count > 0)
                        {
                            DoNodePart(parent[0]);

                            parent.RemoveAt(0);
                        }
                    }
                    else
                    {
                        var cardinality = nodes.SelectMany(x => x.Relations.Where(x => x.Reference == node.Id)).First();

                        DoRelationshipPart(cardinality, DirectionType.Forward);

                        DoNodePart(nodes.First(x => x.Name == node.Name));
                    }
                }

                names.Add(node.Name);
            }
        }

        #region DML

        public void Visit(SetPart part)
        {
            _query.Append(" SET ");

            foreach (var expr in part.Properties)
            {
                if (expr is SetValuePart)
                    ((SetValuePart)expr).Accept(this);
                else if (expr is SeparatorPart)
                    ((SeparatorPart)expr).Accept(this); ;
            }
        }

        public void Visit(SetValuePart part)
        {
            part.Left.Accept(this);

            _query.Append(part.Operator.Type + " ");

            if (part.Right.Type == "string")
                _query.Append("\"" + part.Right.Name + "\"");
            else
                part.Right.Accept(this);
        }

        public void Visit(InsertPart part)
        {
            _query.Append("CREATE ( ");

            foreach (var expr in part.Properties)
            {
                if (expr is NodePart)
                    ((NodePart)expr).Accept(this);
                else if (expr is ValuesPart)
                    ((ValuesPart)expr).Accept(this);
            }

            _query.Append(" )");
        }

        public void Visit(ValuesPart part)
        {
            _query.Append(" { ");

            foreach (var expr in part.Properties)
            {
                if (expr is InsertNodePart)
                    ((InsertNodePart)expr).Accept(this);
                else if (expr is SeparatorPart)
                    ((SeparatorPart)expr).Accept(this);
            }

            _query.Append(" } ");
        }

        public void Visit(InsertNodePart part)
        {
            _query.Append(part.Left.Name + " : ");

            if (part.Right.Type == "string")
                _query.Append("\"" + part.Right.Name + "\"");
            else
                _query.Append(part.Right.Name);
        }

        #endregion

        public void Visit(NodePart part)
        {
            _query.Append(part.AliasIdentifier + ":" + part.Name);
        }

        public void Visit(RelationshipPart part)
        {
            _query.Append("[:" + part.Name + "]");
        }

        public void Visit(PropertyPart part)
        {
            if (_aliases != null && _aliases.Exists(x => x.ParentReferenceAlias == part.AliasIdentifier))
                _query.Append(_aliases.Single(x => x.ParentReferenceAlias == part.AliasIdentifier).UnwindedAlias + ".");
            else
            {
                if (!string.IsNullOrEmpty(part.AliasIdentifier))
                    _query.Append(part.AliasIdentifier + ".");
            }


            _query.Append(part.Name);

            if (!string.IsNullOrEmpty(part.AliasName))
                _query.Append("AS " + part.AliasName);
        }

        public void Visit(NativeFunctionPart part)
        {
            _query.Append(part.Type + "(");

            part.Property.Accept(this);

            _query.Append(") as ");

            if (part.Property is PropertyPart)
                _query.Append(((PropertyPart)part.Property).Name);
        }

        public void Visit(UnwindGroupPart part)
        {
            _query.Append(" UNWIND ");

            foreach (var expr in part.Fields)
            {
                if (expr is UnwindGraphPart)
                {
                    var property = ((UnwindGraphPart)expr);

                    _query.Append("apoc.convert.fromJsonList( ");

                    property.Accept(this);

                    _query.Append(") as " + property.UnwindedAlias);
                }
                else if (expr is SeparatorPart)
                    ((SeparatorPart)expr).Accept(this);
            }
        }

        public void Visit(UnwindGraphPart part)
        {
            _query.Append(part.ParentReferenceAlias + "." + part.UnwindProperty);
        }

        public void Visit(WithPart part)
        {
            _query.Append(part.Keyword);

            foreach (var expr in part.Fields)
            {
                if (expr is WithAliasPart)
                    ((WithAliasPart)expr).Accept(this);
                else if (expr is SeparatorPart)
                    ((SeparatorPart)expr).Accept(this);
            }
        }

        public void Visit(WithAliasPart part)
        {
            if (_aliases != null && _aliases.Exists(x => x.ParentReferenceAlias == part.Value))
                _query.Append(_aliases.Single(x => x.ParentReferenceAlias == part.Value).UnwindedAlias);
            else
                _query.Append(part.Value);
        }

        public void Visit(ConditionPart part)
        {
            _query.Append(" WHERE ");

            foreach (var expr in part.Logic)
            {
                if (expr is LogicalPart)
                    ((LogicalPart)expr).Accept(this);
                else if (expr is ConditionPart)
                    ((ConditionPart)expr).Accept(this);
            }
        }

        public void Visit(LogicalPart part)
        {
            if (!string.IsNullOrEmpty(part.Compare.Type))
                _query.Append(" " + part.Compare.Type + " ");

            part.Left.Accept(this);

            _query.Append(" " + part.Operator.Type + " ");

            if (part.Right.Type == "string")
                _query.Append("\"" + part.Right.Name + "\"");
            else
                part.Right.Accept(this);
        }

        public void Visit(ReturnPart part)
        {
            _query.Append(" RETURN ");

            foreach (var expr in part.Properties)
            {
                if (expr is PropertyPart)
                    ((PropertyPart)expr).Accept(this);
                else if (expr is NativeFunctionPart)
                    ((NativeFunctionPart)expr).Accept(this);
                else if (expr is SeparatorPart)
                    ((SeparatorPart)expr).Accept(this);
            }
        }

        public void Visit(RestrictPart part)
        {
            _query.Append(" LIMIT " + part.Limit.ToString());
        }

        public void Visit(SeparatorPart part)
        {
            _query.Append(part.Delimiter + " ");
        }

        public void Visit(OperatorPart part)
        {
            _query.Append(" " + part.Type + " ");
        }

        public void Visit(ComparePart part)
        {
            _query.Append(" " + part.Type + " ");
        }

        public void Visit(OrderByPart part)
        {
            _query.Append(" ORDER BY ");

            foreach(var field in part.Fields)
                field.Accept(this);
        }

        public void Visit(OrderByPropertyPart part)
        {
            _query.Append(part.AliasIdentifier + "." + part.Name + " ");

            part.Direction.Accept(this);
        }

        public void Visit(DirectionPart part)
        {
            if (!string.IsNullOrEmpty(part.Type))
                _query.Append(part.Type);
        }

        #region private helpers

        private void DoRelationshipPart(RelationshipPart part, DirectionType direction)
        {
            if (direction == DirectionType.Forward)
            {
                _query.Append("-");

                part.Accept(this);

                _query.Append("->");
            }
            else if (direction == DirectionType.Backward)
            {
                _query.Append("<-");

                part.Accept(this);

                _query.Append("-");
            }
        }

        private void DoNodePart(NodePart part)
        {
            _query.Append("(");

            part.Accept(this);

            _query.Append(")");
        }

        private IEnumerable<Tree> UnwindTree(Tree nodes)
        {
            foreach (var node in nodes.Children)
            {
                yield return node;

                foreach (var child in UnwindTree(node))
                    yield return child;
            }
        }

        private Tree CreateTree(List<NodePart> parts)
        {
            var nodes = new List<TreeNode>
            {
                new TreeNode
                {
                    Name = "root",
                    Cardinality = string.Empty
                }
            };

            //determine parent node(s)
            foreach (var part in parts)
            {
                //determine if node is a child in the current list of nodes
                if (parts.Exists(x => x.Relations != null && x.Relations.Any(x => x.Reference == part.Name && x.Name != part.Name)))
                {
                    var parent = parts.Where(x => x.Relations != null && x.Relations.Any(x => x.Reference == part.Name && x.Name != part.Name)).FirstOrDefault();
                    //link child to parent
                    if (parent != null)
                    {
                        nodes.Add(
                            new TreeNode
                            {
                                Name = part.Name,
                                Parent = parent.Name,
                                Cardinality = parent.Relations.First(x => x.Reference == part.Name).Reference
                            });
                    }
                }
                else
                {
                    //parent
                    nodes.Add(
                        new TreeNode
                        {
                            Name = part.Name,
                            Parent = "root"
                        });
                }
            }

            return TreeBuilder.BuildTree(nodes);
        }

        #endregion
    }

    
    
    
    
}