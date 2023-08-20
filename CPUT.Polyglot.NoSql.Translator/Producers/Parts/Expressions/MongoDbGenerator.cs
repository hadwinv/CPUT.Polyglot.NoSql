using CPUT.Polyglot.NoSql.Common.Helpers;
using CPUT.Polyglot.NoSql.Models._data.prep.MongoDb;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.MongoDb;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared.Operators;
using CPUT.Polyglot.NoSql.Translator.Producers.Parts.Shared;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text;
using static CPUT.Polyglot.NoSql.Common.Parsers.Operators;
using UpdatePart = CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions.NoSql.Shared.UpdatePart;

namespace CPUT.Polyglot.NoSql.Translator.Producers.Parts.Expressions
{
    public class MongoDbGenerator : IMongoDbVisitor
    {
        private StringBuilder _query;
        private MongoDBFetchType _format;

        public MongoDbGenerator(StringBuilder query, MongoDBFetchType format)
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
            _query.Append("{ find: '" + part.CollectionName + "',");

            if(part.Condition != null)
            {
                _query.Append("filter: ");

                part.Condition.Accept(this);

                _query.Append(", ");
            }

            if (part.Field != null)
            {
                part.Field.Accept(this);

                _query.Append(", ");
            }
                

            if (part.OrderBy != null)
            {
                part.OrderBy.Accept(this);

                _query.Append(", ");
            }
                

            if (part.Restrict != null)
            {
                part.Restrict.Accept(this);

                _query.Append(", ");
            }

            for (int i = (_query.Length - 1); i >= 0; i--)
            {
                if (_query[i] == ',')
                {
                   _query.Remove(i, 1);
                    break;
                }
            }

            _query.Append("}");
        }

        #region DML

        public void Visit(UpdatePart part)
        {
            _query.Append("{ update: '" + part.CollectionName + "',");
            _query.Append("updates: [{");

            bool hasFilter = false;

            foreach (var expr in part.Properties)
            {
                if (expr is ConditionPart)
                {
                    _query.Append("q:");

                    ((ConditionPart)expr).Accept(this);

                    hasFilter = true;
                }
                else if (expr is SetPart)
                {
                    if (!hasFilter)
                        _query.Append("q:{}");

                    _query.Append(", u: {");

                    ((SetPart)expr).Accept(this);

                    _query.Append("}");
                }
            }

            _query.Append("}]}");
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
            
            _query.Append( part.Left.Name);

            if (types.Contains(part.Operator.Type))
            {
                _query.Append(": { ");
                _query.Append(part.Operator.Type + " : ");

                if (part.Right.Type == "string")
                    _query.Append("'" + part.Right.Name + "'");
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
                    _query.Append("'" + part.Right.Name + "'");
                else
                    _query.Append(part.Right.Name);
            }
        }

        public void Visit(InsertPart part)
        {
            _query.Append("{ insert: '" + part.CollectionName + "',");
            _query.Append("documents: [");

            foreach (var expr in part.Properties)
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

            _query.Append("]}");
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
            _query.Append("{ aggregate: '" + part.CollectionName + "',");
            _query.Append(" pipeline: [ ");
    
            if (part.Match != null)
            {
                part.Match.Accept(this);

                _query.Append(",");
            }


            if (part.Unwind != null)
            {
                part.Unwind.Accept(this);

                _query.Append(",");
            }

            if (part.Project != null)
            {
                part.Project.Accept(this);

               _query.Append(",");
            }
           

            if (part.GroupBy != null)
            {
                part.GroupBy.Accept(this);

                _query.Append(",");
            }

            if (part.OrderBy != null)
            {
                _query.Append("{");

                part.OrderBy.Accept(this);

                _query.Append("}");
                _query.Append(",");
            }

            if (part.Restrict != null)
            {
                _query.Append("{");

                part.Restrict.Accept(this);

                _query.Append("}");
            }
            else
            {
                for (int i = (_query.Length - 1); i >= 0; i--)
                {
                    if (_query[i] == ',')
                    {
                        _query.Remove(i, 1);
                        break;
                    }
                }
            }

            _query.Append("],");
            _query.Append("cursor: { }");
            _query.Append("}");
        }

        public void Visit(MatchPart part)
        {
            _query.Append("{ $match : ");

            var condition = (ConditionPart)part.Properties.SingleOrDefault(x => x.GetType().Equals(typeof(ConditionPart)));

            if (condition != null)
                condition.Accept(this);

            _query.Append("}");
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
                _query.Append("{ $project : { _id: '$_id', ");

                foreach (var field in part.Fields)
                    field.Accept(this);
                    
                _query.Append("}}");
            }
        }

        public void Visit(ProjectFieldPart part)
        {
            if(!part.Ignore)
                _query.Append(part.Alias + " : '$" + part.Name + "'");
        }

        public void Visit(GroupByPart part)
        {
            if (part.Fields.Count() > 0)
            {
                _query.Append("{ $group : { _id: '$_id', ");

                foreach (var field in part.Fields)
                    field.Accept(this);

                _query.Append("}}");
            }
        }
        
        public void Visit(GroupByFieldPart part)
        {
            if (!part.Ignore)
                _query.Append(part.Alias + " : { '$first' : '$" + part.Name + "'}");
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
            _query.Append(" '$" + part.Name + "'");
        }

        #endregion

        public void Visit(ConditionPart part)
        {
            var logicParts = part.Logic.Where(x => x.GetType().Equals(typeof(LogicalPart))).Cast<LogicalPart>().ToList();

            var conditions = Builders<object>.Filter.Empty;

            foreach (var logic in logicParts)
            {
                if (string.IsNullOrEmpty( logic.Compare.Type))
                {
                    if (logic.Operator.Type == ":")
                        conditions = Builders<object>.Filter.Eq(logic.Left.Name.IndexOf(".") > -1 ? "'" + logic.Left.Name +  "'" : logic.Left.Name, logic.Right.Type == "string" ? "'" + logic.Right.Name + "'" : logic.Right.Name);
                    if (logic.Operator.Type == "$gt")
                        conditions = Builders<object>.Filter.Gt(logic.Left.Name.IndexOf(".") > -1 ? "'" + logic.Left.Name + "'" : logic.Left.Name, logic.Right.Type == "string" ? "'" + logic.Right.Name + "'" : logic.Right.Name);
                    if (logic.Operator.Type == "$gte")
                        conditions = Builders<object>.Filter.Gte(logic.Left.Name.IndexOf(".") > -1 ? "'" + logic.Left.Name + "'" : logic.Left.Name, logic.Right.Type == "string" ? "'" + logic.Right.Name + "'" : logic.Right.Name);
                    if (logic.Operator.Type == "$lt")
                        conditions = Builders<object>.Filter.Lt(logic.Left.Name.IndexOf(".") > -1 ? "'" + logic.Left.Name + "'" : logic.Left.Name, logic.Right.Type == "string" ? "'" + logic.Right.Name + "'" : logic.Right.Name);
                    if (logic.Operator.Type == "$lte")
                        conditions = Builders<object>.Filter.Lte(logic.Left.Name.IndexOf(".") > -1 ? "'" + logic.Left.Name + "'" : logic.Left.Name, logic.Right.Type == "string" ? "'" + logic.Right.Name + "'" : logic.Right.Name);
                }
                else
                {
                    if (logic.Compare.Type == "$and")
                    {
                        if (logic.Operator.Type == ":")
                            conditions &= Builders<object>.Filter.Eq(logic.Left.Name.IndexOf(".") > -1 ? "'" + logic.Left.Name + "'" : logic.Left.Name, logic.Right.Type == "string" ? "'" + logic.Right.Name + "'" : logic.Right.Name);
                        if (logic.Operator.Type == "$gt")
                            conditions &= Builders<object>.Filter.Gt(logic.Left.Name.IndexOf(".") > -1 ? "'" + logic.Left.Name + "'" : logic.Left.Name, logic.Right.Type == "string" ? "'" + logic.Right.Name + "'" : logic.Right.Name);
                        if (logic.Operator.Type == "$gte")
                            conditions &= Builders<object>.Filter.Gte(logic.Left.Name.IndexOf(".") > -1 ? "'" + logic.Left.Name + "'" : logic.Left.Name, logic.Right.Type == "string" ? "'" + logic.Right.Name + "'" : logic.Right.Name);
                        if (logic.Operator.Type == "$lt")
                            conditions &= Builders<object>.Filter.Lt(logic.Left.Name.IndexOf(".") > -1 ? "'" + logic.Left.Name + "'" : logic.Left.Name, logic.Right.Type == "string" ? "'" + logic.Right.Name + "'" : logic.Right.Name);
                        if (logic.Operator.Type == "$lte")
                            conditions &= Builders<object>.Filter.Lte(logic.Left.Name.IndexOf(".") > -1 ? "'" + logic.Left.Name + "'" : logic.Left.Name, logic.Right.Type == "string" ? "'" + logic.Right.Name + "'" : logic.Right.Name);
                    }
                    else if (logic.Compare.Type == "$or")
                    {
                        if (logic.Operator.Type == ":")
                            conditions |= Builders<object>.Filter.Eq(logic.Left.Name.IndexOf(".") > -1 ? "'" + logic.Left.Name + "'" : logic.Left.Name, logic.Right.Type == "string" ? "'" + logic.Right.Name + "'" : logic.Right.Name);
                        if (logic.Operator.Type == "$gt")
                            conditions |= Builders<object>.Filter.Gt(logic.Left.Name.IndexOf(".") > -1 ? "'" + logic.Left.Name + "'" : logic.Left.Name, logic.Right.Type == "string" ? "'" + logic.Right.Name + "'" : logic.Right.Name);
                        if (logic.Operator.Type == "$gte")
                            conditions |= Builders<object>.Filter.Gte(logic.Left.Name.IndexOf(".") > -1 ? "'" + logic.Left.Name + "'" : logic.Left.Name, logic.Right.Type == "string" ? "'" + logic.Right.Name + "'" : logic.Right.Name);
                        if (logic.Operator.Type == "$lt")
                            conditions |= Builders<object>.Filter.Lt(logic.Left.Name.IndexOf(".") > -1 ? "'" + logic.Left.Name + "'" : logic.Left.Name, logic.Right.Type == "string" ? "'" + logic.Right.Name + "'" : logic.Right.Name);
                        if (logic.Operator.Type == "$lte")
                            conditions |= Builders<object>.Filter.Lte(logic.Left.Name.IndexOf(".") > -1 ? "'" + logic.Left.Name + "'" : logic.Left.Name, logic.Right.Type == "string" ? "'" + logic.Right.Name + "'" : logic.Right.Name);
                    }
                }
            }

            var result = MongoExtensions.RenderToBsonDocument(conditions).ToJson().Replace("\"", "");

            _query.Append(result);
        }

        public void Visit(LogicalPart part)
        {
            //string[] types = { "$gt", "$gte", "$lt", "$lte" };
            //_query.Append("\"" + part.Left.Name + "\"");

            //if (types.Contains(part.Operator.Type))
            //{
            //    _query.Append(": { ");
            //    _query.Append( part.Operator.Type + " : ");

            //    if (part.Right.Type == "string")
            //        _query.Append("\"" + part.Right.Name + "\"" );
            //    else if (part.Right.Type == "int")
            //        _query.Append("NumberLong(" + part.Right.Name + ")") ;
            //    else
            //        _query.Append(part.Right.Name);

            //    _query.Append(" } ");
            //}
            //else
            //{
            //    _query.Append(" " + part.Operator.Type + " ");

            //    if (part.Right.Type == "string")
            //        _query.Append("\"" + part.Right.Name + "\"");
            //    else
            //        _query.Append(part.Right.Name);
            //}
        }

        public void Visit(OperatorPart part)
        {
            _query.Append(" " + part.Type + " ");
        }

        public void Visit(ComparePart part)
        {
            _query.Append("'" + part.Type + "' : ");
        }

        public void Visit(FieldPart part)
        {
            _query.Append("projection: {");

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

            _query.Append("}");
        }

        public void Visit(PropertyPart part)
        {
            _query.Append(part.Name);
        }

        public void Visit(UnwindArrayPart part)
        {
            _query.Append("path: '$" + part.Name + "'");
        }

        public void Visit(OrderByPart part)
        {
            if (_format == MongoDBFetchType.Aggregate)
            {
                _query.Append("$sort: {");
            }
            else
            {
                _query.Append("sort: {");
            }
                

            foreach (var field in part.Fields)
                field.Accept(this);

            _query.Append("}");

            //if (_format == MongoDBFetchType.Aggregate)
            //{
            //    _query.Append(", { $sort : { ");

            

            //    _query.Append(" } }");
            //}

            //else
            //{
            //    _query.Append("._addSpecial(\"$orderby\", {" );

            //    foreach (var field in part.Fields)
            //        field.Accept(this);

            //    _query.Append("})");
            //}
        }

        public void Visit(OrderByPropertyPart part)
        {
            _query.Append(part.Name + " ");

            part.Direction.Accept(this);
        }

        public void Visit(RestrictPart part)
        {
            if (_format == MongoDBFetchType.Aggregate)
            {
                _query.Append("$limit: " + part.Limit);
            }
            else
            {
                _query.Append("limit : " + part.Limit);
            }

            
            //if (_format == MongoDBFetchType.Aggregate)
            //    _query.Append(", { $limit : " + part.Limit + " }");
            //else
            //    _query.Append(".limit(" + part.Limit.ToString() + ")");
        }

        public void Visit(SeparatorPart part)
        {
            _query.Append(part.Delimiter + " ");
        }

        public void Visit(DirectionPart part)
        {
            //sort: { name: 1 }
            if (part.Type == "DESC")
                _query.Append(" : -1 ");
            else
                _query.Append(" : 1 ");
        }
    }
}