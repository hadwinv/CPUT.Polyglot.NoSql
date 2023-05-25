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
                    _query.Append("OPTIONAL MATCH ");

                    foreach (var relation in node.Value)
                    {
                        var parentNode = nodes.First(x => x.Name == node.Key);
                        var relatedNode = nodes.First(x => x.Name == relation.Value);
                        var relationship = nodes.SelectMany(x => x.Relations.Where(x => x.Name == relation.Key)).First();

                        _query.Append("(" + parentNode.AliasIdentifier + ")");

                        if (direction == DirectionType.Forward)
                            DoRelationshipPart(relationship, DirectionType.Backward);

                        //add left then right node in loop
                        DoNodePart(relatedNode);
                    }
                }
                else
                {
                    _query.Append("MATCH ");

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

            foreach (var expr in part.Parts)
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
            _query.Append(" " + part.Type + "(");

            part.PropertyPart.Accept(this);

            _query.Append(") as ");

            if (part.PropertyPart is PropertyPart)
                _query.Append(((PropertyPart)part.PropertyPart).Name);
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
            _query.Append(" ORDER BY " + part.AliasIdentifier + "." +  part.Name + " ");

            part.Direction.Accept(this);
        }

        public void Visit(DirectionPart part)
        {
            if (!string.IsNullOrEmpty(part.Type))
                _query.Append(" " + part.Type + " ");
        }

        #region private helpers

        private void DoRelationshipPart(RelationshipPart part, DirectionType direction)
        {
            if (direction == DirectionType.Forward)
            {
                _query.Append("<-");

                part.Accept(this);

                _query.Append("-");
            }
            else if (direction == DirectionType.Backward)
            {
                _query.Append("-");

                part.Accept(this);

                _query.Append("->");
            }
        }

        private void DoNodePart(NodePart part)
        {
            _query.Append("(");

            part.Accept(this);

            _query.Append(")");
        }

        #endregion

    }
}