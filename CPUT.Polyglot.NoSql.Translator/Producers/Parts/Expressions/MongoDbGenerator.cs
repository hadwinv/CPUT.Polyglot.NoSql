using CPUT.Polyglot.NoSql.Parser;
using CPUT.Polyglot.NoSql.Parser.Syntax.Component;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.Neo4j;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Base;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Cassandra;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.MongoDb;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared.Operators;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Shared;
using Microsoft.VisualBasic;
using Neo4jClient.Cypher;
using StackExchange.Redis;
using Superpower.Model;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using static CPUT.Polyglot.NoSql.Common.Parsers.Operators;
using UpdatePart = CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared.UpdatePart;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions
{
    public class MongoDbGenerator : IMongoDbVisitor
    {
        StringBuilder Query;

        public MongoDbGenerator(StringBuilder query) => this.Query = query;

        public void Visit(QueryPart query)
        {
            foreach (var expr in query.Expressions)
            {
                if (expr is GetCollectionPart)
                {
                    ((GetCollectionPart)expr).Accept(this);
                }
                else if (expr is FindPart)
                {
                    ((FindPart)expr).Accept(this);
                }
                else if (expr is UpdatePart)
                {
                    ((UpdatePart)expr).Accept(this);
                }
                else if (expr is InsertPart)
                {
                    ((InsertPart)expr).Accept(this);
                }
                else if (expr is OrderByPart)
                {
                    ((OrderByPart)expr).Accept(this);
                }
            }

            var restrictPart = (RestrictPart?)query.Expressions.SingleOrDefault(x => x.GetType().Equals(typeof(RestrictPart)));

            if(restrictPart != null)
                restrictPart.Accept(this);
        }

        public void Visit(GetCollectionPart collection)
        {
            Query.Append("db.getCollection(\"" + collection.Name + "\")");
        }

        public void Visit(FindPart find)
        {
            Query.Append(".find(");

            bool hasFilter = false;

            foreach (var expr in find.Properties)
            {
                if (expr is ConditionPart)
                {
                    Query.Append("{");

                    ((ConditionPart)expr).Accept(this);

                    hasFilter = true;

                    Query.Append("}");
                }
                else if (expr is FieldPart)
                {
                    if(!hasFilter)
                        Query.Append("{}");

                    Query.Append(",{");

                    ((FieldPart)expr).Accept(this);

                    Query.Append("}");
                }
            }

            Query.Append(")");
        }

        public void Visit(UpdatePart update)
        {
            Query.Append(".updateMany(");

            bool hasFilter = false;

            foreach (var expr in update.Properties)
            {
                if (expr is ConditionPart)
                {
                    Query.Append("{");

                    ((ConditionPart)expr).Accept(this);

                    hasFilter = true;

                    Query.Append("}");
                }
                else if (expr is SetPart)
                {
                    if (!hasFilter)
                        Query.Append("{}");

                    Query.Append(",{");

                    ((SetPart)expr).Accept(this);

                    Query.Append("}");
                }
            }

            Query.Append(")");
        }

        public void Visit(SetPart set)
        {
            Query.Append("$set: {");

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
            Query.Append("}");
        }

        public void Visit(SetValuePart setValue)
        {
            string[] types = { "$gt", "$gte", "$lt", "$lte" };
            Query.Append("\"" + setValue.Left.Name + "\"");

            if (types.Contains(setValue.Operator.Type))
            {
                Query.Append(": { ");
                Query.Append(setValue.Operator.Type + " : ");

                if (setValue.Right.Type == "string")
                    Query.Append("\"" + setValue.Right.Name + "\"");
                else if (setValue.Right.Type == "int")
                    Query.Append("NumberLong(" + setValue.Right.Name + ")");
                else
                    Query.Append(setValue.Right.Name);

                Query.Append(" } ");
            }
            else
            {
                Query.Append(" " + setValue.Operator.Type + " ");

                if (setValue.Right.Type == "string")
                    Query.Append("\"" + setValue.Right.Name + "\"");
                else
                    Query.Append(setValue.Right.Name);
            }
        }

        public void Visit(InsertPart insert)
        {
            Query.Append(".insertMany([");

            foreach (var expr in insert.Properties)
            {
                if (expr is AddPart)
                {
                    Query.Append("{");

                    ((AddPart)expr).Accept(this);

                    Query.Append("}");
                }
            }

            Query.Append("])");
        }

        public void Visit(AddPart add)
        {
            foreach (var expr in add.Properties)
            {
                if (expr is SetValuePart)
                {
                    ((SetValuePart)expr).Accept(this);
                }
                else if (expr is SeparatorPart)
                {
                    ((SeparatorPart)expr).Accept(this);
                }
            }
        }

        public void Visit(ConditionPart condition)
        {
            var logicParts = condition.Logic.Where(x => x.GetType().Equals(typeof(LogicalPart))).Cast<LogicalPart>().ToList().OrderByDescending(x => x.Compare?.Type);
            var conditionParts = condition.Logic.Where(x => x.GetType().Equals(typeof(ConditionPart))).Cast<ConditionPart>().ToList();
            var compareParts = logicParts.Where(x => !string.IsNullOrEmpty( x.Compare.Type)).Select(x => x.Compare);

            int logicCount = 0;
            bool openCollection = false;
            
            string? prevType = string.Empty;

            foreach (var logic in logicParts)
            {
                if (string.IsNullOrEmpty(logic.Compare?.Type))
                    logic.Compare.Type = prevType;

                if (prevType != logic.Compare?.Type)
                {
                    if(openCollection)
                    {
                        Query.Append("],");
                        openCollection = false;
                        logicCount = 0;
                    }

                    logic.Compare?.Accept(this);

                    if (logicCount == 0 && logicParts.Count() > 1)
                    {
                        Query.Append("[");
                        openCollection = true;
                    }
                }

                if (logicParts.Count() > 1)
                {
                    if(logicCount > 0)
                        Query.Append(",");

                    Query.Append("{");  

                    logic.Accept(this);

                    Query.Append("}");
                }
                else
                    logic.Accept(this);

                prevType = logic.Compare?.Type;
                logicCount++;
            }

            if (openCollection)
            {
                Query.Append("]");
                openCollection = false;
            }
        }

        public void Visit(LogicalPart logical)
        {
            string[] types = { "$gt", "$gte", "$lt", "$lte" };
            Query.Append("\"" + logical.Left.Name + "\"");

            if (types.Contains(logical.Operator.Type))
            {
                Query.Append(": { ");
                Query.Append( logical.Operator.Type + " : ");

                if (logical.Right.Type == "string")
                    Query.Append("\"" + logical.Right.Name + "\"" );
                else if (logical.Right.Type == "int")
                    Query.Append("NumberLong(" + logical.Right.Name + ")") ;
                else
                    Query.Append(logical.Right.Name);

                Query.Append(" } ");
            }
            else
            {
                Query.Append(" " + logical.Operator.Type + " ");

                if (logical.Right.Type == "string")
                    Query.Append("\"" + logical.Right.Name + "\"");
                else
                    Query.Append(logical.Right.Name);
            }
        }

        public void Visit(FieldPart field)
        {
            foreach (var expr in field.Fields)
            {
                if (expr is PropertyPart)
                {
                    ((PropertyPart)expr).Accept(this);
                         
                    Query.Append(" : 1"); 
                }
                else if (expr is SeparatorPart)
                {
                    ((SeparatorPart)expr).Accept(this); ;
                }
            }
        }

        public void Visit(RestrictPart restrict)
        {
            Query.Append(" .limit(" + restrict.Limit.ToString() + ")");
        }

        public void Visit(PropertyPart property)
        {
            //if (!string.IsNullOrEmpty(property.AliasIdentifier))
            //    Query.Append(property.AliasIdentifier + ".");

            Query.Append(property.Name);

            if (!string.IsNullOrEmpty(property.AliasName))
                Query.Append("AS " + property.AliasName);
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
            Query.Append("\"" + comparePart.Type + "\" : ");
        }

        public void Visit(CreateCollectionPart createCollectionPart)
        {
            throw new NotImplementedException();
        }

        public void Visit(OrderByPart orderBy)
        {
            Query.Append("._addSpecial(\"$orderby\", {" + orderBy.Name);

            orderBy.Direction.Accept(this);

            Query.Append("})");
        }

        public void Visit(DirectionPart direction)
        {
            if (direction.Type == "DESC")
                Query.Append(" : -1 ");
            else
                Query.Append(" : 1 ");
        }

        public void Visit(AggregatePart aggregatePart)
        {
            throw new NotImplementedException();
        }
    }
}