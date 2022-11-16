using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using CPUT.Polyglot.NoSql.Models._data.prep;
using CPUT.Polyglot.NoSql.Models.Mapper;
using CPUT.Polyglot.NoSql.Parser;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.Neo4j;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Neo4j;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared.Operators;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Shared;
using MongoDB.Driver;
using Neo4jClient;
using Neo4jClient.Cypher;
using Pipelines.Sockets.Unofficial.Arenas;
using static System.Formats.Asn1.AsnWriter;
using static System.Net.Mime.MediaTypeNames;
using static CPUT.Polyglot.NoSql.Common.Parsers.Operators;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions
{
    public class Neo4jGenerator : INeo4jVisitor
    {
        StringBuilder Query;

        public Neo4jGenerator(StringBuilder query) => this.Query = query;

        public void Visit(QueryPart query)
        {
            foreach(var expr in query.Expressions)
            {
                if(expr is MatchPart)
                {
                    ((MatchPart)expr).Accept(this);
                }
                else if (expr is UnwindPart)
                {
                    ((UnwindPart)expr).Accept(this);
                }
                else if (expr is InsertPart)
                {
                    ((InsertPart)expr).Accept(this);
                }
                else if (expr is ConditionPart)
                {
                    ((ConditionPart)expr).Accept(this);
                }
                else if (expr is SetPart)
                {
                    ((SetPart)expr).Accept(this);
                }
                else if (expr is ReturnPart)
                {
                    ((ReturnPart)expr).Accept(this);
                }
                else if (expr is OrderByPart)
                {
                    ((OrderByPart)expr).Accept(this);
                }
                else if (expr is RestrictPart)
                {
                    ((RestrictPart)expr).Accept(this);
                }
            }
        }

        public void Visit(MatchPart match)
        {
            var nodes = match.Properties.Where(x => x.GetType().Equals(typeof(NodePart))).Cast<NodePart>().ToList();
            var parent = new Dictionary<string, Dictionary<string, string>>();

            if(nodes.Count > 1)
            {
                foreach (var node in nodes)
                {
                    var parentNode = nodes.Where(x => x.Relations.Exists(x => x.Reference == node.Name)).FirstOrDefault();

                    if (parentNode != null)
                    {
                        var relation = parentNode.Relations.First(x => x.Reference == node.Name);

                        if (!parent.ContainsKey(parentNode.Name))
                        {
                            parent.Add(parentNode.Name, new Dictionary<string, string> {
                                    {
                                            relation.Name,
                                            node.Name
                                    }});
                        }
                        else
                        {
                            parent[parentNode.Name].Add(relation.Name, node.Name);
                        }
                    }
                }
            }
            else
            {
                parent.Add(nodes[0].Name, new Dictionary<string, string>());
            }
            

            NodeDirection direction = NodeDirection.None;
            bool common = false;

            foreach (var node in parent)
            {
                Query.Append("MATCH ");

                if (node.Value.Count > 1)
                {
                    if (node.Value.Count > 2)
                        Query.Append("OPTIONAL ");

                    foreach (var relation in node.Value)
                    {
                        var relatedNode = nodes.First(x => x.Name == relation.Value);
                        var relationship = nodes.SelectMany(x => x.Relations.Where(x => x.Name == relation.Key)).First();

                        if (direction == NodeDirection.None)
                            direction = NodeDirection.Backward;
                        else
                            direction = NodeDirection.Forward;

                        if (direction == NodeDirection.Forward)
                            DoRelationshipPart(relationship, NodeDirection.Backward);

                        //add left then right node in loop
                        DoNodePart(relatedNode);

                        if (direction == NodeDirection.Backward)
                            DoRelationshipPart(relationship, NodeDirection.Forward);

                        if (!common)
                        {
                            DoNodePart(nodes.First(x => x.Name == node.Key));
                            common = true;
                        }
                    }
                }
                else
                {
                    DoNodePart(nodes.First(x => x.Name == node.Key));

                    foreach (var relation in node.Value)
                    {
                        var relatedNode = nodes.First(x => x.Name == relation.Value);
                        var relationship = nodes.SelectMany(x => x.Relations.Where(x => x.Name == relation.Key)).First();
                       
                        DoRelationshipPart(relationship, NodeDirection.Forward);

                        //add left then right node in loop
                     
                        DoNodePart(relatedNode);
                    }
                }
            }
        }

        private void DoRelationshipPart(RelationshipPart relationship, NodeDirection direction)
        {
            if(direction == NodeDirection.Forward)
            {
                Query.Append("-");

                relationship.Accept(this);

                Query.Append("->");
            }
            else if (direction == NodeDirection.Backward)
            {
                Query.Append("<-");

                relationship.Accept(this);

                Query.Append("-");
            }
        }

        private void DoNodePart(NodePart relatedNode)
        {
            Query.Append("(");

            relatedNode.Accept(this);

            Query.Append(")");
        }

        public void Visit(NodePart node)
        {
            Query.Append(node.Alias + ":" + node.Name);
        }

        public void Visit(RelationshipPart relationship)
        {
            Query.Append("[:" + relationship.Name + "]");
        }

        public void Visit(PropertyPart property)
        {
            Query.Append(property.Alias + "." + property.Name);
        }

        public void Visit(ConditionPart condition)
        {
            Query.Append(" WHERE ");

            foreach (var expr in condition.Logic)
            {
                if (expr is LogicalPart)
                {
                    ((LogicalPart)expr).Accept(this);
                }
                else if (expr is ConditionPart)
                {
                    ((ConditionPart)expr).Accept(this);
                }
            }
        }

        public void Visit(LogicalPart logic)
        {
            if (!string.IsNullOrEmpty(logic.Compare.Type))
                Query.Append(" " + logic.Compare.Type + " ");

            Query.Append(logic.Left.Alias + "." + logic.Left.Name);

            Query.Append(" " + logic.Operator.Type + " ");

            if (logic.Right.Type == "string")
                Query.Append("\"" + logic.Right.Name + "\"");
            else
                Query.Append(logic.Right.Name);
        }

        public void Visit(ReturnPart @return)
        {
            Query.Append(" RETURN ");

            foreach (var expr in @return.Properties)
            {
                if (expr is PropertyPart)
                {
                    ((PropertyPart)expr).Accept(this);
                }
                else if (expr is UnwindPropertyPart)
                {
                    ((UnwindPropertyPart)expr).Accept(this);
                }
                else if (expr is FunctionPart)
                {
                    ((FunctionPart)expr).Accept(this);
                }
                else if (expr is SeparatorPart)
                {
                    ((SeparatorPart)expr).Accept(this);
                }
            }
        }

        public void Visit(RestrictPart restrict)
        {
            Query.Append(" LIMIT " + restrict.Limit.ToString());
        }

        public void Visit(SeparatorPart separator)
        {
            Query.Append(separator.Delimiter + " ");
        }

        public void Visit(OperatorPart operatorPart)
        {
            Query.Append(" " + operatorPart.Type + " ");
        }

        public void Visit(ComparePart comparePart)
        {
            Query.Append(" " + comparePart.Type + " ");
        }

        public void Visit(SetPart set)
        {
            Query.Append(" SET ");

            foreach (var expr in set.Properties)
            {
                if (expr is SetValuePart)
                {
                    ((SetValuePart)expr).Accept(this);
                }
                else if (expr is SeparatorPart)
                {
                    ((SeparatorPart)expr).Accept(this); ;
                }
            }
        }

        public void Visit(SetValuePart setValue)
        {
            Query.Append(setValue.Left.Alias + "." + setValue.Left.Name + " " + setValue.Operator.Type + " ");

            if (setValue.Right.Type == "string")
                Query.Append("\"" + setValue.Right.Name + "\"");
            else
                Query.Append(setValue.Right.Name);
        }

        public void Visit(InsertPart insert)
        {
            Query.Append("CREATE ( ");

            foreach (var expr in insert.Properties)
            {
                if (expr is NodePart)
                {
                    ((NodePart)expr).Accept(this);
                }
                else if (expr is ValuesPart)
                {
                    ((ValuesPart)expr).Accept(this);
                }
            }

            Query.Append(" )");
        }

        public void Visit(ValuesPart values)
        {
            Query.Append(" { ");
            
            foreach (var expr in values.Properties)
            {
                if (expr is InsertNodePart)
                {
                    ((InsertNodePart)expr).Accept(this);
                }
                else if (expr is SeparatorPart)
                {
                    ((SeparatorPart)expr).Accept(this);
                }
            }

            Query.Append(" } ");
        }

        public void Visit(InsertNodePart insertNode)
        {
            Query.Append(insertNode.Left.Name + " : ");

            if (insertNode.Right.Type == "string")
                Query.Append("\"" + insertNode.Right.Name + "\"");
            else
                Query.Append(insertNode.Right.Name);
        }

        public void Visit(OrderByPart orderBy)
        {
            Query.Append(" ORDER BY " + orderBy.Alias + "." +  orderBy.Name + " ");

            orderBy.Direction.Accept(this);
        }

        public void Visit(DirectionPart direction)
        {
            if (!string.IsNullOrEmpty(direction.Type))
                Query.Append(" " + direction.Type + " ");
        }

        public void Visit(FunctionPart function)
        {
            Query.Append(" " + function.Type + "(");

            function.PropertyPart.Accept(this);
            
            Query.Append(") as ");

            if (function.PropertyPart is PropertyPart)
                Query.Append(((PropertyPart)function.PropertyPart).Name);
            else if (function.PropertyPart is UnwindPropertyPart)
                Query.Append(((UnwindPropertyPart)function.PropertyPart).Name);
        }

        public void Visit(UnwindPart unwind)
        {
            Query.Append(" UNWIND ");

            foreach(var expr in unwind.Expressions)
            {
                if (expr is UnwindJsonPart)
                {
                    var property = ((UnwindJsonPart)expr);

                    Query.Append("apoc.convert.fromJsonList( ");

                    property.Accept(this);

                    Query.Append(") as " + property.UnwindAlias);
                }
                else if (expr is SeparatorPart)
                {
                    ((SeparatorPart)expr).Accept(this);
                }
            }
        }

        public void Visit(UnwindPropertyPart unwindProperty)
        {
            Query.Append(unwindProperty.Alias + "." + unwindProperty.Name);
        }

        public void Visit(UnwindJsonPart unwindJson)
        {
            Query.Append(unwindJson.Alias + "." + unwindJson.Name);
        }
    }
}