using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.MongoDb;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared.Operators;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Shared;
using System.Text;
using static CPUT.Polyglot.NoSql.Common.Parsers.Operators;
using UpdatePart = CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared.UpdatePart;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions
{
    public class MongoDbGenerator : IMongoDbVisitor
    {
        private StringBuilder _query;
        private MongoDBFormat _format;

        public MongoDbGenerator(StringBuilder query, MongoDBFormat format)
        {
            this._query = query; 
            this._format = format;
        }

        public void Visit(QueryPart query)
        {
            foreach (var expr in query.Expressions)
            {
                if (expr is CollectionPart)
                {
                    var collection = ((CollectionPart)expr);

                    collection.Accept(this);
                }
            }
        }

        public void Visit(CollectionPart part)
        {
            _query.Append("db.getCollection(\"" + part.Target + "\")");

            if(part.Find != null)
                part.Find.Accept(this);
            else if (part.Aggregate != null)
                part.Aggregate.Accept(this);
            else if (part.Insert != null)
                part.Insert.Accept(this);
            else if (part.Update != null)
                part.Update.Accept(this);
        }

        public void Visit(FindPart part)
        {
            _query.Append(".find(");

            bool hasFilter = false;

            if(part.Condition != null)
            {
                _query.Append("{");

                part.Condition.Accept(this);

                _query.Append("}");

                hasFilter = true;
            }

            if (part.Field != null)
            {
                if (!hasFilter)
                    _query.Append("{}");

                _query.Append(",{");

                part.Field.Accept(this);

                _query.Append("}");
            }

            _query.Append(")");

            if (part.OrderBy != null)
                part.OrderBy.Accept(this);

            if (part.Restrict != null)
                part.Restrict.Accept(this);
        }

        #region DML

        public void Visit(UpdatePart part)
        {
            _query.Append(".updateMany(");

            bool hasFilter = false;

            foreach (var expr in part.Properties)
            {
                if (expr is ConditionPart)
                {
                    _query.Append("{");

                    ((ConditionPart)expr).Accept(this);

                    hasFilter = true;

                    _query.Append("}");
                }
                else if (expr is SetPart)
                {
                    if (!hasFilter)
                        _query.Append("{}");

                    _query.Append(",{");

                    ((SetPart)expr).Accept(this);

                    _query.Append("}");
                }
            }

            _query.Append(")");
        }

        public void Visit(SetPart part)
        {
            _query.Append("$set: {");

            foreach (var expr in part.Properties)
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
            _query.Append("}");
        }

        public void Visit(SetValuePart part)
        {
            string[] types = { "$gt", "$gte", "$lt", "$lte" };
            
            _query.Append("\"" + part.Left.Name + "\"");

            if (types.Contains(part.Operator.Type))
            {
                _query.Append(": { ");
                _query.Append(part.Operator.Type + " : ");

                if (part.Right.Type == "string")
                    _query.Append("\"" + part.Right.Name + "\"");
                else if (part.Right.Type == "int")
                    _query.Append("NumberLong(" + part.Right.Name + ")");
                else
                    _query.Append(part.Right.Name);

                _query.Append(" } ");
            }
            else
            {
                _query.Append(" " + part.Operator.Type + " ");

                if (part.Right.Type == "string")
                    _query.Append("\"" + part.Right.Name + "\"");
                else
                    _query.Append(part.Right.Name);
            }
        }

        public void Visit(InsertPart part)
        {
            _query.Append(".insertMany([");

            foreach(var expr in part.Properties)
            {
                if (expr is AddPart)
                {
                    _query.Append("{");

                    ((AddPart)expr).Accept(this);

                    _query.Append("}");
                }
                else if (expr is SeparatorPart)
                    ((SeparatorPart)expr).Accept(this);
            }

            _query.Append("])");
        }

        public void Visit(AddPart part)
        {
            foreach (var expr in part.Properties)
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

        #endregion

        #region Aggregation

        public void Visit(AggregatePart part)
        {
            var blockAdded = false;

            _query.Append(".aggregate([");

            if (part.Match != null)
            {
                part.Match.Accept(this);

                blockAdded = true;
            }

            if (part.Unwind != null)
            {
                if (blockAdded)
                    _query.Append(",");

                part.Unwind.Accept(this);

                if (!blockAdded)
                    blockAdded = true;
            }

            if (part.Project != null)
            {
                if (blockAdded)
                    _query.Append(",");

                part.Project.Accept(this);

                if (!blockAdded)
                    blockAdded = true;
            }

            if (part.GroupBy != null)
            {
                if (blockAdded)
                    _query.Append(",");

                part.GroupBy.Accept(this);
            }

            if (part.OrderBy != null)
                part.OrderBy.Accept(this);

            if (part.Restrict != null)
                part.Restrict.Accept(this);

            _query.Append("])");
        }

        public void Visit(MatchPart part)
        {
            _query.Append("{ $match : {");

            var condition = (ConditionPart)part.Properties.SingleOrDefault(x => x.GetType().Equals(typeof(ConditionPart)));

            if (condition != null)
                condition.Accept(this);

            _query.Append("}}");
        }

        public void Visit(UnwindGroupPart part)
        {
            if (part.Fields.Count() > 0)
            {
                _query.Append("{ $unwind : {");

                foreach (var exp in part.Fields)
                    exp.Accept(this);

                _query.Append("}}");
            }
        }

        public void Visit(ProjectPart part)
        {
            if (part.Fields.Count() > 0)
            {
                _query.Append("{ $project : { _id: \"$_id\", ");

                foreach (var field in part.Fields)
                    field.Accept(this);
                    
                _query.Append("}}");
            }
        }

        public void Visit(ProjectFieldPart part)
        {
            _query.Append(part.Alias + " : \"$" + part.Property + "\"");
        }

        public void Visit(GroupByPart part)
        {
            if (part.Fields.Count() > 0)
            {
                _query.Append("{ $group : { _id: \"$_id\", ");

                foreach (var field in part.Fields)
                    field.Accept(this);

                _query.Append("}}");
            }
        }
        
        public void Visit(GroupByFieldPart part)
        {
            _query.Append(part.Alias + " : { \"$first\" : \"$" + part.Property + "\"}");
        }

        public void Visit(NativeFunctionPart part)
        {
            if(part.Type.ToLower() == "count")
                _query.Append( part.Alias + ": { $" + part.Type.ToLower() + ": {}");
            else
            {
                _query.Append(part.Alias + ": { $" + part.Type.ToLower() + ":");

                part.Property.Accept(this);
            }

            _query.Append("}");
        }

        public void Visit(FunctionFieldPart part)
        {
            _query.Append( " \"$" + part.Property + "\"");
        }

        #endregion

        public void Visit(ConditionPart part)
        {
            var logicParts = part.Logic.Where(x => x.GetType().Equals(typeof(LogicalPart))).Cast<LogicalPart>().ToList();//.OrderByDescending(x => x.Compare?.Type);

            int logicCount = 0;
            bool openCollection = false;
            
            string? prevType = string.Empty;

            if (logicParts.Count > 1)
                logicParts[0].Compare.Type = logicParts[1].Compare.Type;

            foreach (var logic in logicParts)
            {
                if (string.IsNullOrEmpty(logic.Compare?.Type))
                    logic.Compare.Type = prevType;

                if (prevType != logic.Compare?.Type)
                {
                    if(openCollection)
                    {
                        _query.Append("],");
                        openCollection = false;
                        logicCount = 0;
                    }

                    logic.Compare?.Accept(this);

                    if (logicCount == 0 && logicParts.Count() > 1)
                    {
                        _query.Append("[");
                        openCollection = true;
                    }
                }

                if (logicParts.Count() > 1)
                {
                    if(logicCount > 0)
                        _query.Append(",");

                    _query.Append("{");  

                    logic.Accept(this);

                    _query.Append("}");
                }
                else
                    logic.Accept(this);

                prevType = logic.Compare?.Type;
                logicCount++;
            }

            if (openCollection)
            {
                _query.Append("]");
                openCollection = false;
            }
        }

        public void Visit(LogicalPart part)
        {
            string[] types = { "$gt", "$gte", "$lt", "$lte" };
            _query.Append("\"" + part.Left.Name + "\"");

            if (types.Contains(part.Operator.Type))
            {
                _query.Append(": { ");
                _query.Append( part.Operator.Type + " : ");

                if (part.Right.Type == "string")
                    _query.Append("\"" + part.Right.Name + "\"" );
                else if (part.Right.Type == "int")
                    _query.Append("NumberLong(" + part.Right.Name + ")") ;
                else
                    _query.Append(part.Right.Name);

                _query.Append(" } ");
            }
            else
            {
                _query.Append(" " + part.Operator.Type + " ");

                if (part.Right.Type == "string")
                    _query.Append("\"" + part.Right.Name + "\"");
                else
                    _query.Append(part.Right.Name);
            }
        }

        public void Visit(OperatorPart part)
        {
            _query.Append(" " + part.Type + " ");
        }

        public void Visit(ComparePart part)
        {
            _query.Append("\"" + part.Type + "\" : ");
        }

        public void Visit(FieldPart part)
        {
            foreach (var expr in part.Fields)
            {
                if (expr is PropertyPart)
                {
                    ((PropertyPart)expr).Accept(this);
                         
                    _query.Append(" : 1"); 
                }
                else if (expr is SeparatorPart)
                {
                    ((SeparatorPart)expr).Accept(this); ;
                }
            }
        }

        public void Visit(PropertyPart part)
        {
            _query.Append(part.Name);
        }

        public void Visit(UnwindJsonPart part)
        {
            _query.Append("path: \"$" + part.Name + "\"");
        }

        public void Visit(OrderByPart part)
        {
            if (_format == MongoDBFormat.Aggregate_Order)
            {
                _query.Append(", { $sort : { ");

                foreach (var field in part.Fields)
                    field.Accept(this);

                _query.Append(" } }");
            }
                
            else
            {
                _query.Append("._addSpecial(\"$orderby\", {" );

                foreach (var field in part.Fields)
                    field.Accept(this);

                _query.Append("})");
            }
        }

        public void Visit(OrderByPropertyPart part)
        {
            _query.Append(part.Name + " ");

            part.Direction.Accept(this);
        }

        public void Visit(RestrictPart part)
        {
            if (_format == MongoDBFormat.Aggregate_Order)
                _query.Append(", { $limit : " + part.Limit + " }");
            else
                _query.Append(" .limit(" + part.Limit.ToString() + ")");
        }

        public void Visit(SeparatorPart part)
        {
            _query.Append(part.Delimiter + " ");
        }

        public void Visit(DirectionPart part)
        {
            if (part.Type == "DESC")
                _query.Append(" : -1 ");
            else
                _query.Append(" : 1 ");
        }
    }
}