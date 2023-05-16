using System.Text;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.Neo4j;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Neo4j;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared.Operators;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Shared;
using MongoDB.Driver;
using Pipelines.Sockets.Unofficial.Arenas;
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
                            parent[parentNode.Name].Add(relation.Name, node.Name);
                    }
                }
            }
            else
                parent.Add(nodes[0].Name, new Dictionary<string, string>());
            

            DirectionType direction = DirectionType.None;
            bool common = false;
            int parentCount = 0;

            foreach (var node in parent)
            {
                if(parentCount > 0)
                {
                    Query.Append("OPTIONAL MATCH ");

                    foreach (var relation in node.Value)
                    {
                        var parentNode = nodes.First(x => x.Name == node.Key);
                        var relatedNode = nodes.First(x => x.Name == relation.Value);
                        var relationship = nodes.SelectMany(x => x.Relations.Where(x => x.Name == relation.Key)).First();

                        Query.Append("(" + parentNode.AliasIdentifier + ")");

                        if (direction == DirectionType.Forward)
                            DoRelationshipPart(relationship, DirectionType.Backward);

                        //add left then right node in loop
                        DoNodePart(relatedNode);
                    }
                }
                else
                {
                    Query.Append("MATCH ");

                    if (node.Value.Count > 1)
                    {
                        foreach (var relation in node.Value)
                        {
                            var relatedNode = nodes.First(x => x.Name == relation.Value);
                            var relationship = nodes.SelectMany(x => x.Relations.Where(x => x.Name == relation.Key)).First();

                            if (direction == DirectionType.None)
                                direction = DirectionType.Backward;
                            else
                                direction = DirectionType.Forward;

                            if (direction == DirectionType.Forward)
                                DoRelationshipPart(relationship, DirectionType.Backward);

                            //add left then right node in loop
                            DoNodePart(relatedNode);

                            if (direction == DirectionType.Backward)
                                DoRelationshipPart(relationship, DirectionType.Forward);

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

                            if (node.Value.Count == 1)
                                DoRelationshipPart(relationship, DirectionType.Backward);
                            else
                                DoRelationshipPart(relationship, DirectionType.Forward);
                            //add left then right node in loop

                            DoNodePart(relatedNode);
                        }
                    }

                }

                parentCount++;
            }
        }

        private void DoRelationshipPart(RelationshipPart relationship, DirectionType direction)
        {
            if(direction == DirectionType.Forward)
            {
                Query.Append("<-");

                relationship.Accept(this);

                Query.Append("-");
            }
            else if (direction == DirectionType.Backward)
            {
                Query.Append("-");

                relationship.Accept(this);

                Query.Append("->");
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
            Query.Append(node.AliasIdentifier + ":" + node.Name);
        }

        public void Visit(RelationshipPart relationship)
        {
            Query.Append("[:" + relationship.Name + "]");
        }

        public void Visit(PropertyPart property)
        {
            if(!string.IsNullOrEmpty(property.AliasIdentifier))
                Query.Append(property.AliasIdentifier + ".");

            Query.Append(property.Name);

            if (!string.IsNullOrEmpty(property.AliasName))
                Query.Append("AS " + property.AliasName);
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

            logic.Left.Accept(this);

            Query.Append(" " + logic.Operator.Type + " ");

            if (logic.Right.Type == "string")
                Query.Append("\"" + logic.Right.Name + "\"");
            else
                logic.Right.Accept(this);
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
                else if (expr is NativeFunctionPart)
                {
                    ((NativeFunctionPart)expr).Accept(this);
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
            setValue.Left.Accept(this);

            Query.Append(setValue.Operator.Type + " ");

            if (setValue.Right.Type == "string")
                Query.Append("\"" + setValue.Right.Name + "\"");
            else
                setValue.Right.Accept(this);
        }

        public void Visit(InsertPart insert)
        {
            Query.Append("CREATE ( ");

            foreach (var expr in insert.Parts)
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
            Query.Append(" ORDER BY " + orderBy.AliasIdentifier + "." +  orderBy.Name + " ");

            orderBy.Direction.Accept(this);
        }

        public void Visit(DirectionPart direction)
        {
            if (!string.IsNullOrEmpty(direction.Type))
                Query.Append(" " + direction.Type + " ");
        }

        public void Visit(NativeFunctionPart function)
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

            foreach(var expr in unwind.Fields)
            {
                if (expr is UnwindJsonPart)
                {
                    var property = ((UnwindJsonPart)expr);

                    Query.Append("apoc.convert.fromJsonList( ");

                    property.Accept(this);

                    Query.Append(") as " + property.UnwindAliasIdentifier);
                }
                else if (expr is SeparatorPart)
                {
                    ((SeparatorPart)expr).Accept(this);
                }
            }
        }

        public void Visit(UnwindPropertyPart unwindProperty)
        {
            if (!string.IsNullOrEmpty(unwindProperty.AliasIdentifier))
                Query.Append(unwindProperty.AliasIdentifier + ".");

            Query.Append(unwindProperty.Name);

            if (!string.IsNullOrEmpty(unwindProperty.AliasName))
                Query.Append("AS " + unwindProperty.AliasName);

        }

        public void Visit(UnwindJsonPart unwindJson)
        {
            Query.Append(unwindJson.AliasIdentifier + "." + unwindJson.Name);
        }
    }
}