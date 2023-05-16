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

        public void Visit(CollectionPart collection)
        {
            _query.Append("db.getCollection(\"" + collection.Target + "\")");

            if(collection.Find != null)
                collection.Find.Accept(this);
            else if (collection.Aggregate != null)
                collection.Aggregate.Accept(this);
            else if (collection.Insert != null)
                collection.Insert.Accept(this);
            else if (collection.Update != null)
                collection.Update.Accept(this);
        }

        public void Visit(FindPart find)
        {
            _query.Append(".find(");

            bool hasFilter = false;

            if(find.Condition != null)
            {
                _query.Append("{");

                find.Condition.Accept(this);

                _query.Append("}");

                hasFilter = true;
            }

            if (find.Field != null)
            {
                if (!hasFilter)
                    _query.Append("{}");

                _query.Append(",{");

                find.Field.Accept(this);

                _query.Append("}");
            }

            _query.Append(")");

            if (find.OrderBy != null)
                find.OrderBy.Accept(this);

            if (find.Restrict != null)
                find.Restrict.Accept(this);
        }

        #region DML

        public void Visit(UpdatePart update)
        {
            _query.Append(".updateMany(");

            bool hasFilter = false;

            foreach (var expr in update.Parts)
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

        public void Visit(SetPart set)
        {
            _query.Append("$set: {");

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
            _query.Append("}");
        }

        public void Visit(SetValuePart setValue)
        {
            string[] types = { "$gt", "$gte", "$lt", "$lte" };
            
            _query.Append("\"" + setValue.Left.Name + "\"");

            if (types.Contains(setValue.Operator.Type))
            {
                _query.Append(": { ");
                _query.Append(setValue.Operator.Type + " : ");

                if (setValue.Right.Type == "string")
                    _query.Append("\"" + setValue.Right.Name + "\"");
                else if (setValue.Right.Type == "int")
                    _query.Append("NumberLong(" + setValue.Right.Name + ")");
                else
                    _query.Append(setValue.Right.Name);

                _query.Append(" } ");
            }
            else
            {
                _query.Append(" " + setValue.Operator.Type + " ");

                if (setValue.Right.Type == "string")
                    _query.Append("\"" + setValue.Right.Name + "\"");
                else
                    _query.Append(setValue.Right.Name);
            }
        }

        public void Visit(InsertPart insert)
        {
            _query.Append(".insertMany([");

            foreach(var expr in insert.Parts)
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

        #endregion

        #region Aggregation

        public void Visit(AggregatePart aggregate)
        {
            var blockAdded = false;

            _query.Append(".aggregate([");

            if (aggregate.Match != null)
            {
                aggregate.Match.Accept(this);

                blockAdded = true;
            }

            if (aggregate.Unwind != null)
            {
                if (blockAdded)
                    _query.Append(",");

                aggregate.Unwind.Accept(this);

                if (!blockAdded)
                    blockAdded = true;
            }

            if (aggregate.Project != null)
            {
                if (blockAdded)
                    _query.Append(",");

                aggregate.Project.Accept(this);

                if (!blockAdded)
                    blockAdded = true;
            }

            if (aggregate.GroupBy != null)
            {
                if (blockAdded)
                    _query.Append(",");

                aggregate.GroupBy.Accept(this);
            }

            if (aggregate.OrderBy != null)
                aggregate.OrderBy.Accept(this);

            if (aggregate.Restrict != null)
                aggregate.Restrict.Accept(this);

            _query.Append("])");
        }

        public void Visit(MatchPart match)
        {
            _query.Append("{ $match : {");

            var condition = (ConditionPart)match.Properties.SingleOrDefault(x => x.GetType().Equals(typeof(ConditionPart)));

            if (condition != null)
                condition.Accept(this);

            _query.Append("}}");
        }

        public void Visit(UnwindPart unwind)
        {
            if (unwind.Fields.Count() > 0)
            {
                _query.Append("{ $unwind : {");

                foreach (var exp in unwind.Fields)
                    exp.Accept(this);

                _query.Append("}}");
            }
        }

        public void Visit(ProjectPart project)
        {
            if (project.Fields.Count() > 0)
            {
                _query.Append("{ $project : { _id: \"$_id\", ");

                foreach (var part in project.Fields)
                    part.Accept(this);
                    
                _query.Append("}}");
            }
        }

        public void Visit(ProjectFieldPart field)
        {
            _query.Append(field.Alias + " : \"$" + field.Property + "\"");
        }

        public void Visit(GroupByPart groupBy)
        {
            if (groupBy.Fields.Count() > 0)
            {
                _query.Append("{ $group : { _id: \"$_id\", ");

                foreach (var part in groupBy.Fields)
                    part.Accept(this);

                _query.Append("}}");
            }
        }
        
        public void Visit(GroupByFieldPart field)
        {
            _query.Append(field.Alias + " : { \"$first\" : \"$" + field.Property + "\"}");
        }

        public void Visit(NativeFunctionPart function)
        {
            if(function.Type.ToLower() == "count")
                _query.Append( function.Alias + ": { $" + function.Type.ToLower() + ": {}");
            else
            {
                _query.Append(function.Alias + ": { $" + function.Type.ToLower() + ":");

                function.PropertyPart.Accept(this);
            }

            _query.Append("}");
        }

        public void Visit(FunctionFieldPart functionProperty)
        {
            _query.Append( " \"$" + functionProperty.Property + "\"");
        }

        #endregion

        public void Visit(ConditionPart condition)
        {
            var logicParts = condition.Logic.Where(x => x.GetType().Equals(typeof(LogicalPart))).Cast<LogicalPart>().ToList();//.OrderByDescending(x => x.Compare?.Type);

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

        public void Visit(LogicalPart logical)
        {
            string[] types = { "$gt", "$gte", "$lt", "$lte" };
            _query.Append("\"" + logical.Left.Name + "\"");

            if (types.Contains(logical.Operator.Type))
            {
                _query.Append(": { ");
                _query.Append( logical.Operator.Type + " : ");

                if (logical.Right.Type == "string")
                    _query.Append("\"" + logical.Right.Name + "\"" );
                else if (logical.Right.Type == "int")
                    _query.Append("NumberLong(" + logical.Right.Name + ")") ;
                else
                    _query.Append(logical.Right.Name);

                _query.Append(" } ");
            }
            else
            {
                _query.Append(" " + logical.Operator.Type + " ");

                if (logical.Right.Type == "string")
                    _query.Append("\"" + logical.Right.Name + "\"");
                else
                    _query.Append(logical.Right.Name);
            }
        }

        public void Visit(FieldPart field)
        {
            foreach (var expr in field.Fields)
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

        public void Visit(PropertyPart property)
        {
            _query.Append(property.Name);
        }

        public void Visit(UnwindJsonPart unwindJson)
        {
            _query.Append("path: \"$" + unwindJson.Name + "\"");
        }

        public void Visit(SeparatorPart separator)
        {
            _query.Append(separator.Delimiter + " ");
        }

        public void Visit(OperatorPart operatorPart)
        {
            _query.Append(" " + operatorPart.Type + " ");
        }

        public void Visit(ComparePart comparePart)
        {
            _query.Append("\"" + comparePart.Type + "\" : ");
        }

        public void Visit(OrderByPart orderBy)
        {
            if (_format == MongoDBFormat.Aggregate_Order)
                _query.Append(", { $sort : { " + orderBy.Name + " : 1 } }");
            else
            {
                _query.Append("._addSpecial(\"$orderby\", {" + orderBy.Name);

                orderBy.Direction.Accept(this);

                _query.Append("})");
            }
        }

        public void Visit(RestrictPart restrict)
        {
            if (_format == MongoDBFormat.Aggregate_Order)
                _query.Append(", { $limit : " + restrict.Limit + " }");
            else
                _query.Append(" .limit(" + restrict.Limit.ToString() + ")");
        }

        public void Visit(DirectionPart direction)
        {
            if (direction.Type == "DESC")
                _query.Append(" : -1 ");
            else
                _query.Append(" : 1 ");
        }
    }
}